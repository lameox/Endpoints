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
            string? permissionClaimType,
            ImmutableArray<string> customPolicies,
            Action<IEndpointConventionBuilder>? customUserOptions)
        {
            return new EndpointDescription(
                endpointType,
                pattern,
                anonymousVerbs,
                authorizedVerbs,
                permissionClaimType,
                customPolicies,
                customUserOptions);
        }
    }
}