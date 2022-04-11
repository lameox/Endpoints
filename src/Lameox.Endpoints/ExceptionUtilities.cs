using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    internal static class ExceptionUtilities
    {
        public static Exception Unreachable()
        {
            return new InvalidOperationException("This place in the code is thought to be unreachable.");
        }

        internal static Exception DidNotAddEndpoints()
        {
            throw new NotImplementedException();
        }

        internal static Exception BadOverrideInEndpoint()
        {
            return new InvalidOperationException("You must override one of the request handling methods of the endpoint.");
        }

        internal static Exception UnableToDeserializeRequest()
        {
            throw new InvalidOperationException("Failed to deserialize the request object.");
        }
    }
}
