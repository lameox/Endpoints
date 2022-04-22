using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
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
            var metadata = endpointFeature?.Endpoint?.Metadata;

            if (metadata is null)
            {
                await _nextMiddleware(requestContext);
                return;
            }

            var endpointDescription = metadata.GetMetadata<EndpointDescription>();

            if (endpointDescription is null)
            {
                await _nextMiddleware(requestContext);
                return;
            }

            EnsureAuthenticationMiddlewareWasInvokedIfRequired(requestContext, metadata, endpointDescription);
            EnsureCorsMiddlewareWasInvokedIfRequired(requestContext, metadata, endpointDescription);

            var instance = requestContext.RequestServices.GetService(endpointDescription.EndpointType);

            if (instance is null || instance is not IEndpoint endpoint)
            {
                throw ExceptionUtilities.Unreachable();
            }

            await endpoint.HandleRequestAsync(requestContext, requestContext.RequestAborted);
        }

        private static void EnsureAuthenticationMiddlewareWasInvokedIfRequired(HttpContext requestContext, EndpointMetadataCollection metadata, EndpointDescription endpoint)
        {
            const string AuthenticationWasInvokedKey = "__AuthorizationMiddlewareWithEndpointInvoked";

            if (metadata.GetMetadata<IAuthorizeData>() is not null && !requestContext.Items.ContainsKey(AuthenticationWasInvokedKey))
            {
                throw ExceptionUtilities.MissingAuthenticationMiddleware(endpoint.EndpointType);
            }
        }

        private static void EnsureCorsMiddlewareWasInvokedIfRequired(HttpContext requestContext, EndpointMetadataCollection metadata, EndpointDescription endpoint)
        {
            const string CorsWasInvokedKey = "__CorsMiddlewareWithEndpointInvoked";

            if (metadata.GetMetadata<ICorsMetadata>() is not null && !requestContext.Items.ContainsKey(CorsWasInvokedKey))
            {
                throw ExceptionUtilities.MissingCorsMiddleware(endpoint.EndpointType);
            }
        }
    }
}
