namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal Type EndpointType { get; }

        public string Pattern { get; }
        public HttpVerb Verbs { get; }
        public string? PermissionClaimType { get; }

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