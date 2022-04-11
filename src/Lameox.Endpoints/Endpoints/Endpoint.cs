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

        private readonly Implementation<NoRequest, NoResponse> _implementation;

        public EndpointConfiguration Configuration => ((IEndpoint)_implementation).Configuration;
        public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
        {
            return ((IEndpoint)_implementation).HandleRequestAsync(requestContext, cancellationToken);
        }

        protected Endpoint()
        {
            EnsureCorrectOverrides(GetType(), IsHandleAsyncOverridden, IsGetResponseAsyncOverridden, out var _);

            _implementation = new Implementation<NoRequest, NoResponse>(
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
