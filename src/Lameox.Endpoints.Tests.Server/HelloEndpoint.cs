namespace Lameox.Endpoints.Tests.Server
{
    public class HelloEndpoint : Endpoint.WithResponse<string>
    {
        protected override EndpointConfiguration Configure(EndpointConfiguration configuration)
        {
            return configuration
                .WithRoutes("/")
                .WithVerbs(HttpVerb.Get, EndpointAuthorization.None);
        }

        protected override ValueTask<string> GetResponseAsync(CancellationToken cancellationToken)
        {
            return ValueTask.FromResult("Hello");
        }
    }
}
