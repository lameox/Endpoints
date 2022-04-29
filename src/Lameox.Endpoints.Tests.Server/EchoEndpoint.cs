namespace Lameox.Endpoints.Tests.Server
{
    public class EchoEndpoint : Endpoint.WithStringRequest.WithResponse<string>
    {
        protected override EndpointConfiguration Configure(EndpointConfiguration configuration)
        {
            return configuration
                .WithRoutes("/echo")
                .WithVerbs(HttpVerb.Post, EndpointAuthorization.None);
        }

        protected override ValueTask<string> GetResponseAsync(string request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(request);
        }
    }
}
