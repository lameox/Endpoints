namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal Type EndpointType { get; }

        public string Pattern { get; }
        public HttpVerb Verbs { get; }

        private EndpointDescription(
            Type endpointType,
            string pattern,
            HttpVerb verbs)
        {
            EndpointType = endpointType;
            Pattern = pattern;
            Verbs = verbs;
        }
    }
}