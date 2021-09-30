using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    public class PointTest
    {
        [Test]
        public void Deconstruct()
        {
            Point point = new Point(int.MinValue, int.MaxValue);

            int x, y;

            point.Deconstruct(out x, out y);

            Assert.AreEqual(x, point.X);
            Assert.AreEqual(y, point.Y);
        }

        /// <summary>
        /// Test ToString Method in point
        /// </summary>
        [Test]
        public void TestToString() {
            Point point = new Point(5,10);
            Point point2 = new Point(100);
            Point point3 = new Point();
            Point point4 = new Point(3, 5);
            int x, y;
            StringAssert.IsMatch("X:5 Y:10", point.ToString());
            StringAssert.IsMatch("X:100 Y:100", point2.ToString());
            StringAssert.IsMatch("X:0 Y:0", point3.ToString());

            point.Deconstruct(out x, out y);
            StringAssert.IsMatch("X:3 Y:5", point4.ToString());
        }
    }
}
