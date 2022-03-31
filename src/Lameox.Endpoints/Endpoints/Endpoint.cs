using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    public abstract class Endpoint<TRequest, TResponse>
        where TRequest : notnull, new()
        where TResponse : notnull, new()
    {
        private EndpointConfiguration? _lazyConfiguration;
        public EndpointConfiguration Configuration => _lazyConfiguration ??= InitializeConfiguration();

        protected abstract EndpointConfiguration Configure(EndpointConfiguration configuration);
        private EndpointConfiguration InitializeConfiguration()
        {
            return Configure(new EndpointConfiguration());
        }


    }
}
