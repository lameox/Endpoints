using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Lameox.Endpoints.Tests
{
    public class ValueParserTests
    {
        private static void TestParser<T>(string input, Action<bool, T?> assertResult)
        {
            if (!ValueParser<T>.HasParser)
            {
                assertResult(false, default);
                return;
            }

            var parser = ValueParser<T>.TryParseValue!;
            var result = parser(input, out var parsedObject);

            if (result)
            {
                var typedObject = Assert.IsType<T>(parsedObject);
                assertResult(true, typedObject);
            }
            else
            {
                Assert.Equal(default(T), parsedObject);
                assertResult(false, default);
            }
        }

        private class WithConstructor
        {
            public string Value { get; }
            public WithConstructor(string value)
            {
                Value = value;
            }
        }

        [Fact]
        public void ConstructorIsUsed()
        {
            TestParser<WithConstructor>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class WithDeserialize
        {
            public string Value { get; }

            private WithDeserialize(string value)
            {
                Value = value;
            }

            public static WithDeserialize Deserialize(string input)
            {
                return new WithDeserialize(input);
            }
        }

        [Fact]
        public void DeserializeIsUsed()
        {
            TestParser<WithDeserialize>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class WithCreate
        {
            public string Value { get; }

            private WithCreate(string value)
            {
                Value = value;
            }

            public static WithCreate Create(string input)
            {
                return new WithCreate(input);
            }
        }

        [Fact]
        public void CreateIsUsed()
        {
            TestParser<WithCreate>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class WithParse
        {
            public string Value { get; }

            private WithParse(string value)
            {
                Value = value;
            }

            public static WithParse Parse(string input)
            {
                return new WithParse(input);
            }
        }

        [Fact]
        public void ParseIsUsed()
        {
            TestParser<WithParse>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class WithFromString
        {
            public string Value { get; }

            private WithFromString(string value)
            {
                Value = value;
            }

            public static WithFromString FromString(string input)
            {
                return new WithFromString(input);
            }
        }

        [Fact]
        public void FromStringIsUsed()
        {
            TestParser<WithFromString>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class WithTryDeserialize
        {
            public string Value { get; }

            private WithTryDeserialize(string value)
            {
                Value = value;
            }

            public static bool TryDeserialize(string input, out WithTryDeserialize? value)
            {
                value = new WithTryDeserialize(input);
                return true;
            }
        }

        [Fact]
        public void TryDeserializeIsUsed()
        {
            TestParser<WithTryDeserialize>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class WithTryCreate
        {
            public string Value { get; }

            private WithTryCreate(string value)
            {
                Value = value;
            }

            public static bool TryCreate(string input, out WithTryCreate? value)
            {
                value = new WithTryCreate(input);
                return true;
            }
        }

        [Fact]
        public void TryCreateIsUsed()
        {
            TestParser<WithTryCreate>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class WithTryParse
        {
            public string Value { get; }

            private WithTryParse(string value)
            {
                Value = value;
            }

            public static bool TryParse(string input, out WithTryParse? value)
            {
                value = new WithTryParse(input);
                return true;
            }
        }

        [Fact]
        public void TryParseIsUsed()
        {
            TestParser<WithTryParse>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class PrecedenceOfTryParse
        {
            public string Value { get; }

            public PrecedenceOfTryParse(string value)
            {
                _ = value;
                throw new InvalidOperationException();
            }

            private PrecedenceOfTryParse(string value, bool dummy)
            {
                _ = dummy;
                Value = value;
            }

            public static PrecedenceOfTryParse FromString(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static PrecedenceOfTryParse Parse(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static PrecedenceOfTryParse Create(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static PrecedenceOfTryParse Deserialize(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static bool TryParse(string input, out PrecedenceOfTryParse? value)
            {
                value = new PrecedenceOfTryParse(input, false);
                return true;
            }
        }

        [Fact]
        public void TryParseHasPrecedenceOverNonTryMethods()
        {
            TestParser<PrecedenceOfTryParse>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class PrecedenceOfTryCreate
        {
            public string Value { get; }

            public PrecedenceOfTryCreate(string value)
            {
                _ = value;
                throw new InvalidOperationException();
            }

            private PrecedenceOfTryCreate(string value, bool dummy)
            {
                _ = dummy;
                Value = value;
            }

            public static PrecedenceOfTryCreate FromString(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static PrecedenceOfTryCreate Parse(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static PrecedenceOfTryCreate Create(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static PrecedenceOfTryCreate Deserialize(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static bool TryCreate(string input, out PrecedenceOfTryCreate? value)
            {
                value = new PrecedenceOfTryCreate(input, false);
                return true;
            }
        }

        [Fact]
        public void TryCreateHasPrecedenceOverNonTryMethods()
        {
            TestParser<PrecedenceOfTryCreate>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class PrecedenceOfTryDeserialize
        {
            public string Value { get; }

            public PrecedenceOfTryDeserialize(string value)
            {
                _ = value;
                throw new InvalidOperationException();
            }

            private PrecedenceOfTryDeserialize(string value, bool dummy)
            {
                _ = dummy;
                Value = value;
            }

            public static PrecedenceOfTryDeserialize FromString(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static PrecedenceOfTryDeserialize Parse(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static PrecedenceOfTryDeserialize Create(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static PrecedenceOfTryDeserialize Deserialize(string input)
            {
                _ = input;
                throw new InvalidOperationException();
            }

            public static bool TryDeserialize(string input, out PrecedenceOfTryDeserialize? value)
            {
                value = new PrecedenceOfTryDeserialize(input, false);
                return true;
            }
        }

        [Fact]
        public void TryDeserializeHasPrecedenceOverNonTryMethods()
        {
            TestParser<PrecedenceOfTryDeserialize>("A", (result, value) =>
            {
                Assert.True(result);
                Assert.Equal("A", value!.Value);
            });
        }

        private class HasNoMethods
        {

        }

        private class HasNoPublicMethods
        {
            public string Value { get; }
            protected HasNoPublicMethods(string value)
            {
                Value = value;
            }

            internal static bool TryDeserialize(string input, out HasNoPublicMethods? value)
            {
                value = new HasNoPublicMethods(input);
                return true;
            }
        }

        [Fact]
        public void ParserFailsForClassesWithoutAppropriateMethods()
        {
            TestParser<HasNoMethods>("A", (result, value) =>
            {
                Assert.False(result);
                Assert.Null(value);
            });

            TestParser<HasNoPublicMethods>("A", (result, value) =>
            {
                Assert.False(result);
                Assert.Null(value);
            });
        }

        private class Throws
        {
            public string Value { get; }
            private Throws(string value)
            {
                Value = value;
            }

            internal static bool TryDeserialize(string input, out Throws? value)
            {
                value = new Throws(input);
                throw new InvalidOperationException();
            }
        }

        [Fact]
        public void ParserFailsForThrowingMethodsAndReturnsNull()
        {
            TestParser<Throws>("A", (result, value) =>
            {
                Assert.False(result);
                Assert.Null(value);
            });
        }
    }
}
