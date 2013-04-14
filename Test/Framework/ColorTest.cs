using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class ColorTest
    {
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
            // Test zero and underflow.
            Assert.AreEqual(Color.Transparent, Color.Lerp(Color.Transparent, Color.White, 0.0f));
            Assert.AreEqual(Color.Transparent, Color.Lerp(Color.Transparent, Color.White, 0.001f));
            Assert.AreEqual(Color.Transparent, Color.Lerp(Color.Transparent, Color.White, -0.001f));
            Assert.AreEqual(Color.Transparent, Color.Lerp(Color.Transparent, Color.White, -1.0f));

            // Test one scale and overflows.
            Assert.AreEqual(Color.White, Color.Lerp(Color.Transparent, Color.White, 1.0f));
            Assert.AreEqual(Color.White, Color.Lerp(Color.Transparent, Color.White, 1.001f));
            Assert.AreEqual(new Color(254, 254, 254, 254), Color.Lerp(Color.Transparent, Color.White, 0.999f));
            Assert.AreEqual(Color.White, Color.Lerp(Color.Transparent, Color.White, 2.0f));

            // Test half scale.
            var half = new Color(127, 127, 127, 127);
            Assert.AreEqual(half, Color.Lerp(Color.Transparent, Color.White, 0.5f));
            Assert.AreEqual(half, Color.Lerp(Color.Transparent, Color.White, 0.501f));
            Assert.AreEqual(half, Color.Lerp(Color.Transparent, Color.White, 0.499f));

            // Test backwards lerp.
            Assert.AreEqual(Color.White, Color.Lerp(Color.White, Color.Transparent, 0.0f));
            Assert.AreEqual(Color.Transparent, Color.Lerp(Color.White, Color.Transparent, 1.0f));
            Assert.AreEqual(half, Color.Lerp(Color.White, Color.Transparent, 0.5f));
        }
    }
}
