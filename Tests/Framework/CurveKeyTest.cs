using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class CurveKeyTest
    {
        [Test]
        public void Properties()
        {
            var key = new CurveKey(1, 2, 3, 4, CurveContinuity.Step);

            Assert.AreEqual(1, key.Position);
            Assert.AreEqual(2, key.Value);
            Assert.AreEqual(3, key.TangentIn);
            Assert.AreEqual(4, key.TangentOut);
            Assert.AreEqual(CurveContinuity.Step, key.Continuity);
        }

        [Test]
        public void Clone()
        {
            var key = new CurveKey(1, 2, 3, 4, CurveContinuity.Step);

            Assert.AreEqual(key,key.Clone());
        }
    }
}
