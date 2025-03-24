// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
#if XNA
using System.Reflection;
#endif

namespace MonoGame.Tests.ContentPipeline
{
    class ContentCompilerTest
    {
        class TestDataClass : IEquatable<TestDataClass>
        {
            public int A;
            public string B { get; set; }

            public bool Equals(TestDataClass other)
            {
                return A == other.A
                    && B == other.B;
            }

            public override bool Equals(object obj)
            {
                if (obj is TestDataClass)
                    return Equals(other: (TestDataClass)obj);
                else
                    return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return A.GetHashCode() ^ B.GetHashCode();
            }
        }
        struct TestDataStruct : IEquatable<TestDataStruct>
        {
            public int A;
            public string B { get; set; }

            public bool Equals(TestDataStruct other)
            {
                return A == other.A
                    && B == other.B;
            }

            public override bool Equals(object obj)
            {
                if (obj is TestDataStruct)
                    return Equals(other: (TestDataStruct)obj);
                else
                    return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return A.GetHashCode() ^ B.GetHashCode();
            }
        }
        class TestDataContainer
        {
            public Microsoft.Xna.Framework.Rectangle[,] Array2D { get; set; }
            public Microsoft.Xna.Framework.Rectangle[,,] Array3D { get; set; }

            public TestDataContainer DeepClone()
            {
                return new TestDataContainer
                {
                    Array2D = (Microsoft.Xna.Framework.Rectangle[,])Array2D.Clone(),
                    Array3D = (Microsoft.Xna.Framework.Rectangle[,,])Array3D.Clone()
                };
            }
        }

        [Test]
        public void RoundTripJaggedArrayClass()
        {
            var expected = Enumerable.Range(0, 5).Select(a => Enumerable.Range(0, 3).Select(b => new TestDataClass { A = a * 42 << b, B = "index: " + a * 5 + b }).ToArray()).ToArray();

            TestCompiler.CompileAndLoadAssets(expected.ToArray(), result =>
            {
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<TestDataClass[][]>(result);
                Assert.AreEqual(1, result.Rank);
                Assert.AreEqual(expected.Length, result.Length);
                for (int i = 0; i < expected.Length; i++)
                    CollectionAssert.AreEquivalent(expected[i], result[i]);
            });

        }

        [Test]
        public void RoundTrip1DArrayClass()
        {
            var expected = Enumerable.Range(0, 10).Select(i => new TestDataClass { A = i * 42, B = "index: " + i }).ToArray();

            TestCompiler.CompileAndLoadAssets(expected.ToArray(), result =>
            {
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<TestDataClass[]>(result);
                Assert.AreEqual(1, result.Rank);
                Assert.AreEqual(expected.Length, result.Length);
                CollectionAssert.AreEquivalent(expected, result);
            });

        }

        [Test]
        public void RoundTrip3DArrayClass()
        {
            var expected = new TestDataClass[3, 4, 2];
            for (int z = 0; z < expected.GetLength(2); z++)
                for (int y = 0; y < expected.GetLength(1); y++)
                    for (int x = 0; x < expected.GetLength(0); x++)
                        expected[x, y, z] = new TestDataClass { A = x + y * 10 + z * 100, B = string.Format("X: {0} Y: {1} Z: {2}", x, y, z) };

            TestCompiler.CompileAndLoadAssets((TestDataClass[,,])expected.Clone(), result =>
            {
                Assert.IsNotNull(result);
                Assert.IsInstanceOf<TestDataClass[,,]>(result);
                Assert.AreEqual(result.Rank, 3);

                for (int i = 0; i < result.Rank; i++)
                    Assert.AreEqual(expected.GetLength(i), result.GetLength(i));

                for (int z = 0; z < expected.GetLength(2); z++)
                    for (int y = 0; y < expected.GetLength(1); y++)
                        for (int x = 0; x < expected.GetLength(0); x++)
                            Assert.AreEqual(expected[x, y, z], result[x, y, z]);
            });
        }

        [Test]
        public void RoundTripArrayContainer()
        {
            var expected = new TestDataContainer
            {
                Array2D = new Microsoft.Xna.Framework.Rectangle[3, 2],
                Array3D = new Microsoft.Xna.Framework.Rectangle[3, 4, 2]
            };
            for (int y = 0; y < expected.Array2D.GetLength(1); y++)
                for (int x = 0; x < expected.Array2D.GetLength(0); x++)
                    expected.Array2D[x, y] = new Microsoft.Xna.Framework.Rectangle(x, y, -x, -y);

            for (int z = 0; z < expected.Array3D.GetLength(2); z++)
                for (int y = 0; y < expected.Array3D.GetLength(1); y++)
                    for (int x = 0; x < expected.Array3D.GetLength(0); x++)
                        expected.Array3D[x, y, z] = new Microsoft.Xna.Framework.Rectangle(x, y, z, -z);

            TestCompiler.CompileAndLoadAssets(expected.DeepClone(), result =>
            {
                Assert.IsNotNull(result.Array3D);
                Assert.IsInstanceOf<Microsoft.Xna.Framework.Rectangle[,,]>(result.Array3D);
                Assert.AreEqual(result.Array3D.Rank, 3);

                for (int i = 0; i < result.Array3D.Rank; i++)
                    Assert.AreEqual(expected.Array3D.GetLength(i), result.Array3D.GetLength(i));

                for (int z = 0; z < expected.Array3D.GetLength(2); z++)
                    for (int y = 0; y < expected.Array3D.GetLength(1); y++)
                        for (int x = 0; x < expected.Array3D.GetLength(0); x++)
                            Assert.AreEqual(expected.Array3D[x, y, z], result.Array3D[x, y, z]);

                Assert.IsNotNull(result.Array2D);
                Assert.IsInstanceOf<Microsoft.Xna.Framework.Rectangle[,]>(result.Array2D);
                Assert.AreEqual(result.Array2D.Rank, 2);

                for (int i = 0; i < result.Array2D.Rank; i++)
                    Assert.AreEqual(expected.Array2D.GetLength(i), result.Array2D.GetLength(i));

                for (int y = 0; y < expected.Array2D.GetLength(1); y++)
                    for (int x = 0; x < expected.Array2D.GetLength(0); x++)
                        Assert.AreEqual(expected.Array2D[x, y], result.Array2D[x, y]);
            });
        }

        [Test]
        public void RoundTrip1DArrayStruct()
        {
            var expected = Enumerable.Range(0, 10).Select(i => new TestDataStruct { A = i * 42, B = "index: " + i }).ToArray();

            TestCompiler.CompileAndLoadAssets(expected.ToArray(), result =>
            {
                Assert.IsInstanceOf<TestDataStruct[]>(result);
                Assert.AreEqual(1, result.Rank);
                Assert.AreEqual(expected.Length, result.Length);
                CollectionAssert.AreEquivalent(expected, result);
            });

        }

        [Test]
        public void RoundTrip3DArrayStruct()
        {
            var expected = new TestDataStruct[3, 4, 2];
            for (int z = 0; z < expected.GetLength(2); z++)
                for (int y = 0; y < expected.GetLength(1); y++)
                    for (int x = 0; x < expected.GetLength(0); x++)
                        expected[x, y, z] = new TestDataStruct { A = x + y * 10 + z * 100, B = string.Format("X: {0} Y: {1} Z: {2}", x, y, z) };

            TestCompiler.CompileAndLoadAssets((TestDataStruct[,,])expected.Clone(), result =>
            {
                Assert.IsInstanceOf<TestDataStruct[,,]>(result);
                Assert.AreEqual(result.Rank, 3);

                for (int i = 0; i < result.Rank; i++)
                    Assert.AreEqual(expected.GetLength(i), result.GetLength(i));

                for (int z = 0; z < expected.GetLength(2); z++)
                    for (int y = 0; y < expected.GetLength(1); y++)
                        for (int x = 0; x < expected.GetLength(0); x++)
                            Assert.AreEqual(expected[x, y, z], result[x, y, z]);
            });
        }

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
                get
                { 
                    string value;
                    return _dictionary.TryGetValue(key, out value) ? value : null;
                }
                set { _dictionary[key] = value; }
            }
        }
    }
}
