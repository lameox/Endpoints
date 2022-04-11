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
            where TRequest : notnull, new()
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

                private readonly Implementation<TRequest, TResponse> _implementation;

                public EndpointConfiguration Configuration => ((IEndpoint)_implementation).Configuration;
                public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
                {
                    return ((IEndpoint)_implementation).HandleRequestAsync(requestContext, cancellationToken);
                }

                protected WithResponse()
                {
                    EnsureCorrectOverrides(GetType(), IsHandleAsyncOverridden, IsGetResponseAsyncOverridden, out var useHandleAsyncInImplementation);

                    if (useHandleAsyncInImplementation)
                    {
                        _implementation = new Implementation<TRequest, TResponse>(
                            Configure,
                            CallableHandleAsync);
                    }
                    else
                    {
                        _implementation = new Implementation<TRequest, TResponse>(
                            Configure,
                            CallableGetResponseAsync);
                    }
                }

                protected abstract EndpointConfiguration Configure(EndpointConfiguration configuration);

                private ValueTask CallableHandleAsync(TRequest request, CancellationToken cancellationToken)
                {
                    return HandleAsync(cancellationToken);
                }

                private ValueTask<TResponse> CallableGetResponseAsync(TRequest request, CancellationToken cancellationToken)
                {
                    return GetResponseAsync(cancellationToken);
                }

                protected virtual ValueTask HandleAsync(CancellationToken cancellationToken)
                {
                    throw ExceptionUtilities.BadOverrideInEndpoint();
                }

                protected virtual ValueTask<TResponse> GetResponseAsync(CancellationToken cancellationToken)
                {
                    throw ExceptionUtilities.BadOverrideInEndpoint();
                }
            }
        }
    }
}
