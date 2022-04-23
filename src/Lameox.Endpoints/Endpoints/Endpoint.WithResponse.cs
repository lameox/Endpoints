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
        public abstract class WithResponse<TResponse> : IEndpoint
            where TResponse : notnull
        {
            internal static bool IsHandleAsyncOverridden(Type endpointType)
            {
                var handleAsyncMethod = typeof(WithResponse<>)
                    .GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod)!;

                return endpointType.IsMethodOverridden(handleAsyncMethod);
            }

            internal static bool IsGetResponseAsyncOverridden(Type endpointType)
            {
                var getResponseAsyncMethod = typeof(WithResponse<>)
                    .GetMethod(nameof(GetResponseAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod)!;

                return endpointType.IsMethodOverridden(getResponseAsyncMethod);
            }

            private EndpointImplementation<NoRequest, TResponse>? _implementation;
            private EndpointImplementation<NoRequest, TResponse> Implementation => _implementation ??= Initialize();

            public EndpointConfiguration Configuration => Implementation.Configuration;
            public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
            {
                return Implementation.HandleRequestAsync(requestContext, cancellationToken);
            }

            private EndpointImplementation<NoRequest, TResponse> Initialize()
            {
                EnsureCorrectOverrides(GetType(), IsHandleAsyncOverridden, IsGetResponseAsyncOverridden, out var useHandleAsyncInImplementation);

                if (useHandleAsyncInImplementation)
                {
                    return new EndpointImplementation<NoRequest, TResponse>(
                        Configure,
                        CallableHandleAsync);
                }
                else
                {
                    return new EndpointImplementation<NoRequest, TResponse>(
                        Configure,
                        CallableGetResponseAsync);
                }
            }

            protected abstract EndpointConfiguration Configure(EndpointConfiguration configuration);

            private ValueTask CallableHandleAsync(NoRequest request, CancellationToken cancellationToken)
            {
                return HandleAsync(cancellationToken);
            }

            private ValueTask<TResponse> CallableGetResponseAsync(NoRequest request, CancellationToken cancellationToken)
            {
                return GetResponseAsync(cancellationToken);
            }

            protected virtual ValueTask HandleAsync(CancellationToken cancellationToken)
            {
                throw ExceptionUtilities.DontCallBaseMethodsInHandlers(GetType());
            }

            protected virtual ValueTask<TResponse> GetResponseAsync(CancellationToken cancellationToken)
            {
                throw ExceptionUtilities.DontCallBaseMethodsInHandlers(GetType());
            }
        }
    }
}
