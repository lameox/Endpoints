using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Lameox.Endpoints
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddSimpleEndpoints(this IServiceCollection services, IEnumerable<Assembly>? additionalEndpointAssemblies = null)
        {
            var endpoints = new EndpointDescriptionsBuilder(services, additionalEndpointAssemblies ?? Array.Empty<Assembly>());
            services.AddSingleton(endpoints);

            services.AddHttpContextAccessor();
            return services;
        }

        public static IApplicationBuilder UseSimpleEndpoints(this IApplicationBuilder app)
        {
            return app.UseMiddleware<EndpointExecutionMiddleware>();
        }

        public static IEndpointRouteBuilder MapSimpleEndpoints(this IEndpointRouteBuilder builder)
        {
            var endpointDescriptionContainer = builder.ServiceProvider.GetService<EndpointDescriptionsBuilder>();
            if (endpointDescriptionContainer is null)
            {
                throw ExceptionUtilities.DidNotAddEndpoints();
            }

            foreach (var endpoint in endpointDescriptionContainer.Endpoints)
            {
                MapSingleEndpoint(builder, endpoint);
            }

            return builder;
        }

        private static void MapSingleEndpoint(IEndpointRouteBuilder builder, EndpointDescription endpointDescription)
        {
            var endpoint = builder
                .MapMethods(
                    endpointDescription.Pattern,
                    HttpVerbs.GetMethods(endpointDescription.Verbs),
                    MisconfigurationFallbackAsync)
                .WithMetadata(endpointDescription)
                .AllowAnonymous();
        }

        private static async Task MisconfigurationFallbackAsync(HttpContext context)
        {
            var hostEnvironment = context.RequestServices.GetService<IHostEnvironment>();

            // if we cant know for sure that we are running in a development environment we just return a 500
            // since we dont want to leak implementation details to users.
            if (hostEnvironment is null || !hostEnvironment.IsDevelopment())
            {
                await context.Response.SendGeneralServerErrorAsync();
                return;
            }

            const string Message =
                $"The middleware is misconfigured. Call {nameof(UseSimpleEndpoints)}() after any routing " +
                $"middleware like {nameof(EndpointRoutingApplicationBuilderExtensions.UseRouting)}() " +
                $"and before any terminating middleware like {nameof(EndpointRoutingApplicationBuilderExtensions.UseEndpoints)}().";

            await context.Response.SendGeneralServerErrorAsync(Message);
        }
    }
}
