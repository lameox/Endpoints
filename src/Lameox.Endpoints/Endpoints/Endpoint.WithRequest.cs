using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    public abstract partial class Endpoint
    {
        public abstract partial class WithRequest<TRequest> : IEndpoint
            where TRequest : notnull, new()
        {
            internal static bool IsHandleAsyncOverridden(Type endpointType)
            {
                var handleAsyncMethod = typeof(WithRequest<>)
                    .GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod)!;

                return endpointType.IsMethodOverridden(handleAsyncMethod);
            }

            internal static bool IsGetResponseAsyncOverridden(Type endpointType)
            {
                return false;
            }

            private EndpointImplementation<TRequest, NoResponse>? _implementation;
            private EndpointImplementation<TRequest, NoResponse> Implementation => _implementation ??= Initialize();

            public EndpointConfiguration Configuration => Implementation.Configuration;
            public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
            {
                return Implementation.HandleRequestAsync(requestContext, cancellationToken);
            }

            private EndpointImplementation<TRequest, NoResponse> Initialize()
            {
                EnsureCorrectOverrides(GetType(), IsHandleAsyncOverridden, IsGetResponseAsyncOverridden, out var _);

                return new EndpointImplementation<TRequest, NoResponse>(
                    Configure,
                    CallableHandleAsync);
            }

            protected abstract EndpointConfiguration Configure(EndpointConfiguration configuration);

            private ValueTask CallableHandleAsync(TRequest request, CancellationToken cancellationToken)
            {
                return HandleAsync(request, cancellationToken);
            }

            protected virtual ValueTask HandleAsync(TRequest request, CancellationToken cancellationToken)
            {
                throw ExceptionUtilities.DontCallBaseMethodsInHandlers(GetType());
            }
        }
    }
}
