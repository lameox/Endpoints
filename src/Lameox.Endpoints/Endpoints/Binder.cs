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


        public static ValueTask<ImmutableArray<BindingFailure>> BindRequestValuesAsync(ref TRequest request, HttpContext requestContext, EndpointDescription endpointDescription, CancellationToken cancellationToken)
        {
            _ = endpointDescription;
            _ = cancellationToken;

            var failures = ImmutableArray.CreateBuilder<BindingFailure>();

            BindRouteValues(ref request, requestContext, failures);

            return ValueTask.FromResult(failures.ToImmutable());
        }

        private static void BindRouteValues(ref TRequest request, HttpContext requestContext, ImmutableArray<BindingFailure>.Builder failures)
        {
            foreach (var routeValue in requestContext.Request.RouteValues)
            {
                Bind(ref request, routeValue.Key, routeValue.Value, failures);
            }

            return;
        }

        private static void Bind(ref TRequest request, string key, object? value, ImmutableArray<BindingFailure>.Builder failures)
        {
            if (!_propertySetters.TryGetValue(key, out var propertySetter) || !propertySetter.CanParseValues)
            {
                return;
            }

            if (!propertySetter.TrySet(ref request, value))
            {
                failures.Add(new BindingFailure(propertySetter.PropertyType, key, value));
            }

            return;
        }
    }
}
