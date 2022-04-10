using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    public abstract class Endpoint<TResponse> : EndpointBase<TResponse>
        where TResponse : notnull
    {
        private static MethodInfo? s_handleAsyncMethod;
        internal static bool IsHandleAsyncOverridden(Type endpointType)
        {
            s_handleAsyncMethod ??= typeof(Endpoint<>)
                .GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod)!;

            return endpointType.IsMethodOverridden(s_handleAsyncMethod);
        }

        private static MethodInfo? s_getResponseAsyncMethod;
        internal static bool IsGetResponseAsyncOverridden(Type endpointType)
        {
            s_getResponseAsyncMethod ??= typeof(Endpoint<>)
                .GetMethod(nameof(GetResponseAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod)!;

            return endpointType.IsMethodOverridden(s_getResponseAsyncMethod);
        }

        protected override sealed async ValueTask HandleRequestAsync(
            HttpContext requestContext,
            EndpointDescription endpointDescription,
            CancellationToken cancellationToken)
        {
            if (endpointDescription.ImplementsHandleAsync)
            {
                await HandleAsync(cancellationToken);
                return;
            }

            var response = await GetResponseAsync(cancellationToken);
            SetResponse(response);

            await SendResponseIfNoneSentYetAsync(requestContext, cancellationToken);
        }

        protected virtual ValueTask HandleAsync(CancellationToken cancellationToken)
        {
            throw ExceptionUtilities.Unreachable();
        }

        protected virtual ValueTask<TResponse> GetResponseAsync(CancellationToken cancellationToken)
        {
            throw ExceptionUtilities.Unreachable();
        }
    }
}
