using Microsoft.AspNetCore.Mvc;

namespace Lameox.Endpoints.Tests.Server
{
    public class Json
    {
        public readonly struct Request
        {
            public string A { get; init; }
            public string B { get; init; }
            public C C { get; init; }
        }

        public class C
        {
            public int Value { get; }

            public C(string v)
            {
                Value = int.Parse(v);
            }
            public C(int v)
            {
                Value = v;
            }

            //public static C Parse(string input)
            //{
            //    return new C(int.Parse(input));
            //}
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public class EndpointImpl : Endpoint.WithRequest<Request>.WithResponse<string>
        {
            private readonly ILogger _logger;
            public EndpointImpl(ILogger<EndpointImpl> logger)
            {
                _logger = logger;
            }

            protected override ValueTask<string> GetResponseAsync(Request request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("Endpoint called");
                return ValueTask.FromResult(request.A + " " + request.B + ": " + request.C.Value);
            }

            protected override EndpointConfiguration Configure(EndpointConfiguration configuration)
            {
                return configuration
                    .WithVerbs(HttpVerb.Post, EndpointAuthorization.None)
                    .WithRoutes("/json/{C}");
            }
        }
    }
}
