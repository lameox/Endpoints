namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal static EndpointDescription Create(Type endpointType, string pattern, HttpVerb verbs)
        {
            return new EndpointDescription(endpointType, pattern, verbs);
        }
    }
}