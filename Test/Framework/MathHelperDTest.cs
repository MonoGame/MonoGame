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

        [TestCase(MathHelperD.PiOver4, 0.7853982d)]
        [TestCase(MathHelperD.PiOver2, 1.5707964d)]
        [TestCase(MathHelperD.Pi, 3.1415927d)]
        [TestCase(MathHelperD.TwoPi, 6.2831855d)]
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
        [TestCase(10d, -2.566371d)]
        [TestCase(-10d, 2.566371d)]
        // Pi boundaries
        [TestCase(3.1415927d, 3.1415927d)]
        [TestCase(3.141593d, -3.1415925d)]
        [TestCase(-3.1415925d, -3.1415925d)]
        [TestCase(-3.1415927d, 3.1415927d)]
        // 2 * Pi boundaries
        [TestCase(6.283185d, -4.7683716E-7d)]
        [TestCase(6.2831855d, 0d)]
        [TestCase(6.283186d, 4.7683716E-7d)]
        [TestCase(-6.283185d, 4.7683716E-7d)]
        [TestCase(-6.2831855d, 0d)]
        [TestCase(-6.283186d, -4.7683716E-7d)]
        // 3 * Pi boundaries
        [TestCase(9.424778d, 3.1415925d)]
        [TestCase(9.424779d, -3.141592d)]
        [TestCase(-9.424778d, -3.1415925d)]
        [TestCase(-9.424779d, 3.141592d)]
        // 4 * Pi boundaries
        [TestCase(12.56637d, -9.536743E-7d)]
        [TestCase(12.566371d, 0d)]
        [TestCase(12.566372d, 9.536743E-7d)]
        [TestCase(-12.56637d, 9.536743E-7d)]
        [TestCase(-12.566371d, 0d)]
        [TestCase(-12.566372d, -9.536743E-7d)]
        // 5 * Pi boundaries
        [TestCase(15.707963d, 3.141592d)]
        [TestCase(15.707964d, -3.1415925d)]
        [TestCase(-15.707963d, -3.141592d)]
        [TestCase(-15.707964d, 3.1415925d)]
        // 10 * Pi boundaries
        [TestCase(31.415926d, -1.4305115E-6d)]
        [TestCase(31.415928d, 4.7683716E-7d)]
        [TestCase(-31.415926d, 1.4305115E-6d)]
        [TestCase(-31.415928d, -4.7683716E-7d)]
        // 20 * Pi boundaries
        [TestCase(62.831852d, -2.861023E-6d)]
        [TestCase(62.831856d, 9.536743E-7d)]
        [TestCase(-62.831852d, 2.861023E-6d)]
        [TestCase(-62.831856d, -9.536743E-7d)]
        // 20000000 * Pi boundaries
        [TestCase(6.2831852E7d, -2.8202515d)]
        [TestCase(6.2831856E7d, 1.1797485d)]
        [TestCase(-6.2831852E7d, 2.8202515d)]
        [TestCase(-6.2831856E7d, -1.1797485d)]
        public void WrapAngleReturnsExpectedValues(double angle, double expectedValue)
        {
            var actualValue = MathHelperD.WrapAngle(angle);
            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}
