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
                return new PropertySetter(property);
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

            private readonly PropertyInfo _propertyInfo;

            private SetPropertyDelegate? _lazySetter;
            private ValueParser.TryParseValueDelegate? _lazyTryParseValue;

            private SetPropertyDelegate Setter => _lazySetter ??= CompileSetter(_propertyInfo);
            private ValueParser.TryParseValueDelegate TryParseValue => _lazyTryParseValue ??= ValueParser.Get(PropertyType);


            public bool CanParseValues => TryParseValue != ValueParser.NoParser;
            public Type PropertyType { get; }

            private PropertySetter(PropertyInfo propertyInfo)
            {
                _propertyInfo = propertyInfo;
                PropertyType = _propertyInfo.PropertyType;
            }

            public bool TryParseAndSet(ref TRequest target, object? value)
            {
                if (value is null)
                {
                    return false;
                }

                var valueType = value.GetType();

                if (valueType == PropertyType)
                {
                    return TrySet(ref target, value);
                }

                if (!CanParseValues || !TryParseValue(value.ToString() ?? string.Empty, out var parsedValue))
                {
                    return false;
                }

                return TrySet(ref target, parsedValue);
            }

            public bool TrySet(ref TRequest target, object? value)
            {
                if (value is null || value.GetType() != PropertyType)
                {
                    return false;
                }

                Setter(ref target, value);
                return true;
            }
        }
    }
}
