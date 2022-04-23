using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    internal class EndpointSerializationOptions
    {
        public JsonSerializerOptions JsonOptions { get; }

        public EndpointSerializationOptions(JsonSerializerOptions jsonOptions)
        {
            JsonOptions = jsonOptions;
        }
    }
}
