using System.Collections.Immutable;

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
            ImmutableArray<string> customPolicies)
        {
            return new EndpointDescription(
                endpointType,
                pattern,
                anonymousVerbs,
                authorizedVerbs,
                permissionClaimType,
                customPolicies);
        }
    }
}