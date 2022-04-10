using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    public abstract partial class Endpoint : IEndpoint
    {
        private static readonly Dictionary<Type, bool> s_implementsHandleAsync = new();
        private static readonly Dictionary<Type, bool> s_implementsGetResponseAsync = new();

        internal static void EnsureCorrectOverrides(
            Type endpointType,
            Func<Type, bool> isHandleAsyncOverridden,
            Func<Type, bool> isGetResponseAsyncOverridden,
            out bool useHandleAsyncInImplementation)
        {
            if (!s_implementsHandleAsync.TryGetValue(endpointType, out var implementsHandleAsync))
            {
                implementsHandleAsync = isHandleAsyncOverridden(endpointType);
                s_implementsHandleAsync[endpointType] = implementsHandleAsync;
            }

            if (!s_implementsGetResponseAsync.TryGetValue(endpointType, out var implementsGetResponseAsync))
            {
                implementsHandleAsync = isGetResponseAsyncOverridden(endpointType);
                s_implementsGetResponseAsync[endpointType] = implementsHandleAsync;
            }

            if (implementsHandleAsync && implementsGetResponseAsync)
            {
                throw ExceptionUtilities.BadOverrideInEndpoint();
            }

            if (!implementsHandleAsync && !implementsGetResponseAsync)
            {
                throw ExceptionUtilities.BadOverrideInEndpoint();
            }

            useHandleAsyncInImplementation = implementsHandleAsync;
        }
    }
}
