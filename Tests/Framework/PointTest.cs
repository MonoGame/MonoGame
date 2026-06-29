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

        [Test]
        public void Multiply()
        {
            Point point = new Point(1, 2);

            // Test 0 scale.
            Assert.AreEqual(Point.Zero, point * 0);
            Assert.AreEqual(Point.Zero, 0 * point);

            // Test 1 scale.
            Assert.AreEqual(point, point * 1);
            Assert.AreEqual(point, 1 * point);

            // Test 2 scale.
            Point scaledPoint = point * 2;
            Assert.AreEqual(scaledPoint, point * 2);
            Assert.AreEqual(scaledPoint, 2 * point);

            Point pointTwo = new Point(2, 2);

            // Test two-point multiplication.
            Assert.AreEqual(new Point(point.X * pointTwo.X, point.Y * pointTwo.Y), point * pointTwo);
            Assert.AreEqual(new Point(point.X * pointTwo.X, point.Y * pointTwo.Y), pointTwo * point);
        }

        [Test]
        public void Divide()
        {
            Point point = new Point(2, 4);

            // Test 1 divisor.
            Assert.AreEqual(new Point(point.X / 1, point.Y / 1), point / 1);
            Assert.AreEqual(point, point / 1);

            // Test 2 divisor.
            Assert.AreEqual(new Point(point.X / 2, point.Y / 2), point / 2);
            Assert.AreEqual(new Point(1, 2), point / 2);

            Point pointTwo = new Point(1, 2);

            // Test two-point division.
            Assert.AreEqual(new Point(point.X / pointTwo.X), point / pointTwo);
            Assert.AreEqual(new Point(pointTwo.X / point.X), pointTwo / point);
        }
    }
}
