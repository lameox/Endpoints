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
        private delegate void SetPropertyDelegate(ref TRequest property, object value);

        private static readonly ImmutableDictionary<string, PropertySetter> _propertySetters;

        static Binder()
        {
            _propertySetters = typeof(TRequest).GetProperties().ToImmutableDictionary(p => p.Name, p => PropertySetter.Create(p));
        }

        public static ValueTask<ImmutableArray<BindingFailure>> BindRequestValuesAsync(ref TRequest request, HttpContext requestContext)
        {
            var failures = new FailureCollection();

            BindFormValues(ref request, requestContext, ref failures);
            BindRouteValues(ref request, requestContext, ref failures);
            BindQueryParameters(ref request, requestContext, ref failures);

            return ValueTask.FromResult(failures.ToImmutable());
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
                if (_propertySetters.TryGetValue(formFile.Name, out var propertySetter))
                {
                    if (propertySetter.PropertyType != typeof(IFormFile) || !propertySetter.TrySet(ref request, formFile))
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

        private static void Bind(ref TRequest request, string key, object? value, ref FailureCollection failures)
        {
            if (!_propertySetters.TryGetValue(key, out var propertySetter) || !propertySetter.CanParseValues)
            {
                return;
            }

            if (!propertySetter.TryParseAndSet(ref request, value))
            {
                failures.Add(new BindingFailure(propertySetter.PropertyType, key, value));
            }

            return;
        }
    }
}
