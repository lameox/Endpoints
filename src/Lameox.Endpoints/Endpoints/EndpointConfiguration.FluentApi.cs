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

        public EndpointConfiguration WithVerbs(HttpVerb verbs)
        {
            Verbs = verbs;
            return this;
        }
    }
}
