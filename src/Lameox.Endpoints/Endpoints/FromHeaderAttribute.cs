using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FromHeaderAttribute : Attribute
    {
        public string? HeaderName { get; }
        public bool IsRequired { get; }

        public FromHeaderAttribute(string? headerName = null, bool isRequired = true)
        {
            HeaderName = headerName;
            IsRequired = isRequired;
        }

        public FromHeaderAttribute(bool isRequired)
        {
            HeaderName = null;
            IsRequired = isRequired;
        }
    }
}
