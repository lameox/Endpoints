using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Lameox.Endpoints
{
    public interface IEndpoint
    {
        EndpointConfiguration Configuration { get; }
        ValueTask HandleRequestAsync(HttpContext requestContext, CancellationToken cancellationToken);
    }
}
