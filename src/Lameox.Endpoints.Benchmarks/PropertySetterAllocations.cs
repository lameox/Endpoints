using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Lameox.Endpoints.Benchmarks
{
    [MemoryDiagnoser]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "BenchmarkDotNet needs instance methods")]
    public class PropertySetterAllocations
    {
        private class A
        {
            public string Value { get; set; }
            public A(string value)
            {
                Value = value;
            }

            public static bool TryParse(string input, out A? value)
            {
                value = new A(input);
                return true;
            }
        }

        private class ReferenceType
        {
            public A ReferenceTypeValue { get; init; } = null!;
            public Guid ValueTypeValue { get; init; }
        }

        private readonly struct ValueType
        {
            public A ReferenceTypeValue { get; init; }
            public Guid ValueTypeValue { get; init; }
        }

        const int Iterations = 100000;

        private static readonly string s_input = Guid.NewGuid().ToString();

        private static readonly Binder<ReferenceType>.PropertySetter s_referenceTypeReferenceType =
            Binder<ReferenceType>.PropertySetter.Create(typeof(ReferenceType).GetProperty(nameof(ReferenceType.ReferenceTypeValue))!);
        private static readonly Binder<ReferenceType>.PropertySetter s_referenceTypeValueType =
            Binder<ReferenceType>.PropertySetter.Create(typeof(ReferenceType).GetProperty(nameof(ReferenceType.ValueTypeValue))!);
        private static readonly Binder<ValueType>.PropertySetter s_valueTypeReferenceType =
            Binder<ValueType>.PropertySetter.Create(typeof(ValueType).GetProperty(nameof(ValueType.ReferenceTypeValue))!);
        private static readonly Binder<ValueType>.PropertySetter s_valueTypeValueType =
            Binder<ValueType>.PropertySetter.Create(typeof(ValueType).GetProperty(nameof(ValueType.ValueTypeValue))!);

        private static ReferenceType s_referenceTypeInstance1 = new();
        private static ReferenceType s_referenceTypeInstance2 = new();
        private static ValueType s_valueTypeInstance1 = new();
        private static ValueType s_valueTypeInstance2 = new();

        [GlobalSetup]
        public void GlobalSetup()
        {
            //populate the lazy fields in the setter.

            s_referenceTypeReferenceType.TryParseAndSet(ref s_referenceTypeInstance1, s_input);
            s_referenceTypeValueType.TryParseAndSet(ref s_referenceTypeInstance2, s_input);
            s_referenceTypeValueType.TryParseAndSet(ref s_referenceTypeInstance2, s_input);
            s_valueTypeReferenceType.TryParseAndSet(ref s_valueTypeInstance1, s_input);
        }

        [Benchmark(Baseline = true)]
        public void GuidParse()
        {
            for (int i = 0; i < Iterations; i++)
            {
                Guid.TryParse(s_input, out _);
            }
        }

        [Benchmark]
        public void ReferenceType_ReferenceType()
        {
            for (int i = 0; i < Iterations; i++)
            {
                s_referenceTypeReferenceType.TryParseAndSet(ref s_referenceTypeInstance1, s_input);
            }
        }

        [Benchmark]
        public void ReferenceType_ValueType()
        {
            for (int i = 0; i < Iterations; i++)
            {
                s_referenceTypeValueType.TryParseAndSet(ref s_referenceTypeInstance2, s_input);
            }
        }

        [Benchmark]
        public void ValueType_ReferenceType()
        {
            for (int i = 0; i < Iterations; i++)
            {
                s_valueTypeReferenceType.TryParseAndSet(ref s_valueTypeInstance1, s_input);
            }
        }

        [Benchmark]
        public void ValueType_ValueType()
        {
            for (int i = 0; i < Iterations; i++)
            {
                s_valueTypeValueType.TryParseAndSet(ref s_valueTypeInstance2, s_input);
            }
        }
    }
}
