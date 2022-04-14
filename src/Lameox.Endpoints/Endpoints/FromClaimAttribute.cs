using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    /// <summary>
    /// Can be used to bind to the value of a claim with the given name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class FromClaimAttribute : Attribute
    {
        public string? ClaimType { get; }
        public bool IsRequired { get; }

        public FromClaimAttribute(string? claimType = null, bool isRequired = true)
        {
            ClaimType = claimType;
            IsRequired = isRequired;
        }

        public FromClaimAttribute(bool isRequired)
        {
            ClaimType = null;
            IsRequired = isRequired;
        }
    }
}
