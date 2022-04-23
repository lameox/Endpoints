using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lameox.Endpoints
{
    internal class EndpointDescriptionsBuilder
    {
        private static readonly string[] ExcludedAssemblyRootNamespaces = new[]
        {
            "mscorlib",
            "Microsoft.",
            "System.",
        };

        private static readonly Assembly ThisAssembly = typeof(StartupExtensions).Assembly;

        public ImmutableArray<EndpointDescription> Endpoints { get; }

        public EndpointDescriptionsBuilder(IServiceCollection services, IEnumerable<Assembly> additionalEndpointAssemblies)
        {
            Endpoints = CreateDescriptions(services, additionalEndpointAssemblies);
        }

        private static ImmutableArray<EndpointDescription> CreateDescriptions(IServiceCollection services, IEnumerable<Assembly> additionalEndpointAssemblies)
        {
            _ = services;

            var endpointTypes =
                AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Union(additionalEndpointAssemblies)
                    .Where(a =>
                        !a.IsDynamic &&
                        a != ThisAssembly &&
                        !ExcludedAssemblyRootNamespaces.Any(n => a.FullName!.StartsWith(n)))
                    .SelectMany(a => a.GetTypes())
                    .Where(t =>
                        !t.IsAbstract &&
                        !t.IsInterface &&
                        t.IsAssignableTo(typeof(IEndpoint)));

            if (!endpointTypes.Any())
            {
                return ImmutableArray<EndpointDescription>.Empty;
            }

            var builder = ImmutableArray.CreateBuilder<EndpointDescription>();

            foreach (var endpointType in endpointTypes)
            {
                CreateDescriptionsForSingleType(builder, services, endpointType);
            }

            return builder.ToImmutable();
        }

        private static EndpointConfiguration GetConfigurationForType(Type endpointType)
        {
            IEndpoint? instance =
                endpointType.GetConstructor(Array.Empty<Type>()) is null
                ? (IEndpoint)FormatterServices.GetUninitializedObject(endpointType)!
                : (IEndpoint)Activator.CreateInstance(endpointType)!;

            if (instance is null)
            {
                throw ExceptionUtilities.Unreachable();
            }

            return instance.Configuration;
        }

        private static void CreateDescriptionsForSingleType(
            ImmutableArray<EndpointDescription>.Builder endpointDescriptions,
            IServiceCollection services,
            Type endpointType)
        {
            var configuration = GetConfigurationForType(endpointType);

            services.AddScoped(endpointType);

            foreach (var route in configuration.Routes)
            {
                var descriptionForRoute = CreateDescriptionForRoute(services, endpointType, configuration, route);
                endpointDescriptions.Add(descriptionForRoute);
            }
        }

        private static EndpointDescription CreateDescriptionForRoute(
            IServiceCollection services,
            Type endpointType,
            EndpointConfiguration configuration,
            string route)
        {
            //TODO

            _ = services;

            return EndpointDescription.Create(
                endpointType,
                route,
                configuration.AnonymousVerbs,
                configuration.AuthenticatedVerbs,
                null,
                ImmutableArray<string>.Empty,
                configuration.CustomUserOptions);
        }
    }
}
