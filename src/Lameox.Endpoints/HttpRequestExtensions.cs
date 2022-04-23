using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    internal static class HttpRequestExtensions
    {
        public static async ValueTask<TRequest?> GetRequestObjectAsync<TRequest>(
            this HttpRequest httpRequest,
            JsonSerializerOptions serializerOptions,
            JsonSerializerContext? serializerContext = null,
            CancellationToken cancellationToken = default)
            where TRequest : notnull, new()
        {
            if (!httpRequest.HasJsonContentType())
            {
                return new TRequest();
            }

            if (serializerContext is null)
            {
                return await JsonSerializer.DeserializeAsync<TRequest>(httpRequest.Body, serializerOptions, cancellationToken: cancellationToken);
            }

            var result = await JsonSerializer.DeserializeAsync(httpRequest.Body, typeof(TRequest), serializerContext, cancellationToken: cancellationToken);
            return (TRequest)result!;
        }
    }
}
