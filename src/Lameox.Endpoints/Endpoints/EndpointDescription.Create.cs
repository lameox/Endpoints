namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal static EndpointDescription Create(
            Type endpointType,
            string pattern,
            HttpVerb verbs,
            string? permissionClaimType)
        {
            return new EndpointDescription(
                endpointType,
                pattern,
                verbs,
                permissionClaimType);
        }
    }
}