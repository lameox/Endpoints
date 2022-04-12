using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    internal class ValueParser
    {
        public delegate bool TryParseValueDelegate(object? input, out object value);

        public static TryParseValueDelegate Get<TTargetType>()
        {
            return Get(typeof(TTargetType));
        }

        public static TryParseValueDelegate Get(Type targetType)
        {
            if (targetType == typeof(string))
            {
                return StringParser;
            }

            if (targetType == typeof(int))
            {
                return IntParser;
            }

            throw new NotImplementedException();
        }

        private static bool StringParser(object? input, out object value)
        {
            value = input?.ToString() ?? string.Empty;
            return true;
        }

        private static bool IntParser(object? input, out object value)
        {
            var stringValue = input?.ToString() ?? string.Empty;

            var result = int.TryParse(stringValue, out int intValue);

            value = intValue;
            return result;
        }
    }
}
