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


            private EndpointImplementation<PlainTextRequest, NoResponse>? _implementation;
            private EndpointImplementation<PlainTextRequest, NoResponse> Implementation => _implementation ??= Initialize();

            public EndpointConfiguration Configuration => Implementation.Configuration;
            public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
            {
                return Implementation.HandleRequestAsync(requestContext, cancellationToken);
            }

            private EndpointImplementation<PlainTextRequest, NoResponse> Initialize()
            {
                EnsureCorrectOverrides(GetType(), IsHandleAsyncOverridden, IsGetResponseAsyncOverridden, out var useHandleAsyncInImplementation);

                return new EndpointImplementation<PlainTextRequest, NoResponse>(
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
