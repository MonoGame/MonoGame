using NUnit.Framework;
using System.Collections.Generic;

namespace MonoGame.Tests.ContentPipeline
{
    class XmlContentCompilerTest
    {
        [Test]
        public void ShouldSerializePropertyGivenNameOfItem()
        {
            var expected = new HasNoIndexer { Item = "lorem-ipsum" };

            TestCompiler.CompileAndLoadAssets(expected, result =>
            {
                Assert.AreEqual(expected.Item, result.Item);
            });
        }

        class HasNoIndexer
        {
            public string Item { get; set; }
        }

        [Test]
        public void ShouldNotSerializePropertyGivenIndexer()
        {
            var expected = new HasIndexer();
            expected["anything"] = "value";

            TestCompiler.CompileAndLoadAssets(expected, result =>
            {
                Assert.AreNotEqual(expected["anything"], result["anything"]);
            });
        }

        class HasIndexer
        {
            readonly Dictionary<string, string> _dictionary = new Dictionary<string, string>();
            public string this[string key]
            {
                get => _dictionary.TryGetValue(key, out string value) ? value : null;
                set => _dictionary[key] = value;
            }
        }
    }
}
