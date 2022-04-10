using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    public sealed partial class EndpointConfiguration
    {
        public int Version { get; private set; } = 1;
        public ImmutableArray<string> Routes { get; private set; }
        public HttpVerb Verbs { get; private set; }
    }
}
