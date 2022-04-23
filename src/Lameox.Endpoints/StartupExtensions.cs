using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lameox.Endpoints
{
    public static class StartupExtensions
    {
        /// <summary>
        /// Only used to get a logger with a meaningful name
        /// </summary>
        private class SimpleEndpointMiddleware { }

        public static IServiceCollection AddSimpleEndpoints(this IServiceCollection services, IEnumerable<Assembly>? additionalEndpointAssemblies = null)
        {
            var endpoints = new EndpointDescriptionsBuilder(services, additionalEndpointAssemblies ?? Array.Empty<Assembly>());
            services.AddSingleton(endpoints);

            services.AddSingleton(serviceProvider =>
            {
                var serializerOptions = serviceProvider.GetService<IOptions<JsonOptions>>()?.Value.SerializerOptions ?? new();
                return new EndpointSerializationOptions(serializerOptions);
            });

            AddAuthorizationIfRequired(services, endpoints);

            services.AddHttpContextAccessor();
            return services;
        }

        private static void AddAuthorizationIfRequired(IServiceCollection services, EndpointDescriptionsBuilder endpoints)
        {
            if (!endpoints.Endpoints.Any(endpoint => endpoint.RequiresAuthorizationPipeline))
            {
                return;
            }

            services.AddAuthorization(options => AddAuthorizationOptions(options, endpoints));

            static void AddAuthorizationOptions(AuthorizationOptions options, EndpointDescriptionsBuilder endpoints)
            {
                foreach (var endpoint in endpoints.Endpoints)
                {
                    options.AddPolicy(endpoint.GetEndpointPolicyName(), policy =>
                    {
                        policy.RequireAuthenticatedUser();

                        if (!endpoint.Permissions.IsDefaultOrEmpty)
                        {
                            if (string.IsNullOrEmpty(endpoint.PermissionClaimType))
                            {
                                throw ExceptionUtilities.NoPermissionClaimType(endpoint.EndpointType);
                            }

                            if (endpoint.AllPermissionsRequired)
                            {
                                policy.RequireAssertion(context =>
                                    endpoint.Permissions.All(permission =>
                                        context.User.HasClaim(endpoint.PermissionClaimType, permission)));
                            }
                            else
                            {
                                policy.RequireAssertion(context =>
                                        context.User.HasClaim(claim =>
                                            string.Equals(claim.Type, endpoint.PermissionClaimType, StringComparison.OrdinalIgnoreCase) &&
                                            endpoint.Permissions.Contains(claim.Value, StringComparer.Ordinal)));
                            }
                        }


                        if (!endpoint.ClaimTypes.IsDefaultOrEmpty)
                        {
                            if (endpoint.AllClaimTypesRequired)
                            {
                                policy.RequireAssertion(context =>
                                    endpoint.ClaimTypes.All(permission =>
                                        context.User.HasClaim(claim =>
                                            endpoint.ClaimTypes.Contains(claim.Type, StringComparer.Ordinal))));
                            }
                            else
                            {
                                policy.RequireAssertion(context =>
                                    context.User.HasClaim(claim =>
                                        endpoint.ClaimTypes.Contains(claim.Type, StringComparer.Ordinal)));
                            }
                        }
                    });
                }
            }
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
            MapAnonymousEndpointVerbs(builder, endpointDescription);
            MapAuthorizedEndpointVerbs(builder, endpointDescription);
        }

        private static void MapAnonymousEndpointVerbs(IEndpointRouteBuilder builder, EndpointDescription endpointDescription)
        {
            if (endpointDescription.AnonymousVerbs == HttpVerb.None)
            {
                return;
            }

            var endpoint = builder
                            .MapMethods(
                                endpointDescription.Pattern,
                                HttpVerbs.GetMethods(endpointDescription.AnonymousVerbs),
                                MisconfigurationFallbackAsync)
                            .WithMetadata(endpointDescription)
                            .AllowAnonymous();

            MapCommonEndpointProperties(endpoint, endpointDescription);
        }

        private static void MapAuthorizedEndpointVerbs(IEndpointRouteBuilder builder, EndpointDescription endpointDescription)
        {
            if (endpointDescription.AuthorizedVerbs == HttpVerb.None)
            {
                return;
            }

            var endpoint = builder
                            .MapMethods(
                                endpointDescription.Pattern,
                                HttpVerbs.GetMethods(endpointDescription.AuthorizedVerbs),
                                MisconfigurationFallbackAsync)
                            .WithMetadata(endpointDescription);

            var authorizeData = CreateAuthorizeDataForEndpoint(endpointDescription);
            endpoint = endpoint.RequireAuthorization(authorizeData);

            MapCommonEndpointProperties(endpoint, endpointDescription);
        }

        private static IAuthorizeData[] CreateAuthorizeDataForEndpoint(EndpointDescription endpointDescription)
        {
            var policies = new List<string>();

            if (!endpointDescription.CustomPolicies.IsDefaultOrEmpty)
            {
                policies.AddRange(endpointDescription.CustomPolicies);
            }

            if (!endpointDescription.Permissions.IsDefaultOrEmpty ||
               !endpointDescription.ClaimTypes.IsDefaultOrEmpty ||
               !endpointDescription.Roles.IsDefaultOrEmpty ||
               !endpointDescription.AuthenticationSchemes.IsDefaultOrEmpty)
            {
                policies.Add(endpointDescription.GetEndpointPolicyName());
            }

            return policies.Select(policy =>
            {
                var authorizeAttribute = new AuthorizeAttribute { Policy = policy, };

                if (!endpointDescription.AuthenticationSchemes.IsDefaultOrEmpty)
                {
                    authorizeAttribute.AuthenticationSchemes = string.Join(',', endpointDescription.AuthenticationSchemes);
                }

                if (!endpointDescription.Roles.IsDefaultOrEmpty)
                {
                    authorizeAttribute.Roles = string.Join(',', endpointDescription.Roles);
                }

                return authorizeAttribute;
            }).ToArray();
        }

        private static void MapCommonEndpointProperties(IEndpointConventionBuilder endpoint, EndpointDescription endpointDescription)
        {
            if (endpointDescription.CustomUserOptions is not null)
            {
                endpointDescription.CustomUserOptions(endpoint);
            }
        }

        private static async Task MisconfigurationFallbackAsync(HttpContext context)
        {
            const string Message =
                $"The middleware is misconfigured. Call {nameof(UseSimpleEndpoints)}() after any routing " +
                $"middleware like {nameof(EndpointRoutingApplicationBuilderExtensions.UseRouting)}() " +
                $"and before any terminating middleware like {nameof(EndpointRoutingApplicationBuilderExtensions.UseEndpoints)}().";

            var hostEnvironment = context.RequestServices.GetService<IHostEnvironment>();

            var logger = context.RequestServices.GetService<ILogger<SimpleEndpointMiddleware>>();
            logger?.LogWarning(Message);

            // if we cant know for sure that we are running in a development environment we just return a 500
            // since we dont want to leak implementation details to users.
            if (hostEnvironment is null || !hostEnvironment.IsDevelopment())
            {
                await context.Response.SendGeneralServerErrorAsync();
                return;
            }

            await context.Response.SendGeneralServerErrorAsync(Message);
        }
    }
}
