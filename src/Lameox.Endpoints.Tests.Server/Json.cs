namespace Lameox.Endpoints.Tests.Server
{
    public class Json
    {
        public readonly struct Request
        {
            public string A { get; init; }
            public string B { get; init; }
            public int C { get; init; }
        }

        public class EndpointImpl : Endpoint.WithRequest<Request>.WithResponse<string>
        {
            protected override ValueTask<string> GetResponseAsync(Request request, CancellationToken cancellationToken)
            {
                return ValueTask.FromResult(request.A + " " + request.B + ": " + request.C);
            }

            protected override EndpointConfiguration Configure(EndpointConfiguration configuration)
            {
                return configuration.WithVerbs(HttpVerb.Post).WithRoutes("/json/{C}");
            }
        }
    }
}
