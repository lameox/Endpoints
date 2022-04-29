using System.Collections.Immutable;
using Microsoft.AspNetCore.Builder;

namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal Type EndpointType { get; }

        public string Pattern { get; }
        public HttpVerb AnonymousVerbs { get; }
        public HttpVerb AuthorizedVerbs { get; }

        public Action<IEndpointConventionBuilder>? CustomUserOptions { get; }

        internal bool RequiresAuthorizationPipeline =>
            AuthorizedVerbs != HttpVerb.None ||
            !Policies.IsDefaultOrEmpty ||
            !Roles.IsDefaultOrEmpty ||
            !Permissions.IsDefaultOrEmpty ||
            !ClaimTypes.IsDefaultOrEmpty;

        public ImmutableArray<string> Policies { get; }
        public ImmutableArray<string> AuthenticationSchemes { get; }

        public ImmutableArray<string> Roles { get; }
        public bool AllRolesRequired { get; }

        public string? PermissionClaimType { get; }
        public ImmutableArray<string> Permissions { get; }
        public bool AllPermissionsRequired { get; }
        public ImmutableArray<string> ClaimTypes { get; }
        public bool AllClaimTypesRequired { get; }

        private EndpointDescription(
            Type endpointType,
            string pattern,
            HttpVerb anonymousVerbs,
            HttpVerb authorizedVerbs,
            Action<IEndpointConventionBuilder>? customUserOptions,
            ImmutableArray<string> customPolicies,
            ImmutableArray<string> roles,
            bool allRolesRequired,
            string? permissionClaimType,
            ImmutableArray<string> permissions,
            bool allPermissionsRequired,
            ImmutableArray<string> claimTypes,
            bool allClaimTypesRequired)
        {
            EndpointType = endpointType;
            Pattern = pattern;
            AnonymousVerbs = anonymousVerbs;
            AuthorizedVerbs = authorizedVerbs;
            PermissionClaimType = permissionClaimType;
            Policies = customPolicies;
            CustomUserOptions = customUserOptions;
            Roles = roles;
            AllRolesRequired = allRolesRequired;
            Permissions = permissions;
            AllPermissionsRequired = allPermissionsRequired;
            ClaimTypes = claimTypes;
            AllClaimTypesRequired = allClaimTypesRequired;
        }

        private string? _cachedEndpointPolicyName;
        internal string GetEndpointPolicyName()
        {
            return _cachedEndpointPolicyName ??= $"endpoint_policy_{EndpointType.FullName}";
        }
    }
}