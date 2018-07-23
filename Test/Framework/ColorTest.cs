using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class ColorTest
    {
        // Contains a test case for each constructor type
        private static object[] _ctorTestCases =
        {
#if !XNA
            new object[] { new Color(new Color(64, 128, 192), 32), 64, 128, 192, 32 },
            new object[] { new Color(new Color(64, 128, 192), 256), 64, 128, 192, 255 },
            new object[] { new Color(new Color(64, 128, 192), 0.125f), 64, 128, 192, 32 },
            new object[] { new Color(new Color(64, 128, 192), 1.1f), 64, 128, 192, 255 },
            new object[] { new Color((byte)64, (byte)128, (byte)192, (byte)32), 64, 128, 192, 32 },
#endif
            new object[] { new Color(), 0, 0, 0, 0 },
            new object[] { new Color(64, 128, 192), 64, 128, 192, 255 },
            new object[] { new Color(256, 256, -1), 255, 255, 0, 255},
            new object[] { new Color(64, 128, 192, 32), 64, 128, 192, 32 },
            new object[] { new Color(256, 256, -1, 256), 255, 255, 0, 255},
            new object[] { new Color(0.25f, 0.5f, 0.75f), 64, 128, 192, 255 },
            new object[] { new Color(1.1f, 1.1f, -0.1f), 255, 255, 0, 255 },
            new object[] { new Color(0.25f, 0.5f, 0.75f, 0.125f), 64, 128, 192, 32 },
            new object[] { new Color(1.1f, 1.1f, -0.1f, -0.1f), 255, 255, 0, 0 },
            new object[] { new Color(new Vector3(0.25f, 0.5f, 0.75f)), 64, 128, 192, 255 },
            new object[] { new Color(new Vector3(1.1f, 1.1f, -0.1f)), 255, 255, 0, 255 },
            new object[] { new Color(new Vector4(0.25f, 0.5f, 0.75f, 0.125f)), 64, 128, 192, 32 },
            new object[] { new Color(new Vector4(1.1f, 1.1f, -0.1f, -0.1f)), 255, 255, 0, 0 }
        };

        [Test, TestCaseSource("_ctorTestCases")]
        public void Ctor_Explicit(Color color, int expectedR, int expectedG, int expectedB, int expectedA)
        {
            // Account for rounding differences with float constructors
            Assert.That(color.R, Is.EqualTo(expectedR).Within(1));
            Assert.That(color.G, Is.EqualTo(expectedG).Within(1));
            Assert.That(color.B, Is.EqualTo(expectedB).Within(1));
            Assert.That(color.A, Is.EqualTo(expectedA).Within(1));
        }

#if !XNA
        [Test]
        public void Ctor_Packed()
        {
            var color = new Color(0x20C08040);

            Assert.That(color.R, Is.EqualTo(64));
            Assert.That(color.G, Is.EqualTo(128));
            Assert.That(color.B, Is.EqualTo(192));
            Assert.That(color.A, Is.EqualTo(32));
        }
#endif

        [Test]
        public void FromNonPremultiplied_Int()
        {
            var color = Color.FromNonPremultiplied(255, 128, 64, 128);
            Assert.That(color.R, Is.EqualTo(128).Within(1));
            Assert.That(color.G, Is.EqualTo(64).Within(1));
            Assert.That(color.B, Is.EqualTo(32).Within(1));
            Assert.That(color.A, Is.EqualTo(128).Within(0));

            var overflow = Color.FromNonPremultiplied(280, 128, -10, 128);
            Assert.That(overflow.R, Is.EqualTo(140).Within(1));
            Assert.That(overflow.G, Is.EqualTo(64).Within(1));
            Assert.That(overflow.B, Is.EqualTo(0).Within(1));
            Assert.That(overflow.A, Is.EqualTo(128).Within(0));

            var overflow2 = Color.FromNonPremultiplied(255, 128, 64, 280);
            Assert.That(overflow2.R, Is.EqualTo(255).Within(1));
            Assert.That(overflow2.G, Is.EqualTo(140).Within(1));
            Assert.That(overflow2.B, Is.EqualTo(70).Within(1));
            Assert.That(overflow2.A, Is.EqualTo(255).Within(0));
        }

        [Test]
        public void FromNonPremultiplied_Float()
        {
            var color = Color.FromNonPremultiplied(new Vector4(1.0f, 0.5f, 0.25f, 0.5f));
            Assert.That(color.R, Is.EqualTo(128).Within(1));
            Assert.That(color.G, Is.EqualTo(64).Within(1));
            Assert.That(color.B, Is.EqualTo(32).Within(1));
            Assert.That(color.A, Is.EqualTo(128).Within(1));

            var overflow = Color.FromNonPremultiplied(new Vector4(1.1f, 0.5f, -0.1f, 0.5f));
            Assert.That(overflow.R, Is.EqualTo(140).Within(1));
            Assert.That(overflow.G, Is.EqualTo(64).Within(1));
            Assert.That(overflow.B, Is.EqualTo(0).Within(1));
            Assert.That(overflow.A, Is.EqualTo(128).Within(1));

            var overflow2 = Color.FromNonPremultiplied(new Vector4(1f, 0.5f, 0.25f, 1.1f));
            Assert.That(overflow2.R, Is.EqualTo(255).Within(1));
            Assert.That(overflow2.G, Is.EqualTo(140).Within(1));
            Assert.That(overflow2.B, Is.EqualTo(70).Within(1));
            Assert.That(overflow2.A, Is.EqualTo(255).Within(1));
        }

        [Test]
        public void Multiply()
        {
            var color = new Color(1, 2, 3, 4);

            // Test 1.0 scale.
            Assert.AreEqual(color, color * 1.0f);
            Assert.AreEqual(color, Color.Multiply(color, 1.0f));
            Assert.AreEqual(color * 1.0f, Color.Multiply(color, 1.0f));

            // Test 0.999 scale.
            var almostOne = new Color(0, 1, 2, 3);
            Assert.AreEqual(almostOne, color * 0.999f);
            Assert.AreEqual(almostOne, Color.Multiply(color, 0.999f));
            Assert.AreEqual(color * 0.999f, Color.Multiply(color, 0.999f));

            // Test 1.001 scale.
            Assert.AreEqual(color, color * 1.001f);
            Assert.AreEqual(color, Color.Multiply(color, 1.001f));
            Assert.AreEqual(color * 1.001f, Color.Multiply(color, 1.001f));

            // Test 0.0 scale.
            Assert.AreEqual(Color.Transparent, color * 0.0f);
            Assert.AreEqual(Color.Transparent, Color.Multiply(color, 0.0f));
            Assert.AreEqual(color * 0.0f, Color.Multiply(color, 0.0f));

            // Test 0.001 scale.
            Assert.AreEqual(Color.Transparent, color * 0.001f);
            Assert.AreEqual(Color.Transparent, Color.Multiply(color, 0.001f));
            Assert.AreEqual(color * 0.001f, Color.Multiply(color, 0.001f));

            // Test -0.001 scale.
            Assert.AreEqual(Color.Transparent, color * -0.001f);
            Assert.AreEqual(Color.Transparent, Color.Multiply(color, -0.001f));
            Assert.AreEqual(color * -0.001f, Color.Multiply(color, -0.001f));

            // Test for overflow.
            Assert.AreEqual(Color.White, color * 300.0f);
            Assert.AreEqual(Color.White, Color.Multiply(color, 300.0f));
            Assert.AreEqual(color * 300.0f, Color.Multiply(color, 300.0f));

            // Test for underflow.
            Assert.AreEqual(Color.Transparent, color * -1.0f);
            Assert.AreEqual(Color.Transparent, Color.Multiply(color, -1.0f));
            Assert.AreEqual(color * -1.0f, Color.Multiply(color, -1.0f));
        }

        [Test]
        public void Lerp()
        {
            Color color1 = new Color(20, 40, 0, 0);
            Color color2 = new Color(41, 81, 255, 255);

            // Test zero and underflow.
            Assert.AreEqual(color1, Color.Lerp(color1, color2, 0.0f));
            Assert.AreEqual(color1, Color.Lerp(color1, color2, 0.001f));
            Assert.AreEqual(color1, Color.Lerp(color1, color2, -0.001f));
            Assert.AreEqual(color1, Color.Lerp(color1, color2, -1.0f));

            // Test one scale and overflows.
            Assert.AreEqual(color2, Color.Lerp(color1, color2, 1.0f));
            Assert.AreEqual(color2, Color.Lerp(color1, color2, 1.001f));
            Assert.AreEqual(new Color(254, 254, 254, 254), Color.Lerp(Color.Transparent, Color.White, 0.999f));
            Assert.AreEqual(color2, Color.Lerp(color1, color2, 2.0f));

            // Test half scale.
            var half = new Color(30, 60, 127, 127);
            Assert.AreEqual(half, Color.Lerp(color1, color2, 0.5f));
            Assert.AreEqual(half, Color.Lerp(color1, color2, 0.501f));
            Assert.AreEqual(half, Color.Lerp(color1, color2, 0.499f));

            // Test backwards lerp.
            Assert.AreEqual(color2, Color.Lerp(color2, color1, 0.0f));
            Assert.AreEqual(color1, Color.Lerp(color2, color1, 1.0f));
            Assert.AreEqual(half, Color.Lerp(color2, color1, 0.5f));
        }

#if !XNA
        [Test]
        public void Deconstruct()
        {
            Color color = new Color(255, 255, 255);

            float r, g, b;

            color.Deconstruct(out r, out g, out b);

            Assert.AreEqual(r, color.R);
            Assert.AreEqual(g, color.G);
            Assert.AreEqual(b, color.B);

            Color color2 = new Color(255, 255, 255, 255);

            float r2, g2, b2, a2;

            color2.Deconstruct(out r2, out g2, out b2, out a2);

            Assert.AreEqual(r2, color2.R);
            Assert.AreEqual(g2, color2.G);
            Assert.AreEqual(b2, color2.B);
            Assert.AreEqual(a2, color2.A);
        }
#endif
    }
}
