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
        public void DistanceSquared()
        {
            var p1 = new Point(1, -5);
            var p2 = new Point(-1, 3);
            var d = Point.DistanceSquared(p1, p2);
            var expectedResult = 68;
            Assert.AreEqual(expectedResult, d);
        }

        [Test]
        public void HashCode() {
            // Checking for overflows in hash calculation.
            var max = new Point(int.MaxValue, int.MaxValue);
            var min = Point.One;
            Assert.AreNotEqual(max.GetHashCode(), Point.Zero.GetHashCode());
            Assert.AreNotEqual(min.GetHashCode(), Point.Zero.GetHashCode());

            // Common values
            var a = new Point(0, 0);
            Assert.AreEqual(a.GetHashCode(), Point.Zero.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), Point.One.GetHashCode());

            // Individual properties alter hash
            var xa = new Point(2, 1);
            var xb = new Point(3, 1);
            var ya = new Point(1, 2);
            var yb = new Point(1, 3);
            Assert.AreNotEqual(xa.GetHashCode(), xb.GetHashCode(), "Different properties should change hash.");
            Assert.AreNotEqual(ya.GetHashCode(), yb.GetHashCode(), "Different properties should change hash.");
#if !XNA
            Assert.AreNotEqual(xa.GetHashCode(), ya.GetHashCode(), "Identical values on different properties should have different hashes.");
            Assert.AreNotEqual(xb.GetHashCode(), yb.GetHashCode(), "Identical values on different properties should have different hashes.");
#endif
            Assert.AreNotEqual(xa.GetHashCode(), yb.GetHashCode());
            Assert.AreNotEqual(ya.GetHashCode(), xb.GetHashCode());
        }

        [Test]
        public void Multiply()
        {
            var point1 = new Point(1, 2);

            // Test 0 scale.
            Assert.AreEqual(Point.Zero, 0 * point1);
            Assert.AreEqual(Point.Zero, point1 * 0);
            Assert.AreEqual(Point.Zero, Point.Multiply(point1, 0));
            Assert.AreEqual(Point.Multiply(point1, 0), point1 * 0);

            // Test 1 scale.
            Assert.AreEqual(point1, 1 * point1);
            Assert.AreEqual(point1, point1 * 1);
            Assert.AreEqual(point1, Point.Multiply(point1, 1));
            Assert.AreEqual(Point.Multiply(point1, 1), point1 * 1);

            var scaledPoint = point1 * 2;

            // Test 2 scale.
            Assert.AreEqual(scaledPoint, 2 * point1);
            Assert.AreEqual(scaledPoint, point1 * 2);
            Assert.AreEqual(scaledPoint, Point.Multiply(point1, 2));
            Assert.AreEqual(point1 * 2, scaledPoint);
            Assert.AreEqual(2 * point1, Point.Multiply(point1, 2));

            var point2 = new Point(2, 2);

            // Test two points multiplication.
            Assert.AreEqual(new Point(point1.X * point2.X, point1.Y * point2.Y), point1 * point2);
            Assert.AreEqual(point2 * point1, new Point(point1.X * point2.X, point1.Y * point2.Y));
            Assert.AreEqual(point1 * point2, Point.Multiply(point1, point2));
            Assert.AreEqual(Point.Multiply(point1, point2), point1 * point2);

            Point refVec;
        }
    }
}
