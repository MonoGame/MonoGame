using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    public class MathHelperDTest
    {
        [Test]
        public void ClampDoubleTest()
        {
            Assert.True(MathHelperD.Clamp(1d, 0d, 2d) == 1f, "Failed boundary test, clamp [0,2] on 1 should be 1");
            Assert.True(MathHelperD.Clamp(1d, 0d, 1d) == 1f, "Failed boundary test, clamp [0,1] on 1 should be 1");
            Assert.True(MathHelperD.Clamp(1d, 2d, 5d) == 2f, "Failed boundary test, clamp [2,5] on 1 should be 2");
            Assert.True(MathHelperD.Clamp(1d, -50d, -10d) == -10f, "Failed boundary test, clamp [-50f, -10f] on 1 should be -10");
            Assert.True(MathHelperD.Clamp(1d, -50d, 25d) == 1f, "Failed boundary test, clamp [-50, 25] on 1 should be 1");
            Assert.True(MathHelperD.Clamp(0d, 1d, 1d) == 1f, "Failed boundary test, clamp [1,1] on 0 should be 1");
            Assert.True(MathHelperD.Clamp(0d, -1d, -1d) == -1f, "Failed boundary test, clamp [-1,-1] on 0 should be -1");
        }

        [Test]
        public void ClampIntTest()
        {
            Assert.True(MathHelperD.Clamp(1, 0, 2) == 1, "Failed boundary test, clamp [0,2] on 1 should be 1");
            Assert.True(MathHelperD.Clamp(1, 0, 1) == 1, "Failed boundary test, clamp [0,1] on 1 should be 1");
            Assert.True(MathHelperD.Clamp(1, 2, 5) == 2, "Failed boundary test, clamp [2,5] on 1 should be 2");
            Assert.True(MathHelperD.Clamp(1, -50, -10) == -10, "Failed boundary test, clamp [-50f, -10f] on 1 should be -10");
            Assert.True(MathHelperD.Clamp(1, -50, 25) == 1, "Failed boundary test, clamp [-50, 25] on 1 should be 1");
            Assert.True(MathHelperD.Clamp(0, 1, 1) == 1, "Failed boundary test, clamp [1,1] on 0 should be 1");
            Assert.True(MathHelperD.Clamp(0, -1, -1) == -1, "Failed boundary test, clamp [-1,-1] on 0 should be -1");
        }

        [Test]
        public void DistanceTest()
        {
            Assert.AreEqual(MathHelperD.Distance(0, 5d), 5d, "Distance test failed on [0,5]");
            Assert.AreEqual(MathHelperD.Distance(-5d, 5d), 10d, "Distance test failed on [-5,5]");
            Assert.AreEqual(MathHelperD.Distance(0d, 0d), 0d, "Distance test failed on [0,0]");
            Assert.AreEqual(MathHelperD.Distance(-5d, -1d), 4d, "Distance test failed on [-5,-1]");
        }

        [Test]
        public void LerpTest()
        {
            Assert.AreEqual(MathHelperD.Lerp(0d, 5d, .5d), 2.5d, "Lerp test failed on [0,5,.5]");
            Assert.AreEqual(MathHelperD.Lerp(-5d, 5d, 0.5d), 0d, "Lerp test failed on [-5,5,0.5]");
            Assert.AreEqual(MathHelperD.Lerp(0d, 0d, 0.5d), 0d, "Lerp test failed on [0,0,0.5]");
            Assert.AreEqual(MathHelperD.Lerp(-5d, -1d, 0.5d), -3d, "Lerp test failed on [-5,-1, 0.5]");
        }

#if !XNA
        [Test]
        public void LerpPreciseTest()
        {
            Assert.AreEqual(MathHelperD.LerpPrecise(0d, 5d, .5d), 2.5d, "LerpPrecise test failed on [0,5,.5]");
            Assert.AreEqual(MathHelperD.LerpPrecise(-5d, 5d, 0.5d), 0d, "LerpPrecise test failed on [-5,5,0.5]");
            Assert.AreEqual(MathHelperD.LerpPrecise(0d, 0d, 0.5d), 0d, "LerpPrecise test failed on [0,0,0.5]");
            Assert.AreEqual(MathHelperD.LerpPrecise(-5d, -1d, 0.5d), -3d, "LerpPrecise test failed on [-5,-1, 0.5]");
        }
#endif

        [Test]
        public void Min()
        {
            Assert.AreEqual(-0.5d, MathHelperD.Min(-0.5d, -0.5d));
            Assert.AreEqual(-0.5d, MathHelperD.Min(-0.5d,0.0d));
            Assert.AreEqual(-0.5d, MathHelperD.Min(0.0d, -0.5d));
            Assert.AreEqual(0, MathHelperD.Min(0, 0));
            Assert.AreEqual(-5, MathHelperD.Min(-5, 5));
            Assert.AreEqual(-5, MathHelperD.Min(5, -5));
        }

        [Test]
        public void Max()
        {
            Assert.AreEqual(-0.5d, MathHelperD.Min(-0.5d, -0.5d));
            Assert.AreEqual(0.0d, MathHelperD.Max(-0.5d,0.0d));
            Assert.AreEqual(0.0d, MathHelperD.Max(0.0d, -0.5d));
            Assert.AreEqual(0, MathHelperD.Max(0, 0));
            Assert.AreEqual(5, MathHelperD.Max(-5, 5));
            Assert.AreEqual(5, MathHelperD.Max(5, -5));
        }

        [TestCase(MathHelperD.PiOver4, 0.78539816339744828d)]
        [TestCase(MathHelperD.PiOver2, 1.5707963267948966d)]
        [TestCase(MathHelperD.Pi, 3.1415926535897931d)]
        [TestCase(MathHelperD.TwoPi, 6.2831853071795862)]
        public void PiConstantsAreExpectedValues(double actualValue, double expectedValue)
        {
            Assert.AreEqual(expectedValue, actualValue);
        }

        [TestCase(0d, 0d)]
        [TestCase(MathHelperD.PiOver4, MathHelperD.PiOver4)]
        [TestCase(-MathHelperD.PiOver4, -MathHelperD.PiOver4)]
        [TestCase(MathHelperD.PiOver2, MathHelperD.PiOver2)]
        [TestCase(-MathHelperD.PiOver2, -MathHelperD.PiOver2)]
        [TestCase(MathHelperD.Pi, MathHelperD.Pi)]
        [TestCase(-MathHelperD.Pi, MathHelperD.Pi)]
        [TestCase(MathHelperD.TwoPi, 0d)]
        [TestCase(-MathHelperD.TwoPi, 0d)]
        [TestCase(10d, -2.56637061435917d)]
        [TestCase(-10d, 2.5663706143591725d)]
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
        public void WrapAngleReturnsExpectedValues(double angle, double expectedValue)
        {
            var actualValue = MathHelperD.WrapAngle(angle);
            Assert.AreEqual(expectedValue, actualValue, 0.00000000000001d);
        }
    }
}
