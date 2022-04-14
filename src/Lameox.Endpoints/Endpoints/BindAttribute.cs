using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    /// <summary>
    /// Can be used to bind to a query parameter, route value or formfield of a different name than the property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class BindAttribute : Attribute
    {
        public string Name { get; set; }

        public BindAttribute(string name)
        {
            Name = name;
        }
    }
}
