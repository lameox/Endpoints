using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    /// <summary>
    /// Can be used to bind to a Property of type <see cref="bool"/>.
    /// The Property will be <see langword="true"/> if the user has the given permission, otherwise it will be false.
    /// </summary>
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
