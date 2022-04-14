using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    internal static partial class Binder<TRequest> where TRequest : notnull
    {
        private static readonly ImmutableDictionary<string, PropertySetter> _propertySetters;

        static Binder()
        {
            _propertySetters = typeof(TRequest).GetProperties().ToImmutableDictionary(p => p.Name, p => PropertySetter.Create(p));
        }


    }
}
