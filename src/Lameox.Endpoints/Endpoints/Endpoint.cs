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
        internal static bool IsHandleAsyncOverridden(Type endpointType)
        {
            var handleAsyncMethod = typeof(Endpoint)
                .GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod)!;

            return endpointType.IsMethodOverridden(handleAsyncMethod);
        }

        internal static bool IsGetResponseAsyncOverridden(Type endpointType)
        {
            return false;
        }

        private EndpointImplementation<NoRequest, NoResponse>? _implementation;
        private EndpointImplementation<NoRequest, NoResponse> Implementation => _implementation ??= Initialize();

        public EndpointConfiguration Configuration => Implementation.Configuration;
        public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
        {
            return Implementation.HandleRequestAsync(requestContext, cancellationToken);
        }

        private EndpointImplementation<NoRequest, NoResponse> Initialize()
        {
            EnsureCorrectOverrides(GetType(), IsHandleAsyncOverridden, IsGetResponseAsyncOverridden, out var _);

            return new EndpointImplementation<NoRequest, NoResponse>(
                Configure,
                CallableHandleAsync);
        }

        protected abstract EndpointConfiguration Configure(EndpointConfiguration configuration);
        protected abstract ValueTask HandleAsync(CancellationToken cancellationToken);

        private ValueTask CallableHandleAsync(NoRequest request, CancellationToken cancellationToken)
        {
            return HandleAsync(cancellationToken);
        }
    }
}
