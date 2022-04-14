using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    internal static partial class Binder<TRequest>
    {
        public static ValueTask<ImmutableArray<BindingFailure>> BindRequestValuesAsync(ref TRequest request, HttpContext requestContext, EndpointDescription endpointDescription)
        {
            var failures = new FailureCollection();

            BindFormValues(ref request, requestContext, ref failures);
            BindRouteValues(ref request, requestContext, ref failures);
            BindQueryParameters(ref request, requestContext, ref failures);

            BindRequiredProperties(ref request, requestContext, ref failures, endpointDescription);

            return ValueTask.FromResult(failures.ToImmutable());
        }

        private static void BindRequiredProperties(ref TRequest request, HttpContext requestContext, ref FailureCollection failures, EndpointDescription endpointDescription)
        {
            if (_requiredProperties.Length < 1)
            {
                return;
            }

            foreach (var property in _requiredProperties)
            {
                switch (property.LookupKind)
                {
                    case Binder<TRequest>.LookupKind.HeaderValue:
                        BindHeaderValue(property, ref request, requestContext, ref failures);
                        break;

                    case Binder<TRequest>.LookupKind.Claim:
                        BindClaimValue(property, ref request, requestContext, ref failures);
                        break;

                    case Binder<TRequest>.LookupKind.Permission:
                        BindPermissionValue(property, ref request, requestContext, ref failures, endpointDescription.PermissionClaimType);
                        break;

                    default:
                        throw ExceptionUtilities.UnexpectedValue(property.LookupKind);
                }
            }
        }

        private static void BindFormValues(ref TRequest request, HttpContext requestContext, ref FailureCollection failures)
        {
            if (!requestContext.Request.HasFormContentType)
            {
                return;
            }

            foreach (var formField in requestContext.Request.Form)
            {
                Bind(ref request, formField.Key, formField.Value[0], ref failures);
            }

            foreach (var formFile in requestContext.Request.Form.Files)
            {
                if (_regularProperties.TryGetValue(formFile.Name, out var propertySetter))
                {
                    if (propertySetter.CanSetValueDirectly<IFormFile>() || !propertySetter.TryParseAndSet(ref request, formFile))
                    {
                        failures.Add(new(propertySetter.PropertyType, formFile.Name, null, $"Form files can only be bound to Properties of type {nameof(IFormFile)}."));
                    }
                }
            }

            return;
        }

        private static void BindRouteValues(ref TRequest request, HttpContext requestContext, ref FailureCollection failures)
        {
            if (!requestContext.Request.RouteValues.Any())
            {
                return;
            }

            foreach (var routeValue in requestContext.Request.RouteValues)
            {
                Bind(ref request, routeValue.Key, routeValue.Value, ref failures);
            }

            return;
        }

        private static void BindQueryParameters(ref TRequest request, HttpContext requestContext, ref FailureCollection failures)
        {
            if (!requestContext.Request.Query.Any())
            {
                return;
            }

            foreach (var queryParameter in requestContext.Request.Query)
            {
                Bind(ref request, queryParameter.Key, queryParameter.Value[0], ref failures);
            }

            return;
        }

        private static void BindHeaderValue(RequiredProperty property, ref TRequest request, HttpContext requestContext, ref FailureCollection failures)
        {
            if (!requestContext.Request.Headers.TryGetValue(property.LookupIdentifier, out var values))
            {
                if (property.IsRequired)
                {
                    failures.Add(
                        new BindingFailure(
                            property.PropertySetter.PropertyType,
                            property.PropertySetter.PropertyName,
                            null,
                            $"The request is missing the required header {property.LookupIdentifier}."));
                }

                return;
            }

            BindValue(property.PropertySetter, ref request, values[0], ref failures);
        }

        private static void BindClaimValue(RequiredProperty property, ref TRequest request, HttpContext requestContext, ref FailureCollection failures)
        {
            var claim = requestContext.User.FindFirst(property.LookupIdentifier);

            if (claim is null)
            {
                if (property.IsRequired)
                {
                    failures.Add(
                        new BindingFailure(
                            property.PropertySetter.PropertyType,
                            property.PropertySetter.PropertyName,
                            null,
                            $"The request is missing the required claim {property.LookupIdentifier}."));
                }

                return;
            }

            BindValue(property.PropertySetter, ref request, claim.Value, ref failures);
        }

        private static void BindPermissionValue(
            Binder<TRequest>.RequiredProperty property,
            ref TRequest request,
            HttpContext requestContext,
            ref Binder<TRequest>.FailureCollection failures,
            string? permissionClaimType)
        {
            if (permissionClaimType is null)
            {
                return;
            }

            if (property.PropertySetter.PropertyType != typeof(bool))
            {
                failures.Add(
                    new BindingFailure(
                        property.PropertySetter.PropertyType,
                        property.PropertySetter.PropertyName,
                        null,
                        $"Only properties of type {typeof(bool)} can be bound with the {nameof(HasPermissionAttribute)} attribute."));

                return;
            }

            var hasPermission = requestContext.User.HasClaim(permissionClaimType, property.LookupIdentifier);

            if (!hasPermission && property.IsRequired)
            {
                failures.Add(
                    new BindingFailure(
                        property.PropertySetter.PropertyType,
                        property.PropertySetter.PropertyName,
                        null,
                        $"User is lacking the {property.LookupIdentifier} permission."));

                return;
            }

            BindValue(property.PropertySetter, ref request, hasPermission, ref failures);
        }

        private static void Bind(ref TRequest request, string key, object? value, ref FailureCollection failures)
        {
            if (!_regularProperties.TryGetValue(key, out var propertySetter))
            {
                return;
            }

            BindValue(propertySetter, ref request, value, ref failures);
        }

        private static void BindValue(PropertySetter propertySetter, ref TRequest request, object? value, ref FailureCollection failures)
        {
            if (!propertySetter.TryParseAndSet(ref request, value))
            {
                failures.Add(new BindingFailure(propertySetter.PropertyType, propertySetter.PropertyName, value));
            }
        }
    }
}
