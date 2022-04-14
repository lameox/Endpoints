using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class HasPermissionAttribute : Attribute
    {
        public string Permission { get; }
        public bool IsRequired { get; }

        public HasPermissionAttribute(string permission, bool isRequired = true)
        {
            Permission = permission;
            IsRequired = isRequired;
        }
    }
}
