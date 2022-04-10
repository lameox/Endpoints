namespace Lameox.Endpoints
{
    public partial class EndpointDescription
    {
        internal static EndpointDescription Create(Type endpointType, string pattern, HttpVerb verbs, bool implementsHandleAsync, bool implementsGetResponseAsync)
        {
            return new EndpointDescription(endpointType, pattern, verbs, implementsHandleAsync, implementsGetResponseAsync);
        }
    }
}