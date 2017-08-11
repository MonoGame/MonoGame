// MonoGame - Copyright (C) The MonoGame Team
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
    class ArrayContentCompilerTest
    {
        class TestContentManager : ContentManager
        {
            class FakeGraphicsService : IGraphicsDeviceService
            {
                public GraphicsDevice GraphicsDevice { get; private set; }

#pragma warning disable 67
                public event EventHandler<EventArgs> DeviceCreated;
                public event EventHandler<EventArgs> DeviceDisposing;
                public event EventHandler<EventArgs> DeviceReset;
                public event EventHandler<EventArgs> DeviceResetting;
#pragma warning restore 67
            }

            class FakeServiceProvider : IServiceProvider
            {
                public object GetService(Type serviceType)
                {
                    if (serviceType == typeof(IGraphicsDeviceService))
                        return new FakeGraphicsService();

                    throw new NotImplementedException();
                }
            }

            private readonly MemoryStream _xnbStream;

            public TestContentManager(MemoryStream xnbStream)
                : base(new FakeServiceProvider(), "NONE")
            {
                _xnbStream = xnbStream;
            }

            protected override Stream OpenStream(string assetName)
            {
                return new MemoryStream(_xnbStream.GetBuffer(), false);
            }
        }
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

        static readonly IReadOnlyCollection<TargetPlatform> Platforms = new[]
        {
            TargetPlatform.Windows,
            TargetPlatform.Xbox360,
            TargetPlatform.iOS,
            TargetPlatform.Android,
            TargetPlatform.DesktopGL,
            TargetPlatform.MacOSX,
            TargetPlatform.WindowsStoreApp,
            TargetPlatform.NativeClient,

            TargetPlatform.PlayStationMobile,

            TargetPlatform.WindowsPhone8,
            TargetPlatform.RaspberryPi,
            TargetPlatform.PlayStation4,
            TargetPlatform.PSVita,
            TargetPlatform.XboxOne,
            TargetPlatform.Switch
        };
        static readonly IReadOnlyCollection<GraphicsProfile> GraphicsProfiles = new[]
        {
            GraphicsProfile.HiDef,
            GraphicsProfile.Reach
        };
        static readonly IReadOnlyCollection<bool> CompressContents = new[]
        {
            true,
            false
        };

        ContentCompiler Compiler = new ContentCompiler();


        void CompileAndLoadAssets<T>(T data, Action<T> validation)
        {
            foreach (var platform in Platforms)
                foreach (var gfxProfile in GraphicsProfiles)
                    foreach (var compress in CompressContents)
                        using (var xnbStream = new MemoryStream())
                        {
                            Compiler.Compile(xnbStream, data, TargetPlatform.Windows, GraphicsProfile.HiDef, compress, "", "");
                            using (var content = new TestContentManager(xnbStream))
                            {
                                var result = content.Load<T>("foo");
                                validation(result);
                            }
                        }
        }

        [Test]
        public void RoundTripJaggedArrayClass()
        {
            var expected = Enumerable.Range(0, 5).Select(a => Enumerable.Range(0, 3).Select(b => new TestDataClass { A = a * 42 << b, B = "index: " + a * 5 + b }).ToArray()).ToArray();

            CompileAndLoadAssets(expected.ToArray(), result =>
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

            CompileAndLoadAssets(expected.ToArray(), result =>
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

            CompileAndLoadAssets((TestDataClass[,,])expected.Clone(), result =>
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
        public void RoundTrip1DArrayStruct()
        {
            var expected = Enumerable.Range(0, 10).Select(i => new TestDataStruct { A = i * 42, B = "index: " + i }).ToArray();

            CompileAndLoadAssets(expected.ToArray(), result =>
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

            CompileAndLoadAssets((TestDataStruct[,,])expected.Clone(), result =>
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
    }
}
