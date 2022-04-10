using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Lameox.Endpoints
{
    public abstract class EndpointBase<TResponse> : IEndpoint
        where TResponse : notnull
    {
        private TResponse? _response;

        private EndpointConfiguration? _lazyConfiguration;
        public EndpointConfiguration Configuration => _lazyConfiguration ??= InitializeConfiguration();

        protected abstract EndpointConfiguration Configure(EndpointConfiguration configuration);
        private EndpointConfiguration InitializeConfiguration()
        {
            return Configure(new EndpointConfiguration());
        }

        public ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
        {
            var endpointFeature = requestContext.Features.Get<IEndpointFeature>();

            if (endpointFeature?.Endpoint is null)
            {
                throw ExceptionUtilities.Unreachable();
            }

            var endpointDescription = endpointFeature.Endpoint.Metadata.GetMetadata<EndpointDescription>();

            if (endpointDescription is null)
            {
                throw ExceptionUtilities.Unreachable();
            }

            return HandleRequestAsync(requestContext, endpointDescription, cancellationToken);
        }

        protected abstract ValueTask HandleRequestAsync(
            HttpContext requestContext,
            EndpointDescription endpointDescription,
            CancellationToken cancellationToken);

        protected void SetResponse(TResponse? response)
        {
            _response = response;
        }

        protected async ValueTask SendResponseIfNoneSentYetAsync(HttpContext requestContext, CancellationToken cancellationToken = default)
        {
            if (requestContext.Response.IsResponseSending())
            {
                //response got send somewhere along the way already.
                return;
            }

            var response = _response;

            if (response is null)
            {
                await requestContext.Response.SendEmptyReponseAsync(cancellationToken);
                return;
            }

            await requestContext.Response.SendJsonResponseAsync(response, cancellationToken: cancellationToken);
        }
    }
}
