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
    internal static class HttpResponseExtensions
    {
        private static readonly object ReponseSendingMarker = new();
        public static void MarkAsResponseSending(this HttpResponse response)
        {
            response.HttpContext.Items.TryAdd(ReponseSendingMarker, null);
        }

        public static bool IsResponseSending(this HttpResponse response)
        {
            return response.HasStarted || response.HttpContext.Items.ContainsKey(ReponseSendingMarker);
        }

        private static CancellationToken GetCancellationToken(this HttpResponse response, CancellationToken cancellationToken = default)
        {
            if (cancellationToken == default)
            {
                return response.HttpContext.RequestAborted;
            }

            return cancellationToken;
        }

        private static async ValueTask SendReponseAsync(this HttpResponse response, CancellationToken cancellationToken = default)
        {
            cancellationToken = response.GetCancellationToken(cancellationToken);
            response.MarkAsResponseSending();
            await response.StartAsync(cancellationToken);
        }

        public static async ValueTask SendEmptyReponseAsync(this HttpResponse response, CancellationToken cancellationToken = default)
        {
            response.StatusCode = StatusCodes.Status204NoContent;
            await response.SendReponseAsync(cancellationToken);
        }

        public static async ValueTask SendTextResponseAsync(
            this HttpResponse httpResponse,
            string response,
            int statusCode = StatusCodes.Status200OK,
            string contentType = System.Net.Mime.MediaTypeNames.Text.Plain,
            CancellationToken cancellationToken = default)
        {
            cancellationToken = httpResponse.GetCancellationToken(cancellationToken);

            httpResponse.StatusCode = statusCode;
            httpResponse.ContentType = contentType;

            httpResponse.MarkAsResponseSending();

            await httpResponse.WriteAsync(response, cancellationToken);
        }

        public static async ValueTask SendJsonResponseAsync<TResponse>(
            this HttpResponse httpResponse,
            TResponse response,
            int statusCode = StatusCodes.Status200OK,
            string contentType = System.Net.Mime.MediaTypeNames.Application.Json,
            JsonSerializerOptions? serializerOptions = null,
            CancellationToken cancellationToken = default)
            where TResponse : notnull
        {
            cancellationToken = httpResponse.GetCancellationToken(cancellationToken);

            httpResponse.StatusCode = statusCode;
            httpResponse.ContentType = contentType;

            httpResponse.MarkAsResponseSending();

            if (serializerOptions is null)
            {
                await JsonSerializer.SerializeAsync<TResponse>(httpResponse.Body, response, cancellationToken: cancellationToken);
                return;
            }

            await JsonSerializer.SerializeAsync<TResponse>(httpResponse.Body, response, serializerOptions, cancellationToken: cancellationToken);
        }

        public static async ValueTask SendGeneralServerErrorAsync(this HttpResponse response, CancellationToken cancellationToken = default)
        {
            response.StatusCode = StatusCodes.Status500InternalServerError;
            await response.SendReponseAsync(cancellationToken);
        }

        public static async ValueTask SendGeneralServerErrorAsync(this HttpResponse response, string errorMessage, CancellationToken cancellationToken = default)
        {
            await response.SendTextResponseAsync(
                errorMessage,
                statusCode: StatusCodes.Status500InternalServerError,
                cancellationToken: cancellationToken);
        }
    }
}
