using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    internal static class ExceptionUtilities
    {
        public static Exception Unreachable()
        {
            return new InvalidOperationException("This place in the code is thought to be unreachable.");
        }

        public static Exception UnexpectedValue<TValue>(TValue value)
        {
            return new InvalidOperationException($"Encountered unexpected value of type {typeof(TValue)}: {value}.");
        }

        internal static Exception DidNotAddEndpoints()
        {
            throw new NotImplementedException();
        }

        internal static Exception DontCallBaseMethodsInHandlers(Type endpointType)
        {
            return new InvalidOperationException($"Bad Endpoint {endpointType.FullName}: Don't call base methods in your overrides of the request handling methods.");
        }

        internal static Exception BadOverridesInEndpoint(Type endpointType)
        {
            return new InvalidOperationException($"Bad Endpoint {endpointType.FullName}: You must override exactly one of the request handling methods of the endpoint.");
        }

        internal static Exception NoPermissionClaimType(Type endpointType)
        {
            throw new NotImplementedException($"Bad Endpoint {endpointType.FullName}: The endpoint has specified required permission claims but no permission claim type.");
        }
    }
}
