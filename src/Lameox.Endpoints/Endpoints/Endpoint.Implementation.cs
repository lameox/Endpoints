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

                var request = await GetRequestObjectFromRequestAsync(requestContext, endpointDescription, cancellationToken);

                if (_handleAsync is not null)
                {
                    await _handleAsync(request, cancellationToken);
                    return;
                }

                var response = await _getResponseAsync!(request, cancellationToken);
                SetResponse(response);

                await SendResponseIfNoneSentYetAsync(requestContext, cancellationToken);
            }

            private static async ValueTask<TRequest> GetRequestObjectFromRequestAsync(HttpContext requestContext, EndpointDescription endpointDescription, CancellationToken cancellationToken)
            {
                if (typeof(TRequest) == typeof(NoRequest))
                {
                    return (TRequest)(object)default(NoRequest);
                }

                if (typeof(TRequest) == typeof(PlainTextRequest))
                {
                    using var streamReader = new StreamReader(requestContext.Request.Body);
                    var bodyText = await streamReader.ReadToEndAsync();
                    return (TRequest)(object)new PlainTextRequest { Text = bodyText };
                }

                return await DeserializeAndBindRequestAsync(requestContext, endpointDescription, cancellationToken);
            }

            private static async ValueTask<TRequest> DeserializeAndBindRequestAsync(HttpContext requestContext, EndpointDescription endpointDescription, CancellationToken cancellationToken)
            {
                var request = await requestContext.Request.GetRequestObjectAsync<TRequest>(cancellationToken: cancellationToken);

                if (request is null)
                {
                    throw ExceptionUtilities.UnableToDeserializeRequest(endpointDescription.EndpointType);
                }

                var failures = await Binder<TRequest>.BindRequestValuesAsync(ref request, requestContext, endpointDescription, cancellationToken);

                if (failures.Any())
                {
                    throw ExceptionUtilities.BindingFailed(endpointDescription.EndpointType, failures);
                }

                return request;
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

                if (typeof(TResponse) == typeof(string))
                {
                    await requestContext.Response.SendTextResponseAsync((string)(object)response, cancellationToken: cancellationToken);
                    return;
                }

                await requestContext.Response.SendJsonResponseAsync(response, cancellationToken: cancellationToken);
            }
        }
    }
}
