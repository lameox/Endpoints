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

        internal HttpVerb AuthenticatedVerbs { get; private set; }
        internal HttpVerb AnonymousVerbs { get; private set; }

        internal Action<IEndpointConventionBuilder>? CustomUserOptions { get; private set; }

        internal ImmutableArray<string> Policies { get; private set; }
        internal ImmutableArray<string> Roles { get; private set; }
        internal bool AllRolesRequired { get; private set; }
        internal string? PermissionClaimType { get; private set; }
        internal ImmutableArray<string> Permissions { get; private set; }
        internal bool AllPermissionsRequired { get; private set; }
        internal ImmutableArray<string> ClaimTypes { get; private set; }
        internal bool AllClaimTypesRequired { get; private set; }
    }
}
