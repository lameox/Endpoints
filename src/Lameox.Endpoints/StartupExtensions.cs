using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lameox.Endpoints
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            return services;
        }

        public static IApplicationBuilder UseEndpoints(this IApplicationBuilder app)
        {
            return app.UseMiddleware<EndpointExecutionMiddleware>();
        }
    }
}
