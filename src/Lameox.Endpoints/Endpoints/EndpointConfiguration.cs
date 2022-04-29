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
        private IArrayBuilderOwner<string>? PoliciesBuilder { get; set; }

        internal ImmutableArray<string> Roles { get; private set; }
        private IArrayBuilderOwner<string>? RolesBuilder { get; set; }
        internal bool AllRolesRequired { get; private set; }

        internal string? PermissionClaimType { get; private set; }
        internal ImmutableArray<string> Permissions { get; private set; }
        private IArrayBuilderOwner<string>? PermissionsBuilder { get; set; }
        internal bool AllPermissionsRequired { get; private set; }

        internal ImmutableArray<string> ClaimTypes { get; private set; }
        private IArrayBuilderOwner<string>? ClaimTypesBuilder { get; set; }
        internal bool AllClaimTypesRequired { get; private set; }

        private bool _frozen;

        private void EnsureNotFrozen()
        {
            if (_frozen)
            {
                throw new InvalidOperationException($"Cant modify {nameof(EndpointConfiguration)} after it has been frozen");
            }
        }

        internal void Freeze()
        {
            if (_frozen)
            {
                return;
            }

            _frozen = true;

            if (PoliciesBuilder is not null)
            {
                Policies = PoliciesBuilder.Builder.ToImmutableArray();
                PoliciesBuilder.Dispose();
                PoliciesBuilder = null;
            }
            else
            {
                Policies = ImmutableArray<string>.Empty;
            }

            if (RolesBuilder is not null)
            {
                Roles = RolesBuilder.Builder.ToImmutableArray();
                RolesBuilder.Dispose();
                RolesBuilder = null;
            }
            else
            {
                Roles = ImmutableArray<string>.Empty;
            }

            if (PermissionsBuilder is not null)
            {
                Permissions = PermissionsBuilder.Builder.ToImmutableArray();
                PermissionsBuilder.Dispose();
                PermissionsBuilder = null;
            }
            else
            {
                Permissions = ImmutableArray<string>.Empty;
            }

            if (ClaimTypesBuilder is not null)
            {
                ClaimTypes = ClaimTypesBuilder.Builder.ToImmutableArray();
                ClaimTypesBuilder.Dispose();
                ClaimTypesBuilder = null;
            }
            else
            {
                ClaimTypes = ImmutableArray<string>.Empty;
            }
        }
    }
}
