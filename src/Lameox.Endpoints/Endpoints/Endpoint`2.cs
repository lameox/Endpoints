using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    public abstract class Endpoint<TRequest, TResponse> : EndpointBase<TResponse>
        where TRequest : notnull, new()
        where TResponse : notnull, new()
    {
        private static MethodInfo? s_handleAsyncMethod;
        internal static bool IsHandleAsyncOverridden(Type endpointType)
        {
            s_handleAsyncMethod ??= typeof(Endpoint<,>)
                .GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod)!;

            return endpointType.IsMethodOverridden(s_handleAsyncMethod);
        }

        private static MethodInfo? s_getResponseAsyncMethod;
        internal static bool IsGetResponseAsyncOverridden(Type endpointType)
        {
            s_getResponseAsyncMethod ??= typeof(Endpoint<,>)
                .GetMethod(nameof(GetResponseAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod)!;

            return endpointType.IsMethodOverridden(s_getResponseAsyncMethod);
        }

        protected override async ValueTask HandleRequestAsync(
            HttpContext requestContext,
            EndpointDescription endpointDescription,
            CancellationToken cancellationToken)
        {
            var request = GetRequestObjectFromRequest(requestContext, endpointDescription);

            if (endpointDescription.ImplementsHandleAsync)
            {
                await HandleAsync(request, cancellationToken);
                return;
            }

            var response = await GetResponseAsync(request, cancellationToken);
            SetResponse(response);

            await SendResponseIfNoneSentYetAsync(requestContext, cancellationToken);
        }

        private TRequest GetRequestObjectFromRequest(HttpContext requestContext, EndpointDescription endpointDescription)
        {
            throw new NotImplementedException();
        }

        protected virtual ValueTask HandleAsync(TRequest request, CancellationToken cancellationToken)
        {
            throw ExceptionUtilities.Unreachable();
        }

        protected virtual ValueTask<TResponse> GetResponseAsync(TRequest request, CancellationToken cancellationToken)
        {
            throw ExceptionUtilities.Unreachable();
        }
    }
}
