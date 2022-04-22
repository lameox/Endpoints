using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Lameox.Endpoints
{
    public sealed partial class EndpointConfiguration
    {
        internal int? Version { get; private set; }
        internal ImmutableArray<string> Routes { get; private set; }
        internal HttpVerb Verbs { get; private set; }

        internal Action<IEndpointConventionBuilder>? CustomUserOptions { get; private set; }
    }
}
