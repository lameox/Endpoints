using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Lameox.Endpoints
{
    internal class EndpointExecutionMiddleware
    {
        private readonly RequestDelegate _nextMiddleware;

        public EndpointExecutionMiddleware(RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware ?? throw new ArgumentNullException(nameof(nextMiddleware));
        }

        public async Task InvokeAsync(HttpContext requestContext)
        {
            var endpointFeature = requestContext.Features.Get<IEndpointFeature>();
            var endpointDescription = endpointFeature?.Endpoint?.Metadata.GetMetadata<EndpointDescription>();

            if (endpointDescription is null)
            {
                await _nextMiddleware(requestContext);
                return;
            }

            var instance = requestContext.RequestServices.GetService(endpointDescription.EndpointType);

            if (instance is null || instance is not IEndpoint endpoint)
            {
                throw ExceptionUtilities.Unreachable();
            }

            await endpoint.HandleRequestAsync(requestContext, requestContext.RequestAborted);
        }
    }
}
