using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

namespace Lameox.Endpoints
{
    internal static partial class Binder<TRequest>
    {
        private sealed class PropertySetter
        {
            public static PropertySetter Create(PropertyInfo property)
            {
                var setter = CompileSetter(property);
                return new PropertySetter(property.PropertyType, setter, ValueParser.Get(property.PropertyType));
            }

            private static SetPropertyDelegate CompileSetter(PropertyInfo property)
            {
                // This method compiles a set of the property on the target.
                // The target is passed by reference so we can also work on value types.
                // The compiled pseudocode looks like this:
                // void Setter(ref TRequest target, object value)
                // {
                //     target.Prop = (PropertyType)value;
                // }

                var targetParameter = Expression.Parameter(typeof(TRequest).MakeByRefType(), "target");
                var valueParameter = Expression.Parameter(typeof(object), "value");

                var castToPropertyType = Expression.Convert(valueParameter, property.PropertyType);

                var assignment = Expression.Assign(Expression.Property(targetParameter, property), castToPropertyType);

                return Expression.Lambda<SetPropertyDelegate>(assignment, targetParameter, valueParameter).Compile();
            }

            private readonly SetPropertyDelegate _setter;
            private readonly ValueParser.TryParseValueDelegate? _tryParseValue;

            [MemberNotNullWhen(true, nameof(_tryParseValue))]
            public bool CanParseValues => _tryParseValue is not null;

            public Type PropertyType { get; }

            private PropertySetter(Type propertyType, SetPropertyDelegate setter, ValueParser.TryParseValueDelegate? tryParseValue)
            {
                PropertyType = propertyType;
                _setter = setter;
                _tryParseValue = tryParseValue;
            }

            public bool TrySet(ref TRequest target, object? value)
            {
                if (!CanParseValues || !_tryParseValue(value, out var parsedValue))
                {
                    return false;
                }

                _setter(ref target, parsedValue);
                return true;
            }
        }
    }
}
