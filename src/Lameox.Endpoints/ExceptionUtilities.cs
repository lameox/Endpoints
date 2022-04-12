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

        internal static Exception UnableToDeserializeRequest(Type endpointType)
        {
            throw new InvalidOperationException($"Failed to deserialize the request object for endpoint {endpointType.FullName}.");
        }

        internal static Exception BindingFailed(Type endpointType, ImmutableArray<BindingFailure> failures)
        {
            throw new InvalidOperationException($"Failed to bind request values for endpoint {endpointType.FullName}:{Environment.NewLine}" +
                                                $"{string.Join(Environment.NewLine, failures.Select((f, i) => $"({i}): {f.DisplayString}"))}");
        }
    }
}
