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
        public abstract partial class WithStringRequest : IEndpoint
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

            private readonly Implementation<PlainTextRequest, NoResponse> _implementation;

            public EndpointConfiguration Configuration => ((IEndpoint)_implementation).Configuration;
            public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
            {
                return ((IEndpoint)_implementation).HandleRequestAsync(requestContext, cancellationToken);
            }

            protected WithStringRequest()
            {
                EnsureCorrectOverrides(GetType(), IsHandleAsyncOverridden, IsGetResponseAsyncOverridden, out var useHandleAsyncInImplementation);

                _implementation = new Implementation<PlainTextRequest, NoResponse>(
                    Configure,
                    CallableHandleAsync);
            }

            protected abstract EndpointConfiguration Configure(EndpointConfiguration configuration);

            private ValueTask CallableHandleAsync(PlainTextRequest request, CancellationToken cancellationToken)
            {
                return HandleAsync(request.Text, cancellationToken);
            }

            protected virtual ValueTask HandleAsync(string request, CancellationToken cancellationToken)
            {
                throw ExceptionUtilities.DontCallBaseMethodsInHandlers(GetType());
            }
        }
    }
}
