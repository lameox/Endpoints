using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lameox.Endpoints
{
    internal static class HttpVerbs
    {
        private const string GetVerb = "GET";
        private const string PostVerb = "POST";
        private const string PutVerb = "PUT";
        private const string DeleteVerb = "DELETE";

        private static ImmutableDictionary<HttpVerb, ImmutableArray<string>> _cache = ImmutableDictionary<HttpVerb, ImmutableArray<string>>.Empty;

        public static IEnumerable<string> GetMethods(HttpVerb verbs)
        {
            return ImmutableInterlocked.GetOrAdd(ref _cache, verbs, CreateEntryForVerbs);
        }

        private static ImmutableArray<string> CreateEntryForVerbs(HttpVerb verbs)
        {
            if (verbs == HttpVerb.None)
            {
                return ImmutableArray<string>.Empty;
            }

            var builder = ImmutableArray.CreateBuilder<string>();

            if (verbs.HasFlag(HttpVerb.Get))
            {
                builder.Add(GetVerb);
            }

            if (verbs.HasFlag(HttpVerb.Post))
            {
                builder.Add(PostVerb);
            }

            if (verbs.HasFlag(HttpVerb.Put))
            {
                builder.Add(PutVerb);
            }

            if (verbs.HasFlag(HttpVerb.Delete))
            {
                builder.Add(DeleteVerb);
            }

            return builder.ToImmutable();
        }
    }
}
