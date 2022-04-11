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

            private readonly Implementation<TRequest, NoResponse> _implementation;

            public EndpointConfiguration Configuration => ((IEndpoint)_implementation).Configuration;
            public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
            {
                return ((IEndpoint)_implementation).HandleRequestAsync(requestContext, cancellationToken);
            }

            protected WithRequest()
            {
                EnsureCorrectOverrides(GetType(), IsHandleAsyncOverridden, IsGetResponseAsyncOverridden, out var useHandleAsyncInImplementation);

                _implementation = new Implementation<TRequest, NoResponse>(
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
