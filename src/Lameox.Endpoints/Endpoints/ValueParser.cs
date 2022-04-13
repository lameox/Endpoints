using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    internal static class ValueParser
    {
        public delegate bool TryParseValueDelegate(string input, [NotNullWhen(true)] out object? value);
        public static TryParseValueDelegate NoParser { get; } = NoParserImpl;
        private static bool NoParserImpl(string input, [NotNullWhen(true)] out object? value)
        {
            value = null!;
            return false;
        }

        private static ImmutableDictionary<Type, TryParseValueDelegate> Cache = ImmutableDictionary<Type, TryParseValueDelegate>.Empty;

        public static TryParseValueDelegate Get<TTargetType>()
        {
            return Get(typeof(TTargetType));
        }

        public static TryParseValueDelegate Get(Type targetType)
        {
            return ImmutableInterlocked.GetOrAdd(ref Cache, targetType, CreateValueParserForType);
        }

        private static TryParseValueDelegate CreateValueParserForType(Type targetType)
        {
            if (targetType == typeof(string))
            {
                return StringParser;
            }

            if (targetType.IsEnum)
            {
                return EnumParser(targetType);
            }

            if (targetType == typeof(Uri))
            {
                return UriParser;
            }

            return CompileTryParseOnTargetType(targetType);
        }

        private static bool StringParser(string input, [NotNullWhen(true)] out object? value)
        {
            value = input;
            return true;
        }

        private static bool UriParser(string input, [NotNullWhen(true)] out object? value)
        {
            value = new Uri(input);
            return true;
        }

        private static bool IntParser(string input, [NotNullWhen(true)] out object? value)
        {
            if (int.TryParse(input, out int result))
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }

        private static bool GuidParser(string input, [NotNullWhen(true)] out object? value)
        {
            if (int.TryParse(input, out int result))
            {
                value = result;
                return true;
            }

            value = null;
            return false;
        }

        private static TryParseValueDelegate EnumParser(Type targetType)
        {
            bool EnumParserImpl(string input, [NotNullWhen(true)] out object? value)
            {
                if (Enum.TryParse(targetType, input, out var result))
                {
                    value = result;
                    return value is not null;
                }

                value = null;
                return false;
            }

            return EnumParserImpl;
        }

        private static TryParseValueDelegate CompileTryParseOnTargetType(Type targetType)
        {
            var signature = new[] { typeof(string), targetType.MakeByRefType() };

            var tryParseMethod =
                targetType.FindFactoryMethodWithNameAndSignature("TryParse", signature, typeof(bool)) ??
                targetType.FindFactoryMethodWithNameAndSignature("TryCreate", signature, typeof(bool)) ??
                targetType.FindFactoryMethodWithNameAndSignature("TryDeserialize", signature, typeof(bool));

            if (tryParseMethod is null)
            {
                return CompileFromStringOnTargetType(targetType);
            }

            //we compile the following pseudocode:
            //bool TryParse(string input, out object? value)
            //{
            //    var success = TYPE.TryParse(input, out var result);
            //    if(success)
            //    {
            //        value = result;
            //    }
            //    
            //    return sucess;
            //}

            var method = new DynamicMethod($"{nameof(ValueParser)}_{targetType.FullName}", typeof(bool), new Type[] { typeof(string), typeof(object).MakeByRefType() });
            var il = method.GetILGenerator();

            il.DeclareLocal(targetType);            //result of TryParse
            il.DeclareLocal(typeof(bool));          //return value of TryParse

            il.Emit(OpCodes.Ldarg_0);               //load the string input
            il.Emit(OpCodes.Ldloca_S, 0);           //load the address of our result local
            il.EmitCall(OpCodes.Call, tryParseMethod, null);

            il.Emit(OpCodes.Stloc_1);               //store the boolean success in its local

            il.Emit(OpCodes.Ldarg_1);               //load the output argument for use in Stind_Ref
            il.Emit(OpCodes.Ldloc_0);               //load the result

            if (targetType.IsValueType)             //box value types
            {
                il.Emit(OpCodes.Box, targetType);
            }

            il.Emit(OpCodes.Stind_Ref);             //store the result in the result variable

            il.Emit(OpCodes.Ldloc_1);               //load the boolean success again before leaving the method.
            il.Emit(OpCodes.Ret);

            return method.CreateDelegate<TryParseValueDelegate>();
        }

        private static TryParseValueDelegate CompileFromStringOnTargetType(Type targetType)
        {
            var signature = new[] { typeof(string) };

            var fromStringMethod =
                targetType.FindFactoryMethodWithNameAndSignature("FromString", signature, targetType) ??
                targetType.FindFactoryMethodWithNameAndSignature("Parse", signature, targetType) ??
                targetType.FindFactoryMethodWithNameAndSignature("Deserialize", signature, targetType);

            ConstructorInfo? constructor = null;

            if (fromStringMethod is null)
            {
                constructor = targetType.GetConstructor(signature);
            }

            if (fromStringMethod is null && constructor is null)
            {
                return NoParser;
            }


            //we compile the following pseudocode:
            //bool TryParse(string input, out object? value)
            //{
            //    try
            //    {
            //        value = TYPE.FromString(input);
            //        return value is not null;
            //    }
            //    catch
            //    {
            //        value = null;
            //        return false;
            //    }
            //}
            //
            //or alternatively if only a constructor is found:
            //
            //bool TryParse(string input, out object? value)
            //{
            //    try
            //    {
            //        value = new TYPE(input);
            //        return value is not null;
            //    }
            //    catch
            //    {
            //        value = null;
            //        return false;
            //    }
            //}

            var method = new DynamicMethod($"{nameof(ValueParser)}_{targetType.FullName}", typeof(bool), new Type[] { typeof(string), typeof(object).MakeByRefType() });
            var il = method.GetILGenerator();

            var returnLabel = il.DefineLabel();

            il.DeclareLocal(typeof(bool));          //holds the result

            var tryBlock = il.BeginExceptionBlock();

            il.Emit(OpCodes.Ldarg_1);               //load the output argument for use in Stind_Ref
            il.Emit(OpCodes.Ldarg_0);               //load the string input

            if (fromStringMethod is not null)
            {
                il.EmitCall(OpCodes.Call, fromStringMethod, null);
            }
            else
            {
                il.Emit(OpCodes.Newobj, constructor!);
            }

            if (targetType.IsValueType)             //box value types
            {
                il.Emit(OpCodes.Box, targetType);
            }

            il.Emit(OpCodes.Stind_Ref);             //store the result in the result variable

            il.Emit(OpCodes.Ldarg_1);               //load the output argument for use in Ldind_Ref
            il.Emit(OpCodes.Ldind_Ref);             //store the result in the result variable

            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Ldc_I4_0);              //we want the null check to return true if it is not null. therefore we compare against 0 to invert the result.
            il.Emit(OpCodes.Ceq);
            il.Emit(OpCodes.Stloc_0);               //store the result

            il.Emit(OpCodes.Leave_S, returnLabel);  //leave the try block

            //catch block
            il.BeginCatchBlock(typeof(Exception));

            il.Emit(OpCodes.Ldarg_1);               //load the output argument for use in Stind_Ref
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stind_Ref);             //store the result in the result variable

            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_0);               //store the result
            il.Emit(OpCodes.Leave_S, returnLabel);  //leave the try block

            il.EndExceptionBlock();

            il.MarkLabel(returnLabel);

            il.Emit(OpCodes.Ldloc_0);               //load the stored result
            il.Emit(OpCodes.Ret);

            return method.CreateDelegate<TryParseValueDelegate>();
        }
    }
}