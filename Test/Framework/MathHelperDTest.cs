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

        [TestCase(MathHelperD.PiOver4, 0.785398163397448d)]
        [TestCase(MathHelperD.PiOver2, 1.5707963267949)]
        [TestCase(MathHelperD.Pi, 3.14159265358979d)]
        [TestCase(MathHelperD.TwoPi, 6.28318530717959d)]
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
        [TestCase(-10d, 2.56637061435917d)]
        // Pi boundaries
        [TestCase(3.14159265358979d, 3.14159265358979d)]
        [TestCase(3.14159265358980d, -3.14159265358979d)]
        [TestCase(-3.14159265358977d, -3.14159265358977d)]
        [TestCase(-3.14159265358979d, -3.14159265358979d)]
        // 2 * Pi boundaries
        [TestCase(6.2831853071795855d, -8.88178419700125E-16)]
        [TestCase(6.2831853071795862d, 0)]
        [TestCase(6.28318530717959d, 3.5527136788005E-15)]
        [TestCase(-6.2831853071795855d, 8.88178419700125E-16)]
        [TestCase(-6.2831853071795862d, 0)]
        [TestCase(-6.28318530717959d, -3.5527136788005E-15)]
        // 3 * Pi boundaries
        [TestCase(9.4247779607693793d, 3.14159265358979d)]
        [TestCase(9.42477796077d, -3.14159265358917d)]
        [TestCase(-9.4247779607693793d, 3.14159265358979)]
        [TestCase(-9.42477796077d, 3.14159265358917d)]
        // 4 * Pi boundaries
        [TestCase(12.56637061435917d, -1.77635683940025E-15)]
        [TestCase(12.5663706143591724d, 0d)]
        [TestCase(12.5663706143591748d, 1.77635683940025E-15)]
        [TestCase(-12.56637061435917d, 1.77635683940025E-15)]
        [TestCase(-12.5663706143591724d, 0d)]
        [TestCase(-12.5663706143591748d, -1.77635683940025E-15)]
        // 5 * Pi boundaries
        [TestCase(15.7079632679489655d, 3.141592d)]
        [TestCase(15.707964d, -3.14159192153876d)]
        [TestCase(-15.7079632679489655d, -3.141592d)]
        [TestCase(-15.707964d, 3.14159192153876d)]
        // 10 * Pi boundaries
        [TestCase(31.415926535897931d, 0d)]
        [TestCase(31.4159265358980d, 6.75015598972095E-14)]
        [TestCase(-31.415926535897931d, 0d)]
        [TestCase(-31.4159265358980d, -6.75015598972095E-14)]
        // 20 * Pi boundaries
        [TestCase(62.831853071795862d, 0d)]
        [TestCase(62.83185308d, 8.20413958990684E-09)]
        [TestCase(-62.831853071795862d, 0d)]
        [TestCase(-62.83185308d, -8.20413958990684E-09)]
        // 20000000 * Pi boundaries
        [TestCase(62831853.071795862d, -3.87717591365799E-09)]
        [TestCase(62831854.071795862d, 0.999999996122824d)]
        [TestCase(-62831853.071795862d, 3.87717591365799E-09)]
        [TestCase(-62831854.071795862d, -0.999999996122824d)]
        public void WrapAngleReturnsExpectedValues(double angle, double expectedValue)
        {
            var actualValue = MathHelperD.WrapAngle(angle);
            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}
