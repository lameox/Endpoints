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
        public abstract partial class WithRequest<TRequest>
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

                private EndpointImplementation<TRequest, TResponse>? _implementation;
                private EndpointImplementation<TRequest, TResponse> Implementation => _implementation ??= Initialize();

                public EndpointConfiguration Configuration => Implementation.Configuration;
                public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
                {
                    return Implementation.HandleRequestAsync(requestContext, cancellationToken);
                }

                private EndpointImplementation<TRequest, TResponse> Initialize()
                {
                    EnsureCorrectOverrides(GetType(), IsHandleAsyncOverridden, IsGetResponseAsyncOverridden, out var useHandleAsyncInImplementation);

                    if (useHandleAsyncInImplementation)
                    {
                        return new EndpointImplementation<TRequest, TResponse>(
                            Configure,
                            CallableHandleAsync);
                    }
                    else
                    {
                        return new EndpointImplementation<TRequest, TResponse>(
                            Configure,
                            CallableGetResponseAsync);
                    }
                }

                protected abstract EndpointConfiguration Configure(EndpointConfiguration configuration);

                private ValueTask CallableHandleAsync(TRequest request, CancellationToken cancellationToken)
                {
                    return HandleAsync(request, cancellationToken);
                }

                private ValueTask<TResponse> CallableGetResponseAsync(TRequest request, CancellationToken cancellationToken)
                {
                    return GetResponseAsync(request, cancellationToken);
                }

                protected virtual ValueTask HandleAsync(TRequest request, CancellationToken cancellationToken)
                {
                    throw ExceptionUtilities.DontCallBaseMethodsInHandlers(GetType());
                }

                protected virtual ValueTask<TResponse> GetResponseAsync(TRequest request, CancellationToken cancellationToken)
                {
                    throw ExceptionUtilities.DontCallBaseMethodsInHandlers(GetType());
                }
            }
        }
    }
}
