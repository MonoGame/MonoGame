using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class SegmentTest
    {
        [Test]
        public void BoundingBoxIntersects()
        {
            // Our test box.
            BoundingBox box;
            box.Min = new Vector3(-10, -20, -30);
            box.Max = new Vector3(10, 20, 30);
            var center = (box.Max + box.Min) * 0.5f;

            //test intersect each face
            Assert.AreEqual(30.0f, new Segment(center - Vector3.UnitX * 40, center).Intersects(box));
            Assert.AreEqual(30.0f, new Segment(center + Vector3.UnitX * 40, center).Intersects(box));
            Assert.AreEqual(20.0f, new Segment(center - Vector3.UnitY * 40, center).Intersects(box));
            Assert.AreEqual(20.0f, new Segment(center + Vector3.UnitY * 40, center).Intersects(box));
            Assert.AreEqual(10.0f, new Segment(center - Vector3.UnitZ * 40, center).Intersects(box));
            Assert.AreEqual(10.0f, new Segment(center + Vector3.UnitZ * 40, center).Intersects(box));


            // Test the corners along each axis.
            Assert.AreEqual(10.0f, new Segment(box.Min - Vector3.UnitX * 10, box.Min + new Vector3(10,0,0)).Intersects(box));
            Assert.AreEqual(10.0f, new Segment(box.Min - Vector3.UnitY * 10, box.Min + Vector3.UnitY).Intersects(box));
            Assert.AreEqual(10.0f, new Segment(box.Min - Vector3.UnitZ * 10, box.Min + Vector3.UnitZ).Intersects(box));
            Assert.AreEqual(10.0f, new Segment(box.Max + Vector3.UnitX * 10, box.Max - Vector3.UnitX).Intersects(box));
            Assert.AreEqual(10.0f, new Segment(box.Max + Vector3.UnitY * 10, box.Max - Vector3.UnitY).Intersects(box));
            Assert.AreEqual(10.0f, new Segment(box.Max + Vector3.UnitZ * 10, box.Max - Vector3.UnitZ).Intersects(box));

            //test misses.
            Assert.IsNull(new Segment(center - Vector3.UnitX * 40, center - Vector3.UnitX * 30).Intersects(box));
            Assert.IsNull(new Segment(center - Vector3.UnitX * 30, center - Vector3.UnitX * 40).Intersects(box));
            Assert.IsNull(new Segment(center - Vector3.UnitX * 30, center - Vector3.UnitX * 30 + Vector3.One * 20).Intersects(box));
        }

        [Test]
        public void PlaneIntersects()
        {
            var p1 = new Plane(Vector3.Zero, Vector3.UnitY);

            //test parallel
            Assert.AreEqual(null, new Segment(Vector3.Up, Vector3.One).Intersects(p1));
            Assert.AreEqual(null, new Segment(Vector3.Zero, Vector3.UnitX).Intersects(p1)); //should the value be 0 when the segment is parallel?

            //test perpendicular
            Assert.AreEqual(null, new Segment(Vector3.UnitY * 2, Vector3.UnitY).Intersects(p1));
            Assert.AreEqual(1.0f, new Segment(Vector3.UnitY, -Vector3.UnitY).Intersects(p1));
            Assert.AreEqual(1.0f, new Segment(Vector3.UnitY, -Vector3.UnitY * 10).Intersects(p1));
        }

        [Test]
        public void TestIsEqual()
        {
            var a = new Segment(Vector3.Zero, Vector3.One);
            var b = new Segment(Vector3.One, Vector3.Zero);
            var c = new Segment(Vector3.Zero, Vector3.UnitX);
            var d = new Segment(Vector3.Zero, Vector3.UnitX);

            Assert.True(a.Equals(a));
            Assert.False(a.Equals(b));
            Assert.True(c.Equals(d));
            Assert.False(a.Equals(c));
        }
    }
}
