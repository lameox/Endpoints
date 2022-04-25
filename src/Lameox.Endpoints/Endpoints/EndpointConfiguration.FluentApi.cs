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
    public sealed partial class EndpointConfiguration
    {
        public EndpointConfiguration WithVersion(int version)
        {
            if (version < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(version));
            }

            Version = version;
            return this;
        }

        public EndpointConfiguration WithRoutes(string route)
        {
            return WithRoutes(ImmutableArray.Create(route));
        }

        public EndpointConfiguration WithRoutes(string route1, string route2)
        {
            return WithRoutes(ImmutableArray.Create(route1, route2));
        }

        public EndpointConfiguration WithRoutes(string route1, string route2, string route3)
        {
            return WithRoutes(ImmutableArray.Create(route1, route2, route3));
        }

        public EndpointConfiguration WithRoutes(params string[] routes)
        {
            return WithRoutes(routes.AsEnumerable());
        }

        public EndpointConfiguration WithRoutes(IEnumerable<string> routes)
        {
            Routes = routes.ToImmutableArray();
            return this;
        }

        public EndpointConfiguration WithGet(bool requireAuthentication = true)
        {
            return WithVerb(HttpVerb.Get, requireAuthentication);
        }

        public EndpointConfiguration WithPost(bool requireAuthentication = true)
        {
            return WithVerb(HttpVerb.Post, requireAuthentication);
        }

        public EndpointConfiguration WithPut(bool requireAuthentication = true)
        {
            return WithVerb(HttpVerb.Put, requireAuthentication);
        }

        public EndpointConfiguration WithDelete(bool requireAuthentication = true)
        {
            return WithVerb(HttpVerb.Delete, requireAuthentication);
        }

        public EndpointConfiguration WithVerb(HttpVerb verb, bool requireAuthentication = true)
        {
            if (BitOperations.PopCount((uint)verb) != 1)
            {
                throw new ArgumentException("Only a single verb can be specified", nameof(verb));
            }

            return requireAuthentication ?
                WithVerbs(AuthenticatedVerbs | verb) :
                WithAnonymousVerbs(AnonymousVerbs | verb);
        }

        public EndpointConfiguration WithVerbs(HttpVerb verbs)
        {
            var overlap = AnonymousVerbs & verbs;
            if (overlap != HttpVerb.None)
            {
                throw ExceptionUtilities.VerbsDefinedAsAnonymousAndAuthenticated(overlap);
            }

            AuthenticatedVerbs = verbs;
            return this;
        }

        public EndpointConfiguration WithAnonymousVerbs(HttpVerb verbs)
        {
            var overlap = AuthenticatedVerbs & verbs;
            if (overlap != HttpVerb.None)
            {
                throw ExceptionUtilities.VerbsDefinedAsAnonymousAndAuthenticated(overlap);
            }

            AnonymousVerbs = verbs;
            return this;
        }

        public EndpointConfiguration WithOptions(Action<IEndpointConventionBuilder> builderOptions)
        {
            CustomUserOptions = builderOptions;
            return this;
        }
    }
}
