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
        public void DistanceLerp()
        {
            Assert.AreEqual(MathHelper.Lerp(0f, 5f, .5f), 2.5f, "Lerp test failed on [0,5,.5]");
            Assert.AreEqual(MathHelper.Lerp(-5f, 5f, 0.5f), 0f, "Distance test failed on [-5,5,0.5]");
            Assert.AreEqual(MathHelper.Lerp(0f, 0f, 0.5f), 0f, "Distance test failed on [0,0,0.5]");
            Assert.AreEqual(MathHelper.Lerp(-5f, -1f, 0.5f), -3f, "Distance test failed on [-5,-1, 0.5]");
        }
    }
}
