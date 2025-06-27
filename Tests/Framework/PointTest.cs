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
        public void ToDrawing()
        {
            Point point = new Point(1, 2);

            var expected = new System.Drawing.Point(point.X, point.Y);
            var actual = point.ToDrawing();

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ImplicitOperator()
        {
            var drawingPoint = new System.Drawing.Point(1, 2);
            var xnaPoint = new Point(1, 2);

            Assert.AreEqual(drawingPoint.X, xnaPoint.X);
            Assert.AreEqual(drawingPoint.Y, xnaPoint.Y);
        }
    }
}
