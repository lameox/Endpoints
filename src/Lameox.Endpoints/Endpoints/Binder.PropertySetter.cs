using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Lameox.Endpoints
{
    internal static partial class Binder<TRequest>
    {
        internal abstract class PropertySetter
        {
            public abstract Type PropertyType { get; }
            public abstract string PropertyName { get; }

            public abstract bool TryParseAndSet(ref TRequest target, object? value);
            public abstract bool CanSetValueDirectly<TValue>();

            public static PropertySetter Create(PropertyInfo property)
            {
                var genericType = typeof(PropertySetter<>).MakeGenericType(typeof(TRequest), property.PropertyType);
                return (PropertySetter)genericType
                    .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.CreateInstance)
                    .Single()
                    .Invoke(new[] { property });
            }
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
        internal sealed class PropertySetter<TProperty> : PropertySetter
        {
            private delegate void SetPropertyDelegate(ref TRequest property, TProperty value);

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
                var valueParameter = Expression.Parameter(typeof(TProperty), "value");

                var assignment = Expression.Assign(Expression.Property(targetParameter, property), valueParameter);

                return Expression.Lambda<SetPropertyDelegate>(assignment, targetParameter, valueParameter).Compile();
            }

            private readonly PropertyInfo _propertyInfo;

            private SetPropertyDelegate? _lazySetter;
            private SetPropertyDelegate Setter => _lazySetter ??= CompileSetter(_propertyInfo);

            public override Type PropertyType => typeof(TProperty);
            public override string PropertyName => _propertyInfo.Name;

            private PropertySetter(PropertyInfo propertyInfo)
            {
                Debug.Assert(propertyInfo.PropertyType == typeof(TProperty));
                _propertyInfo = propertyInfo;
            }

            public override bool CanSetValueDirectly<TValue>()
            {
                return typeof(TProperty) == typeof(TValue);
            }

            public override bool TryParseAndSet(ref TRequest target, object? value)
            {
                if (value is null)
                {
                    return false;
                }

                if (value is TProperty typedValue)
                {
                    return TrySet(ref target, typedValue);
                }

                if (!ValueParser<TProperty>.HasParser || !ValueParser<TProperty>.TryParseValue(value.ToString() ?? string.Empty, out var parsedValue))
                {
                    return false;
                }

                return TrySet(ref target, parsedValue);
            }

            private bool TrySet(ref TRequest target, TProperty value)
            {
                Setter(ref target, value);
                return true;
            }
        }
    }
}
