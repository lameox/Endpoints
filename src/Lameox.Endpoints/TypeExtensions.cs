using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    internal static class TypeExtensions
    {
        public static bool IsDerivedFromGenericBase(this Type type, Type genericBaseType)
        {
            if (type == genericBaseType)
            {
                return true;
            }

            if (!genericBaseType.IsGenericType || genericBaseType.IsConstructedGenericType)
            {
                return type.IsSubclassOf(genericBaseType);
            }

            var current = type.BaseType;

            while (current != null)
            {
                if (current.IsConstructedGenericType && current.GetGenericTypeDefinition() == genericBaseType)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        public static bool IsMethodOverridden(
            this Type type,
            MethodInfo methodDefinedOnBaseType)
        {
            if (methodDefinedOnBaseType.DeclaringType!.IsGenericType)
            {
                return IsMethodOverriddenOnGenericBaseType(type, methodDefinedOnBaseType.GetBaseDefinition());
            }

            if (!type.IsSubclassOf(methodDefinedOnBaseType.DeclaringType))
            {
                return false;
            }

            var method = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod)
                .Single(m => m.GetBaseDefinition() == methodDefinedOnBaseType);

            return method != methodDefinedOnBaseType;
        }

        private static bool IsMethodOverriddenOnGenericBaseType(Type type, MethodInfo methodDefinedOnBaseType)
        {
            var genericBaseType = methodDefinedOnBaseType.DeclaringType;

            if (!type.IsDerivedFromGenericBase(genericBaseType!))
            {
                return false;
            }

            var method = type
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod)
                .Single(m =>
                {
                    var baseDefinition = m.GetBaseDefinition();
                    if (baseDefinition == m)
                    {
                        return false;
                    }

                    var definingType = m.GetBaseDefinition().DeclaringType;

                    if (definingType is null ||
                        !definingType.IsGenericType ||
                        !definingType.IsConstructedGenericType ||
                        definingType.GetGenericTypeDefinition() != genericBaseType)
                    {
                        return false;
                    }

                    if (MethodBase.GetMethodFromHandle(
                        baseDefinition.MethodHandle,
                        genericBaseType.TypeHandle) is not MethodInfo info)
                    {
                        return false;
                    }

                    return info == methodDefinedOnBaseType;
                });

            return
                !method.DeclaringType!.IsConstructedGenericType ||
                method.DeclaringType.GetGenericTypeDefinition() != genericBaseType;
        }

        public static MethodInfo? FindFactoryMethodWithNameAndSignature(this Type type, string methodName, Type[] signature, Type returnType)
        {
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, signature);

            if (method is null || method.ReturnType != returnType)
            {
                return null;
            }

            return method;
        }
    }
}
