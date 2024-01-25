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
    }
}
