using System.Collections.Immutable;

namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal Type EndpointType { get; }

        public string Pattern { get; }
        public HttpVerb AnonymousVerbs { get; }
        public HttpVerb AuthorizedVerbs { get; }
        public string? PermissionClaimType { get; }

        internal bool RequiresAuthorizationPipeline => AuthorizedVerbs != HttpVerb.None || !CustomPolicies.IsDefaultOrEmpty;

        public ImmutableArray<string> CustomPolicies { get; }
        public ImmutableArray<string> AuthenticationSchemes { get; }

        public ImmutableArray<string> Roles { get; }
        public bool AllRolesRequired { get; }
        public ImmutableArray<string> Policies { get; }
        public bool AllPoliciesRequired { get; }
        public ImmutableArray<string> Permissions { get; }
        public bool AllPermissionsRequired { get; }
        public ImmutableArray<string> ClaimTypes { get; }
        public bool AllClaimTypesRequired { get; }

        private EndpointDescription(
            Type endpointType,
            string pattern,
            HttpVerb anonymousVerbs,
            HttpVerb authorizedVerbs,
            string? permissionClaimType,
            ImmutableArray<string> customPolicies)
        {
            EndpointType = endpointType;
            Pattern = pattern;
            AnonymousVerbs = anonymousVerbs;
            AuthorizedVerbs = authorizedVerbs;
            PermissionClaimType = permissionClaimType;
            CustomPolicies = customPolicies;
        }

        private string? _cachedEndpointPolicyName;
        internal string GetEndpointPolicyName()
        {
            return _cachedEndpointPolicyName ??= $"endpoint_policy_{EndpointType.FullName}";
        }
    }
}