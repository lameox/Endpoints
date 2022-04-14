using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Lameox.Endpoints.Benchmarks
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "BenchmarkDotNet needs instance methods")]
    public class ValueParserOverhead
    {
        private const int Iterations = 100000;

        [Benchmark(Baseline = true)]
        public void Int()
        {
            for (int i = 0; i < Iterations; i++)
            {
                int.TryParse("19258292", out _);
            }
        }

        [Benchmark]
        public void ValueParser()
        {
            for (int i = 0; i < Iterations; i++)
            {
                ValueParser<int>.TryParseValue!("19258292", out _);
            }
        }
    }
}
