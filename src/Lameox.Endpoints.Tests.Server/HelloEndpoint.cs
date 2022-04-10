namespace Lameox.Endpoints.Tests.Server
{
    public class HelloEndpoint : Endpoint.WithResponse<string>
    {
        protected override EndpointConfiguration Configure(EndpointConfiguration configuration)
        {
            return configuration
                .WithRoute("/")
                .WithVerbs(HttpVerb.Get);
        }

        protected override ValueTask<string> GetResponseAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult("Hello");
        }
    }
}
