using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    public class MathHelperTest
    {
        [Test]
        public void ClampFloatTest()
        {
            Assert.True(MathHelper.Clamp(1f, 0f, 2f) == 1f, "Failed boundary test, clamp [0,2] on 1 should be 1");
            Assert.True(MathHelper.Clamp(1f, 0f, 1f) == 1f, "Failed boundary test, clamp [0,1] on 1 should be 1");
            Assert.True(MathHelper.Clamp(1f, 2f, 5f) == 2f, "Failed boundary test, clamp [2,5] on 1 should be 2");
            Assert.True(MathHelper.Clamp(1f, -50f, -10f) == -10f, "Failed boundary test, clamp [-50f, -10f] on 1 should be -10");
            Assert.True(MathHelper.Clamp(1f, -50f, 25f) == 1f, "Failed boundary test, clamp [-50, 25] on 1 should be 1");
            Assert.True(MathHelper.Clamp(0f, 1f, 1f) == 1f, "Failed boundary test, clamp [1,1] on 0 should be 1");
            Assert.True(MathHelper.Clamp(0f, -1f, -1f) == -1f, "Failed boundary test, clamp [-1,-1] on 0 should be -1");
        }

        [Test]
        public void ClampIntTest()
        {
            Assert.True(MathHelper.Clamp(1, 0, 2) == 1, "Failed boundary test, clamp [0,2] on 1 should be 1");
            Assert.True(MathHelper.Clamp(1, 0, 1) == 1, "Failed boundary test, clamp [0,1] on 1 should be 1");
            Assert.True(MathHelper.Clamp(1, 2, 5) == 2, "Failed boundary test, clamp [2,5] on 1 should be 2");
            Assert.True(MathHelper.Clamp(1, -50, -10) == -10, "Failed boundary test, clamp [-50f, -10f] on 1 should be -10");
            Assert.True(MathHelper.Clamp(1, -50, 25) == 1, "Failed boundary test, clamp [-50, 25] on 1 should be 1");
            Assert.True(MathHelper.Clamp(0, 1, 1) == 1, "Failed boundary test, clamp [1,1] on 0 should be 1");
            Assert.True(MathHelper.Clamp(0, -1, -1) == -1, "Failed boundary test, clamp [-1,-1] on 0 should be -1");
        }

        [Test]
        public void DistanceTest()
        {
            Assert.AreEqual(MathHelper.Distance(0, 5f), 5f, "Distance test failed on [0,5]");
            Assert.AreEqual(MathHelper.Distance(-5f, 5f), 10f, "Distance test failed on [-5,5]");
            Assert.AreEqual(MathHelper.Distance(0f, 0f), 0f, "Distance test failed on [0,0]");
            Assert.AreEqual(MathHelper.Distance(-5f, -1f), 4f, "Distance test failed on [-5,-1]");
        }

        [Test]
        public void LerpTest()
        {
            Assert.AreEqual(MathHelper.Lerp(0f, 5f, .5f), 2.5f, "Lerp test failed on [0,5,.5]");
            Assert.AreEqual(MathHelper.Lerp(-5f, 5f, 0.5f), 0f, "Lerp test failed on [-5,5,0.5]");
            Assert.AreEqual(MathHelper.Lerp(0f, 0f, 0.5f), 0f, "Lerp test failed on [0,0,0.5]");
            Assert.AreEqual(MathHelper.Lerp(-5f, -1f, 0.5f), -3f, "Lerp test failed on [-5,-1, 0.5]");
            // The following test checks for XNA compatibility. 
            // Even though the calculation itself should return "1", the XNA implementaion returns 0 (presumably due to a efficiency/precision tradeoff).
            Assert.AreEqual(MathHelper.Lerp(10000000000000000f, 1f, 1f), 0f, "Lerp test failed on [10000000000000000,1,1]");
        }

#if !XNA
        [Test]
        public void LerpPreciseTest()
        {
            Assert.AreEqual(MathHelper.LerpPrecise(0f, 5f, .5f), 2.5f, "LerpPrecise test failed on [0,5,.5]");
            Assert.AreEqual(MathHelper.LerpPrecise(-5f, 5f, 0.5f), 0f, "LerpPrecise test failed on [-5,5,0.5]");
            Assert.AreEqual(MathHelper.LerpPrecise(0f, 0f, 0.5f), 0f, "LerpPrecise test failed on [0,0,0.5]");
            Assert.AreEqual(MathHelper.LerpPrecise(-5f, -1f, 0.5f), -3f, "LerpPrecise test failed on [-5,-1, 0.5]");
            Assert.AreEqual(MathHelper.LerpPrecise(10000000000000000f, 1f, 1f), 1, "LerpPrecise test failed on [10000000000000000,1,1]");
        }
#endif

        [Test]
        public void Min()
        {
            Assert.AreEqual(-0.5f, MathHelper.Min(-0.5f, -0.5f));
            Assert.AreEqual(-0.5f, MathHelper.Min(-0.5f,0.0f));
            Assert.AreEqual(-0.5f, MathHelper.Min(0.0f, -0.5f));
            Assert.AreEqual(0, MathHelper.Min(0, 0));
            Assert.AreEqual(-5, MathHelper.Min(-5, 5));
            Assert.AreEqual(-5, MathHelper.Min(5, -5));
        }

        [Test]
        public void Max()
        {
            Assert.AreEqual(-0.5f, MathHelper.Min(-0.5f, -0.5f));
            Assert.AreEqual(0.0f, MathHelper.Max(-0.5f,0.0f));
            Assert.AreEqual(0.0f, MathHelper.Max(0.0f, -0.5f));
            Assert.AreEqual(0, MathHelper.Max(0, 0));
            Assert.AreEqual(5, MathHelper.Max(-5, 5));
            Assert.AreEqual(5, MathHelper.Max(5, -5));
        }

        [TestCase(MathHelper.PiOver4, 0.7853982f)]
        [TestCase(MathHelper.PiOver2, 1.5707964f)]
        [TestCase(MathHelper.Pi, 3.1415927f)]
        [TestCase(MathHelper.TwoPi, 6.2831855f)]
        public void PiConstantsAreExpectedValues(float actualValue, float expectedValue)
        {
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestCase(0f, 0f)]
        [TestCase(MathHelper.PiOver4, MathHelper.PiOver4)]
        [TestCase(-MathHelper.PiOver4, -MathHelper.PiOver4)]
        [TestCase(MathHelper.PiOver2, MathHelper.PiOver2)]
        [TestCase(-MathHelper.PiOver2, -MathHelper.PiOver2)]
        [TestCase(MathHelper.Pi, MathHelper.Pi)]
        [TestCase(-MathHelper.Pi, MathHelper.Pi)]
        [TestCase(MathHelper.TwoPi, 0f)]
        [TestCase(-MathHelper.TwoPi, 0f)]
        [TestCase(10f, -2.566371f)]
        [TestCase(-10f, 2.566371f)]
        // Pi boundaries
        [TestCase(3.1415927f, 3.1415927f)]
        [TestCase(3.141593f, -3.1415925f)]
        [TestCase(-3.1415925f, -3.1415925f)]
        [TestCase(-3.1415927f, 3.1415927f)]
        // 2 * Pi boundaries
        [TestCase(6.283185f, -4.7683716E-7f)]
        [TestCase(6.2831855f, 0f)]
        [TestCase(6.283186f, 4.7683716E-7f)]
        [TestCase(-6.283185f, 4.7683716E-7f)]
        [TestCase(-6.2831855f, 0f)]
        [TestCase(-6.283186f, -4.7683716E-7f)]
        // 3 * Pi boundaries
        [TestCase(9.424778f, 3.1415925f)]
        [TestCase(9.424779f, -3.141592f)]
        [TestCase(-9.424778f, -3.1415925f)]
        [TestCase(-9.424779f, 3.141592f)]
        // 4 * Pi boundaries
        [TestCase(12.56637f, -9.536743E-7f)]
        [TestCase(12.566371f, 0f)]
        [TestCase(12.566372f, 9.536743E-7f)]
        [TestCase(-12.56637f, 9.536743E-7f)]
        [TestCase(-12.566371f, 0f)]
        [TestCase(-12.566372f, -9.536743E-7f)]
        // 5 * Pi boundaries
        [TestCase(15.707963f, 3.141592f)]
        [TestCase(15.707964f, -3.1415925f)]
        [TestCase(-15.707963f, -3.141592f)]
        [TestCase(-15.707964f, 3.1415925f)]
        // 10 * Pi boundaries
        [TestCase(31.415926f, -1.4305115E-6f)]
        [TestCase(31.415928f, 4.7683716E-7f)]
        [TestCase(-31.415926f, 1.4305115E-6f)]
        [TestCase(-31.415928f, -4.7683716E-7f)]
        // 20 * Pi boundaries
        [TestCase(62.831852f, -2.861023E-6f)]
        [TestCase(62.831856f, 9.536743E-7f)]
        [TestCase(-62.831852f, 2.861023E-6f)]
        [TestCase(-62.831856f, -9.536743E-7f)]
        // 20000000 * Pi boundaries
        [TestCase(6.2831852E7f, -2.8202515f)]
        [TestCase(6.2831856E7f, 1.1797485f)]
        [TestCase(-6.2831852E7f, 2.8202515f)]
        [TestCase(-6.2831856E7f, -1.1797485f)]
        public void WrapAngleReturnsExpectedValues(float angle, float expectedValue)
        {
            var actualValue = MathHelper.WrapAngle(angle);
            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}
