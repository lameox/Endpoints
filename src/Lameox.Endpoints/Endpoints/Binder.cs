using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    internal static partial class Binder<TRequest> where TRequest : notnull
    {
        private static readonly ImmutableDictionary<string, PropertySetter> _regularProperties;
        private static readonly ImmutableArray<RequiredProperty> _requiredProperties;

        static Binder()
        {
            InitializeProperties(out _regularProperties, out _requiredProperties);
        }

        private static void InitializeProperties(
            out ImmutableDictionary<string, PropertySetter> regularProperties,
            out ImmutableArray<RequiredProperty> requiredProperties)
        {
            ImmutableDictionary<string, PropertySetter>.Builder? regularPropertiesBuilder = null;
            ImmutableArray<RequiredProperty>.Builder? requiredPropertiesBuilder = null;

            foreach (var property in typeof(TRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                var fromClaimAttribute = property.GetCustomAttribute<FromClaimAttribute>();
                if (fromClaimAttribute is not null)
                {
                    AddFromClaimProperty(property, fromClaimAttribute, ref requiredPropertiesBuilder);
                    continue;
                }

                var fromHeaderAttribute = property.GetCustomAttribute<FromHeaderAttribute>();
                if (fromHeaderAttribute is not null)
                {
                    AddFromHeaderProperty(property, fromHeaderAttribute, ref requiredPropertiesBuilder);
                    continue;
                }

                var hasPermissionAttribute = property.GetCustomAttribute<HasPermissionAttribute>();
                if (hasPermissionAttribute is not null)
                {
                    AddHasPermissionProperty(property, hasPermissionAttribute, ref requiredPropertiesBuilder);
                    continue;
                }

                AddRegularProperty(property, ref regularPropertiesBuilder);
            }

            regularProperties = regularPropertiesBuilder?.ToImmutable() ?? ImmutableDictionary<string, PropertySetter>.Empty;
            requiredProperties = requiredPropertiesBuilder?.ToImmutable() ?? ImmutableArray<RequiredProperty>.Empty;
        }

        private static void AddRegularProperty(PropertyInfo property, ref ImmutableDictionary<string, Binder<TRequest>.PropertySetter>.Builder? properties)
        {
            properties ??= ImmutableDictionary.CreateBuilder<string, PropertySetter>();

            var bindAttribute = property.GetCustomAttribute<BindAttribute>();

            var bindingName = bindAttribute?.Name ?? property.Name;

            properties.Add(bindingName, PropertySetter.Create(property));
        }

        private static void AddFromClaimProperty(PropertyInfo property, FromClaimAttribute attribute, ref ImmutableArray<RequiredProperty>.Builder? properties)
        {
            properties ??= ImmutableArray.CreateBuilder<RequiredProperty>();

            var identifier = attribute.ClaimType ?? property.Name;

            var requiredProperty = new RequiredProperty(LookupKind.Claim, identifier, attribute.IsRequired, PropertySetter.Create(property));

            properties.Add(requiredProperty);
        }

        private static void AddFromHeaderProperty(PropertyInfo property, FromHeaderAttribute attribute, ref ImmutableArray<RequiredProperty>.Builder? properties)
        {
            properties ??= ImmutableArray.CreateBuilder<RequiredProperty>();

            var identifier = attribute.HeaderName ?? property.Name;

            var requiredProperty = new RequiredProperty(LookupKind.HeaderValue, identifier, attribute.IsRequired, PropertySetter.Create(property));

            properties.Add(requiredProperty);
        }

        private static void AddHasPermissionProperty(PropertyInfo property, HasPermissionAttribute attribute, ref ImmutableArray<RequiredProperty>.Builder? properties)
        {
            properties ??= ImmutableArray.CreateBuilder<RequiredProperty>();

            var identifier = attribute.Permission ?? property.Name;

            var requiredProperty = new RequiredProperty(LookupKind.Permission, identifier, attribute.IsRequired, PropertySetter.Create(property));

            properties.Add(requiredProperty);
        }
    }
}
