using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    public sealed class BindingFailure
    {
        public string PropertyName { get; }
        public Type PropertyType { get; }
        public object? Value { get; }

        public string DisplayString =>
            $"Could not bind property {PropertyName} with type {PropertyType.FullName} to value {Value?.ToString()}.";

        internal BindingFailure(Type propertyType, string propertyName, object? value)
        {
            PropertyType = propertyType;
            PropertyName = propertyName;
            Value = value;
        }

        internal BindingFailure(Type propertyType, KeyValuePair<string, object?> keyValuePair) : this(propertyType, keyValuePair.Key, keyValuePair.Value)
        {
        }
    }
}
