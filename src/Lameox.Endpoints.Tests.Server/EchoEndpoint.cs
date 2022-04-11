namespace Lameox.Endpoints.Tests.Server
{
    public class EchoEndpoint : Endpoint.WithStringRequest.WithResponse<string>
    {
        protected override EndpointConfiguration Configure(EndpointConfiguration configuration)
        {
            return configuration
                .WithRoute("/echo")
                .WithVerbs(HttpVerb.Post);
        }

        protected override ValueTask<string> GetResponseAsync(string request, CancellationToken cancellationToken)
        {
            return ValueTask.FromResult(request);
        }
    }
}
