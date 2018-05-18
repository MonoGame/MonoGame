using Microsoft.Xna.Framework;
using NUnit.Framework;
using System.ComponentModel;
using System.Globalization;

namespace MonoGame.Tests.Framework
{
    class Point3Test
    {
#if !XNA
        [Test]
        public void Deconstruct()
        {
            Point3 p = new Point3(int.MinValue, int.MaxValue, int.MinValue);

            int x, y, z;

            p.Deconstruct(out x, out y, out z);

            Assert.AreEqual(x, p.X);
            Assert.AreEqual(y, p.Y);
            Assert.AreEqual(z, p.Z);
        }
#endif

        [Test]
        public void DistanceSquared()
        {
            var p1 = new Point3(1, 13, -5);
            var p2 = new Point3(-1, -2, 3);
            var d = Point3.DistanceSquared(p1, p2);
            var expectedResult = 293;
            Assert.AreEqual(expectedResult, d);
        }

        [Test]
        public void HashCode() {
            // Checking for overflows in hash calculation.
            var max = new Point3(int.MaxValue, int.MaxValue, int.MaxValue);
            var min = new Point3(int.MinValue, int.MinValue, int.MinValue);
            Assert.AreNotEqual(max.GetHashCode(), Point3.Zero.GetHashCode());
            Assert.AreNotEqual(min.GetHashCode(), Point3.Zero.GetHashCode());

            // Common values
            var a = new Point3(0, 0, 0);
            Assert.AreEqual(a.GetHashCode(), Point3.Zero.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), Point3.One.GetHashCode());

            // Individual properties alter hash
            var xa = new Point3(2, 1, 1);
            var xb = new Point3(3, 1, 1);
            var ya = new Point3(1, 2, 1);
            var yb = new Point3(1, 3, 1);
            var za = new Point3(1, 1, 2);
            var zb = new Point3(1, 1, 3);
            Assert.AreNotEqual(xa.GetHashCode(), xb.GetHashCode(), "Different properties should change hash.");
            Assert.AreNotEqual(ya.GetHashCode(), yb.GetHashCode(), "Different properties should change hash.");
            Assert.AreNotEqual(za.GetHashCode(), zb.GetHashCode(), "Different properties should change hash.");
#if !XNA
            Assert.AreNotEqual(xa.GetHashCode(), ya.GetHashCode(), "Identical values on different properties should have different hashes.");
            Assert.AreNotEqual(xb.GetHashCode(), yb.GetHashCode(), "Identical values on different properties should have different hashes.");
            Assert.AreNotEqual(xb.GetHashCode(), zb.GetHashCode(), "Identical values on different properties should have different hashes.");
            Assert.AreNotEqual(yb.GetHashCode(), zb.GetHashCode(), "Identical values on different properties should have different hashes.");
#endif
            Assert.AreNotEqual(xa.GetHashCode(), yb.GetHashCode());
            Assert.AreNotEqual(ya.GetHashCode(), xb.GetHashCode());
            Assert.AreNotEqual(xa.GetHashCode(), zb.GetHashCode());
        }

        [Test]
        public void Multiply()
        {
            var point1 = new Point3(1, 2, 3);

            // Test 0 scale.
            Assert.AreEqual(Point3.Zero, 0 * point1);
            Assert.AreEqual(Point3.Zero, point1 * 0);
            Assert.AreEqual(Point3.Zero, Point3.Multiply(point1, 0));
            Assert.AreEqual(Point3.Multiply(point1, 0), point1 * 0);

            // Test 1 scale.
            Assert.AreEqual(point1, 1 * point1);
            Assert.AreEqual(point1, point1 * 1);
            Assert.AreEqual(point1, Point3.Multiply(point1, 1));
            Assert.AreEqual(Point3.Multiply(point1, 1), point1 * 1);

            var scaledPoint3 = point1 * 2;

            // Test 2 scale.
            Assert.AreEqual(scaledPoint3, 2 * point1);
            Assert.AreEqual(scaledPoint3, point1 * 2);
            Assert.AreEqual(scaledPoint3, Point3.Multiply(point1, 2));
            Assert.AreEqual(point1 * 2, scaledPoint3);
            Assert.AreEqual(2 * point1, Point3.Multiply(point1, 2));

            var point2 = new Point3(2, 2, 2);

            // Test two points multiplication.
            Assert.AreEqual(new Point3(point1.X * point2.X, point1.Y * point2.Y, point1.Z * point2.Z), point1 * point2);
            Assert.AreEqual(point2 * point1, new Point3(point1.X * point2.X, point1.Y * point2.Y, point1.Z * point2.Z));
            Assert.AreEqual(point1 * point2, Point3.Multiply(point1, point2));
            Assert.AreEqual(Point3.Multiply(point1, point2), point1 * point2);

            Point3 refVec;
        }
    }
}
