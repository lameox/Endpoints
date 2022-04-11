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
            JsonSerializerOptions? serializerOptions = null,
            CancellationToken cancellationToken = default)
            where TRequest : notnull, new()
        {
            if (!httpRequest.HasJsonContentType())
            {
                return new TRequest();
            }

            if (serializerOptions is null)
            {
                return await JsonSerializer.DeserializeAsync<TRequest>(httpRequest.Body, cancellationToken: cancellationToken);
            }

            return await JsonSerializer.DeserializeAsync<TRequest>(httpRequest.Body, serializerOptions, cancellationToken: cancellationToken);
        }
    }
}
