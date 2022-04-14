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
    internal static class ValueParser<TValue>
    {
        public delegate bool TryParseValueDelegate(string input, [NotNullWhen(true)] out TValue? value);

        [MemberNotNullWhen(true, nameof(TryParseValue))]
        public static bool HasParser => TryParseValue is not null;
        public static TryParseValueDelegate? TryParseValue { get; } = CreateParser();

        private static TryParseValueDelegate? CreateParser()
        {
            if (typeof(TValue) == typeof(string))
            {
                return StringParser;
            }

            if (typeof(TValue).IsEnum)
            {
                return EnumParser;
            }

            if (typeof(TValue).IsInterface)
            {
                //we can never parse interfaces since we dont know which implementing class to call
                return null;
            }

            if (typeof(TValue) == typeof(Uri))
            {
                return UriParser;
            }

            return CompileTryParseOnTargetType();
        }

        private static bool StringParser(string input, [NotNullWhen(true)] out TValue? value)
        {
            if (typeof(TValue) != typeof(string))
            {
                throw ExceptionUtilities.Unreachable();
            }

            value = (TValue)(object)input;
            return true;
        }

        private static bool UriParser(string input, [NotNullWhen(true)] out TValue? value)
        {
            if (typeof(TValue) != typeof(Uri))
            {
                throw ExceptionUtilities.Unreachable();
            }

            value = (TValue)(object)new Uri(input);
            return true;
        }

        private static bool EnumParser(string input, [NotNullWhen(true)] out TValue? value)
        {
            if (Enum.TryParse(typeof(TValue), input, out var result))
            {
                value = (TValue)(object)result!;
                return value is not null;
            }

            value = default;
            return false;
        }

        private static TryParseValueDelegate? CompileTryParseOnTargetType()
        {
            var signature = new[] { typeof(string), typeof(TValue).MakeByRefType() };

            var tryParseMethod =
                typeof(TValue).FindFactoryMethodWithNameAndSignature("TryParse", signature, typeof(bool)) ??
                typeof(TValue).FindFactoryMethodWithNameAndSignature("TryCreate", signature, typeof(bool)) ??
                typeof(TValue).FindFactoryMethodWithNameAndSignature("TryDeserialize", signature, typeof(bool));

            if (tryParseMethod is null)
            {
                return CompileFromStringOnTargetType();
            }

            //we compile the following pseudocode:
            //bool TryParse(string input, out TValue? value)
            //{
            //    var success = TYPE.TryParse(input, out var result);
            //    if(success)
            //    {
            //        value = result;
            //    }
            //    
            //    return sucess;
            //}

            var method = new DynamicMethod(
                $"{nameof(ValueParser<TValue>)}_{typeof(TValue).FullName}",
                typeof(bool),
                new Type[] { typeof(string), typeof(TValue).MakeByRefType() });

            var il = method.GetILGenerator();

            il.DeclareLocal(typeof(TValue));            //result of TryParse
            il.DeclareLocal(typeof(bool));          //return value of TryParse

            il.Emit(OpCodes.Ldarg_0);               //load the string input
            il.Emit(OpCodes.Ldloca_S, 0);           //load the address of our result local
            il.EmitCall(OpCodes.Call, tryParseMethod, null);

            il.Emit(OpCodes.Stloc_1);               //store the boolean success in its local

            il.Emit(OpCodes.Ldarg_1);               //load the output argument for use in Stind_Ref
            il.Emit(OpCodes.Ldloc_0);               //load the result

            il.Emit(OpCodes.Stind_Ref);             //store the result in the result variable

            il.Emit(OpCodes.Ldloc_1);               //load the boolean success again before leaving the method.
            il.Emit(OpCodes.Ret);

            return method.CreateDelegate<TryParseValueDelegate>();
        }

        private static TryParseValueDelegate? CompileFromStringOnTargetType()
        {
            var signature = new[] { typeof(string) };

            var fromStringMethod =
                typeof(TValue).FindFactoryMethodWithNameAndSignature("FromString", signature, typeof(TValue)) ??
                typeof(TValue).FindFactoryMethodWithNameAndSignature("Parse", signature, typeof(TValue)) ??
                typeof(TValue).FindFactoryMethodWithNameAndSignature("Deserialize", signature, typeof(TValue));

            ConstructorInfo? constructor = null;

            if (fromStringMethod is null)
            {
                constructor = typeof(TValue).GetConstructor(signature);
            }

            if (fromStringMethod is null && constructor is null)
            {
                return null;
            }


            //we compile the following pseudocode:
            //bool TryParse(string input, out TValue? value)
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
            //bool TryParse(string input, out TValue? value)
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

            var method = new DynamicMethod(
                $"{nameof(ValueParser<TValue>)}_{typeof(TValue).FullName}",
                typeof(bool),
                new Type[] { typeof(string), typeof(TValue).MakeByRefType() });

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