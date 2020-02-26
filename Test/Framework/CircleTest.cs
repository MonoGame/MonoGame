using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class CircleTest
    {
#if !XNA
        [Test]
        public void CircleConstructorsAndProperties()
        {
            var circle = new Circle(10, 20, 64);

            // Constructor

            Assert.AreEqual(new Circle(){X = 10, Y = 20, Radius = 64}, circle);

            // Constructor 2

            Assert.AreEqual(new Circle() { X = 1, Y = 2, Radius = 45 }, new Circle(new Vector2(1, 2), 45));

            // X property

            Assert.AreEqual(10, circle.X);

            // Y property

            Assert.AreEqual(20, circle.Y);

            // Radius property

            Assert.AreEqual(64, circle.Radius);

            // Location property

            Assert.AreEqual(new Vector2(10, 20), circle.Location);

            // Center property

            Assert.AreEqual(new Vector2(10,20), circle.Center);

        }

        [Test]
        public void CircleContainsPoint()
        {
            Circle circle = new Circle(0,0,64);

            var p1 = new Point(-1, -1);
            var p2 = new Point(0, 0);
            var p3 = new Point(32, 32);
            var p4 = new Point(63, 63);
            var p5 = new Point(64, 64);
            var p6 = new Point(-64, -64);
            var p7 = new Point(65, 65);

            bool result;

            circle.Contains(ref p1, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p2, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p3, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p4, out result);
            Assert.AreEqual(false, result);
            circle.Contains(ref p5, out result);
            Assert.AreEqual(false, result);
            circle.Contains(ref p6, out result);
            Assert.AreEqual(false, result);
            circle.Contains(ref p7, out result);
            Assert.AreEqual(false, result);

            Assert.AreEqual(true, circle.Contains(p1));
            Assert.AreEqual(true, circle.Contains(p2));
            Assert.AreEqual(true, circle.Contains(p3));
            Assert.AreEqual(false, circle.Contains(p4));
            Assert.AreEqual(false, circle.Contains(p5));
            Assert.AreEqual(false, circle.Contains(p6));
            Assert.AreEqual(false, circle.Contains(p7));
        }

        [Test]
        public void CircleContainsVector2()
        {
            Circle circle = new Circle(0, 0, 64);
            
            var p1 = new Vector2(-1, -1);
            var p2 = new Vector2(0, 0);
            var p3 = new Vector2(32, 32);
            var p4 = new Vector2(63, 63);
            var p5 = new Vector2(64, 64);
            var p6 = new Vector2(-64, -64);
            var p7 = new Vector2(65, 65);

            bool result;

            circle.Contains(ref p1, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p2, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p3, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p4, out result);
            Assert.AreEqual(false, result);
            circle.Contains(ref p5, out result);
            Assert.AreEqual(false, result);
            circle.Contains(ref p6, out result);
            Assert.AreEqual(false, result);
            circle.Contains(ref p7, out result);
            Assert.AreEqual(false, result);

            Assert.AreEqual(true, circle.Contains(p1));
            Assert.AreEqual(true, circle.Contains(p2));
            Assert.AreEqual(true, circle.Contains(p3));
            Assert.AreEqual(false, circle.Contains(p4));
            Assert.AreEqual(false, circle.Contains(p5));
            Assert.AreEqual(false, circle.Contains(p6));
            Assert.AreEqual(false, circle.Contains(p7));
        }

        [Test]
        public void CircleContainsInts()
        {
            Circle circle = new Circle(0, 0, 64);

            int x1 = -1; int y1 = -1;
            int x2 = 0;  int y2 = 0;
            int x3 = 32; int y3 = 32;
            int x4 = 63; int y4 = 63;
            int x5 = 64; int y5 = 64;
            int x6 = -64; int y6 = -64;
            int x7 = 65; int y7 = 65;

            Assert.AreEqual(true, circle.Contains(x1,y1));
            Assert.AreEqual(true, circle.Contains(x2,y2));
            Assert.AreEqual(true, circle.Contains(x3,y3));
            Assert.AreEqual(false, circle.Contains(x4,y4));
            Assert.AreEqual(false, circle.Contains(x5,y5));
            Assert.AreEqual(false, circle.Contains(x6,y6));
            Assert.AreEqual(false, circle.Contains(x7,y7));
        }

        [Test]
        public void CircleContainsFloats()
        {
            Circle circle = new Circle(0, 0, 64);

            float x1 = -1; float y1 = -1;
            float x2 = 0;  float y2 = 0;
            float x3 = 32; float y3 = 32;
            float x4 = 63; float y4 = 63;
            float x5 = 64; float y5 = 64;
            float x6 = -64; float y6 = -64;
            float x7 = 65; float y7 = 65;

            Assert.AreEqual(true, circle.Contains(x1, y1));
            Assert.AreEqual(true, circle.Contains(x2, y2));
            Assert.AreEqual(true, circle.Contains(x3, y3));
            Assert.AreEqual(false, circle.Contains(x4, y4));
            Assert.AreEqual(false, circle.Contains(x5, y5));
            Assert.AreEqual(false, circle.Contains(x6, y6));
            Assert.AreEqual(false, circle.Contains(x7, y7));
        }

        [Test]
        public void CircleContainsCircle()
        {
            var circle = new Circle(0, 0, 64);
            var circle1 = new Circle(-1, -1, 32);
            var circle2 = new Circle(0, 0, 32);
            var circle3 = new Circle(0, 0, 64);
            var circle4 = new Circle(1, 1, 64);
            var circle5 = new Circle(10, 10, 100);

            bool result;

            circle.Contains(ref circle1, out result);
            Assert.AreEqual(true, result);

            circle.Contains(ref circle2, out result);
            Assert.AreEqual(true, result);

            circle.Contains(ref circle3, out result);
            Assert.AreEqual(false, result);

            circle.Contains(ref circle4, out result);
            Assert.AreEqual(false, result);

            circle.Contains(ref circle5, out result);
            Assert.AreEqual(false, result);

            Assert.AreEqual(true, circle.Contains(circle1));
            Assert.AreEqual(true, circle.Contains(circle2));
            Assert.AreEqual(false, circle.Contains(circle3));
            Assert.AreEqual(false, circle.Contains(circle4));
            Assert.AreEqual(false, circle1.Contains(circle));
            Assert.AreEqual(false, circle2.Contains(circle));
            Assert.AreEqual(false, circle3.Contains(circle));
            Assert.AreEqual(false, circle4.Contains(circle));
            Assert.AreEqual(true, circle5.Contains(circle));
        }

        [Test]
        public void CircleIntersects()
        {
            var circle = new Circle(0, 0, 64);
            var rectangle1 = new Rectangle(-32, -32, 128, 128);
            var rectangle2 = new Rectangle(-32, -32, 32, 32);
            var rectangle3 = new Rectangle(32, 32, 32, 32);
            var rectangle4 = new Rectangle(65, 0, 64, 64);
            var circle1 = new Circle(-30, -30, 32);
            var circle2 = new Circle(30, 30, 32);
            var circle3 = new Circle(0, 0, 32);
            var circle4 = new Circle(96, 96, 32);

            // First overload testing(forward)

            Assert.AreEqual(false, circle.Intersects(rectangle1));
            Assert.AreEqual(false, circle.Intersects(rectangle2));
            Assert.AreEqual(true, circle.Intersects(rectangle3));
            Assert.AreEqual(false, circle.Intersects(rectangle4));
            Assert.AreEqual(true, circle.Intersects(circle1));
            Assert.AreEqual(true, circle.Intersects(circle2));
            Assert.AreEqual(true, circle.Intersects(circle3));
            Assert.AreEqual(false, circle.Intersects(circle4));

            // First overload testing(forward and backward)
            Assert.AreEqual(true, circle1.Intersects(circle));
            Assert.AreEqual(true, circle2.Intersects(circle));
            Assert.AreEqual(true, circle3.Intersects(circle));
            Assert.AreEqual(false, circle4.Intersects(circle));

        }

        [Test]
        public void CircleUnion()
        {
            var first = new Circle(-64, -64, 64);
            var second = new Circle(0, 0, 64);
            var expected = new Circle(-64, -64, 128);

            // First overload testing(forward and backward)

            Circle result;
            Circle.Union(ref first, ref second, out result);

            Assert.AreEqual(expected, result);

            Circle.Union(ref second, ref first, out result);

            Assert.AreEqual(expected, result);

            // Second overload testing(forward and backward)

            Assert.AreEqual(expected, Circle.Union(first, second));
            Assert.AreEqual(expected, Circle.Union(second, first));
        }
        
        [Test]
        public void CircleToStringTest()
        {
            StringAssert.IsMatch("{X:-10 Y:10 Radius:100}",new Circle(-10,10,100).ToString());
        }
    }
#endif
}
