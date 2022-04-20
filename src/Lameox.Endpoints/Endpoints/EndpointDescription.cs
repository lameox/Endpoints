using System.Collections.Immutable;

namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal Type EndpointType { get; }

        public string Pattern { get; }
        public HttpVerb Verbs { get; }
        public string? PermissionClaimType { get; }

        public ImmutableArray<string> Roles { get; }
        public bool AllRolesRequired { get; }
        public ImmutableArray<string> Policies { get; }
        public bool AllPoliciesRequired { get; }
        public ImmutableArray<string> Permissions { get; }
        public bool AllPermissionsRequired { get; }
        public ImmutableArray<string> AuthenticationSchemes { get; }

        private EndpointDescription(
            Type endpointType,
            string pattern,
            HttpVerb verbs,
            string? permissionClaimType)
        {
            EndpointType = endpointType;
            Pattern = pattern;
            Verbs = verbs;
            PermissionClaimType = permissionClaimType;
        }
    }
}