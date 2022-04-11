using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Lameox.Endpoints
{
    public abstract partial class Endpoint
    {
        private sealed class Implementation<TRequest, TResponse> : IEndpoint
            where TRequest : notnull, new()
            where TResponse : notnull
        {
            private readonly Func<TRequest, CancellationToken, ValueTask>? _handleAsync;
            private readonly Func<TRequest, CancellationToken, ValueTask<TResponse>>? _getResponseAsync;
            private readonly Func<EndpointConfiguration, EndpointConfiguration> _configure;

            private TResponse? _response;

            private EndpointConfiguration? _lazyConfiguration;

            public Implementation(
                Func<EndpointConfiguration, EndpointConfiguration> configure,
                Func<TRequest, CancellationToken, ValueTask> handleAsync)
            {
                _handleAsync = handleAsync;
                _configure = configure;
            }

            public Implementation(
                Func<EndpointConfiguration, EndpointConfiguration> configure,
                Func<TRequest, CancellationToken, ValueTask<TResponse>> getResponseAsync)
            {
                _getResponseAsync = getResponseAsync;
                _configure = configure;
            }

            public EndpointConfiguration Configuration => _lazyConfiguration ??= InitializeConfiguration();

            private EndpointConfiguration InitializeConfiguration()
            {
                return _configure(new EndpointConfiguration());
            }

            public async ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken)
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

                var request = GetRequestObjectFromRequest(requestContext, endpointDescription);

                if (_handleAsync is not null)
                {
                    await _handleAsync(request, cancellationToken);
                    return;
                }

                var response = await _getResponseAsync!(request, cancellationToken);
                SetResponse(response);

                await SendResponseIfNoneSentYetAsync(requestContext, cancellationToken);
            }

            private static TRequest GetRequestObjectFromRequest(HttpContext requestContext, EndpointDescription endpointDescription)
            {
                if (typeof(TRequest) == typeof(NoRequest))
                {
                    return (TRequest)(object)default(NoRequest);
                }

                _ = requestContext;
                _ = endpointDescription;

                throw new NotImplementedException();
            }

            private void SetResponse(TResponse? response)
            {
                _response = response;
            }

            private async ValueTask SendResponseIfNoneSentYetAsync(HttpContext requestContext, CancellationToken cancellationToken = default)
            {
                if (requestContext.Response.IsResponseSending())
                {
                    //response got sent somewhere along the way already.
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
}
