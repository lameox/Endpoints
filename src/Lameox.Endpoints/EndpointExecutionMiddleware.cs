using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    internal class EndpointExecutionMiddleware
    {
        private readonly RequestDelegate _nextMiddleware;

        public EndpointExecutionMiddleware(RequestDelegate nextMiddleware)
        {
            _nextMiddleware = nextMiddleware ?? throw new ArgumentNullException(nameof(nextMiddleware));
        }

        public Task InvokeAsync(HttpContext ctx)
        {
            return _nextMiddleware(ctx);
        }
    }
}
