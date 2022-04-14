using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    internal static partial class Binder<TRequest> where TRequest : notnull
    {
        private enum LookupKind
        {
            HeaderValue,
            Claim,
            Permission,
        }

        private readonly struct RequiredProperty
        {
            private readonly LookupKind _lookupKind;
            private readonly string _lookupIdentifier;
            private readonly bool _isRequired;
            private readonly PropertySetter _propertySetter;

#if DEBUG
            private readonly bool _isCreated;
#endif

            public LookupKind LookupKind => EnsureCreated(_lookupKind);
            public string LookupIdentifier => EnsureCreated(_lookupIdentifier);
            public bool IsRequired => EnsureCreated(_isRequired);
            public PropertySetter PropertySetter => EnsureCreated(_propertySetter);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private T EnsureCreated<T>(T value)
            {
#if DEBUG
                if (!_isCreated)
                {
                    throw ExceptionUtilities.Unreachable();
                }
#endif
                return value;
            }

            public RequiredProperty(Binder<TRequest>.LookupKind lookupKind, string lookupIdentifier, bool isRequired, Binder<TRequest>.PropertySetter propertySetter)
            {
                _lookupKind = lookupKind;
                _lookupIdentifier = lookupIdentifier;
                _isRequired = isRequired;
                _propertySetter = propertySetter;

                _isCreated = true;
            }
        }
    }
}
