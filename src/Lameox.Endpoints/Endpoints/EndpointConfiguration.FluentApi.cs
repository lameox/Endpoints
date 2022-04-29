using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Lameox.Endpoints
{
    public enum EndpointAuthorization
    {
        None,
        Required,
    }

    public interface IRequiredPoliciesBuilder
    {
        IRequiredPoliciesBuilder WithPolicy(string policy);
        IRequiredPoliciesBuilder WithPolicies(IEnumerable<string> policies);
    }

    public interface IRequiredPermissionsBuilder
    {
        IRequiredPermissionsBuilder WithPermission(string permission);
        IRequiredPermissionsBuilder WithPermissions(IEnumerable<string> permissions);
        IRequiredPermissionsBuilder WithPermissionClaimType(string claimType);
        IRequiredPermissionsBuilder RequireAll();
        IRequiredPermissionsBuilder RequireAny();
    }

    public interface IRequiredRolesBuilder
    {
        IRequiredRolesBuilder WithRole(string role);
        IRequiredRolesBuilder WithRoles(IEnumerable<string> roles);
        IRequiredRolesBuilder RequireAll();
        IRequiredRolesBuilder RequireAny();
    }

    public interface IRequiredClaimsBuilder
    {
        IRequiredClaimsBuilder WithClaim(string claimType);
        IRequiredClaimsBuilder WithClaims(IEnumerable<string> claimTypes);
        IRequiredClaimsBuilder RequireAll();
        IRequiredClaimsBuilder RequireAny();
    }

    public sealed partial class EndpointConfiguration : IRequiredPoliciesBuilder, IRequiredPermissionsBuilder, IRequiredRolesBuilder, IRequiredClaimsBuilder
    {
        public EndpointConfiguration WithVersion(int version)
        {
            EnsureNotFrozen();

            if (version < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version));
            }

            Version = version;
            return this;
        }

        public EndpointConfiguration WithRoutes(string route)
        {
            EnsureNotFrozen();
            return WithRoutes(ImmutableArray.Create(route));
        }

        public EndpointConfiguration WithRoutes(string route1, string route2)
        {
            EnsureNotFrozen();
            return WithRoutes(ImmutableArray.Create(route1, route2));
        }

        public EndpointConfiguration WithRoutes(string route1, string route2, string route3)
        {
            EnsureNotFrozen();
            return WithRoutes(ImmutableArray.Create(route1, route2, route3));
        }

        public EndpointConfiguration WithRoutes(params string[] routes)
        {
            EnsureNotFrozen();
            return WithRoutes(routes.AsEnumerable());
        }

        public EndpointConfiguration WithRoutes(IEnumerable<string> routes)
        {
            EnsureNotFrozen();
            Routes = routes.ToImmutableArray();
            return this;
        }

        public EndpointConfiguration WithGet(EndpointAuthorization authorization)
        {
            EnsureNotFrozen();
            return WithVerb(HttpVerb.Get, authorization);
        }

        public EndpointConfiguration WithPost(EndpointAuthorization authorization)
        {
            EnsureNotFrozen();
            return WithVerb(HttpVerb.Post, authorization);
        }

        public EndpointConfiguration WithPut(EndpointAuthorization authorization)
        {
            EnsureNotFrozen();
            return WithVerb(HttpVerb.Put, authorization);
        }

        public EndpointConfiguration WithDelete(EndpointAuthorization authorization)
        {
            EnsureNotFrozen();
            return WithVerb(HttpVerb.Delete, authorization);
        }

        private EndpointConfiguration WithVerb(HttpVerb verb, EndpointAuthorization authorization)
        {
            if (BitOperations.PopCount((uint)verb) != 1)
            {
                throw new ArgumentException("Only a single verb can be specified", nameof(verb));
            }

            return authorization == EndpointAuthorization.Required ?
                WithAuthorizedVerbs(AuthenticatedVerbs | verb) :
                WithAnonymousVerbs(AnonymousVerbs | verb);
        }

        public EndpointConfiguration WithVerbs(HttpVerb verbs, EndpointAuthorization authorization)
        {
            EnsureNotFrozen();
            return authorization == EndpointAuthorization.Required ?
                WithAuthorizedVerbs(verbs) :
                WithAnonymousVerbs(verbs);
        }

        public EndpointConfiguration WithAuthorizedVerbs(HttpVerb verbs)
        {
            EnsureNotFrozen();
            var invalidVerbs = AuthenticatedVerbs & verbs;
            if (invalidVerbs != HttpVerb.None)
            {
                throw ExceptionUtilities.VerbsDefinedAsAnonymousAndAuthenticated(invalidVerbs);
            }

            AnonymousVerbs = verbs;
            return this;
        }

        public EndpointConfiguration WithAnonymousVerbs(HttpVerb verbs)
        {
            EnsureNotFrozen();
            var invalidVerbs = AuthenticatedVerbs & verbs;
            if (invalidVerbs != HttpVerb.None)
            {
                throw ExceptionUtilities.VerbsDefinedAsAnonymousAndAuthenticated(invalidVerbs);
            }

            AnonymousVerbs = verbs;
            return this;
        }

        public EndpointConfiguration WithOptions(Action<IEndpointConventionBuilder> builderOptions)
        {
            EnsureNotFrozen();
            CustomUserOptions = builderOptions;
            return this;
        }

        public EndpointConfiguration WithRequiredPolicies(Action<IRequiredPoliciesBuilder> policiesBuilder)
        {
            EnsureNotFrozen();
            policiesBuilder(this);
            return this;
        }

        public EndpointConfiguration WithRequiredPolicies(IEnumerable<string> policies)
        {
            EnsureNotFrozen();
            ((IRequiredPoliciesBuilder)this).WithPolicies(policies);
            return this;
        }

        IRequiredPoliciesBuilder IRequiredPoliciesBuilder.WithPolicy(string policy)
        {
            EnsureNotFrozen();
            (PoliciesBuilder ??= ImmutableArrayBuilderPool<string>.Get()).Builder.Add(policy);
            return this;
        }

        IRequiredPoliciesBuilder IRequiredPoliciesBuilder.WithPolicies(IEnumerable<string> policies)
        {
            EnsureNotFrozen();
            (PoliciesBuilder ??= ImmutableArrayBuilderPool<string>.Get()).Builder.AddRange(policies);
            return this;
        }

        public EndpointConfiguration WithRequiredPermissions(Action<IRequiredPermissionsBuilder> permissionsBuilder)
        {
            EnsureNotFrozen();
            permissionsBuilder(this);
            return this;
        }

        public EndpointConfiguration WithRequiredPermissions(IEnumerable<string> permissions, string permissionClaimType, bool requireAll)
        {
            EnsureNotFrozen();
            var builder = ((IRequiredPermissionsBuilder)this);

            builder.WithPermissions(permissions);
            builder.WithPermissionClaimType(permissionClaimType);

            if (requireAll)
            {
                builder.RequireAll();
            }
            else
            {
                builder.RequireAny();
            }

            return this;
        }

        IRequiredPermissionsBuilder IRequiredPermissionsBuilder.WithPermission(string permission)
        {
            EnsureNotFrozen();
            (PermissionsBuilder ??= ImmutableArrayBuilderPool<string>.Get()).Builder.Add(permission);
            return this;
        }

        IRequiredPermissionsBuilder IRequiredPermissionsBuilder.WithPermissions(IEnumerable<string> permissions)
        {
            EnsureNotFrozen();
            (PermissionsBuilder ??= ImmutableArrayBuilderPool<string>.Get()).Builder.AddRange(permissions);
            return this;
        }

        IRequiredPermissionsBuilder IRequiredPermissionsBuilder.WithPermissionClaimType(string claimType)
        {
            EnsureNotFrozen();
            PermissionClaimType = claimType;
            return this;
        }

        IRequiredPermissionsBuilder IRequiredPermissionsBuilder.RequireAll()
        {
            EnsureNotFrozen();
            AllPermissionsRequired = true;
            return this;
        }

        IRequiredPermissionsBuilder IRequiredPermissionsBuilder.RequireAny()
        {
            EnsureNotFrozen();
            AllPermissionsRequired = false;
            return this;
        }

        public EndpointConfiguration WithRequiredRoles(IEnumerable<string> roles, bool requireAll)
        {
            EnsureNotFrozen();
            var builder = ((IRequiredRolesBuilder)this);

            builder.WithRoles(roles);

            if (requireAll)
            {
                builder.RequireAll();
            }
            else
            {
                builder.RequireAny();
            }

            return this;
        }

        IRequiredRolesBuilder IRequiredRolesBuilder.WithRole(string role)
        {
            EnsureNotFrozen();
            (RolesBuilder ??= ImmutableArrayBuilderPool<string>.Get()).Builder.Add(role);
            return this;
        }

        IRequiredRolesBuilder IRequiredRolesBuilder.WithRoles(IEnumerable<string> roles)
        {
            EnsureNotFrozen();
            (RolesBuilder ??= ImmutableArrayBuilderPool<string>.Get()).Builder.AddRange(roles);
            return this;
        }

        IRequiredRolesBuilder IRequiredRolesBuilder.RequireAll()
        {
            EnsureNotFrozen();
            AllRolesRequired = true;
            return this;
        }

        IRequiredRolesBuilder IRequiredRolesBuilder.RequireAny()
        {
            EnsureNotFrozen();
            AllRolesRequired = false;
            return this;
        }

        public EndpointConfiguration WithRequiredClaimTypes(IEnumerable<string> claimTypes, bool requireAll)
        {
            EnsureNotFrozen();
            var builder = ((IRequiredClaimsBuilder)this);

            builder.WithClaims(claimTypes);

            if (requireAll)
            {
                builder.RequireAll();
            }
            else
            {
                builder.RequireAny();
            }

            return this;
        }

        IRequiredClaimsBuilder IRequiredClaimsBuilder.WithClaim(string claimType)
        {
            EnsureNotFrozen();
            (ClaimTypesBuilder ??= ImmutableArrayBuilderPool<string>.Get()).Builder.Add(claimType);
            return this;
        }

        IRequiredClaimsBuilder IRequiredClaimsBuilder.WithClaims(IEnumerable<string> claimTypes)
        {
            EnsureNotFrozen();
            (ClaimTypesBuilder ??= ImmutableArrayBuilderPool<string>.Get()).Builder.AddRange(claimTypes);
            return this;
        }

        IRequiredClaimsBuilder IRequiredClaimsBuilder.RequireAll()
        {
            EnsureNotFrozen();
            AllClaimTypesRequired = true;
            return this;
        }

        IRequiredClaimsBuilder IRequiredClaimsBuilder.RequireAny()
        {
            EnsureNotFrozen();
            AllClaimTypesRequired = false;
            return this;
        }
    }
}
