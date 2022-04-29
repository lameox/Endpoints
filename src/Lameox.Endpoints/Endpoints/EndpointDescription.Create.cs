using System.Collections.Immutable;
using Microsoft.AspNetCore.Builder;

namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal static EndpointDescription Create(
            Type endpointType,
            string pattern,
            HttpVerb anonymousVerbs,
            HttpVerb authorizedVerbs,
            Action<IEndpointConventionBuilder>? customUserOptions,
            ImmutableArray<string> policies,
            ImmutableArray<string> roles,
            bool allRolesRequired,
            string? permissionClaimType,
            ImmutableArray<string> permissions,
            bool allPermissionsRequired,
            ImmutableArray<string> claimTypes,
            bool allClaimTypesRequired)
        {
            return new EndpointDescription(
                endpointType,
                pattern,
                anonymousVerbs,
                authorizedVerbs,
                customUserOptions,
                policies,
                roles,
                allRolesRequired,
                permissionClaimType,
                permissions,
                allPermissionsRequired,
                claimTypes,
                allClaimTypesRequired);
        }
    }
}