using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class CircleTest
    {
        [Test]
        public void ConstructorsAndProperties()
        {
#if !XNA
            var circle = new Circle(new Vector2(200.0f, 300.0f), 100.0f);

            // Constructor

            Assert.AreEqual(new Circle() { Center = new Vector2(200.0f, 300.0f), Radius = 100.0f }, circle);

            // Left property

            Assert.AreEqual(200.0f - 100.0f, circle.Left);

            // Right property

            Assert.AreEqual(200.0f + 100.0f, circle.Right);

            // Top property

            Assert.AreEqual(300.0f - 100.0f, circle.Top);

            // Bottom property

            Assert.AreEqual(300.0f + 100.0f, circle.Bottom);

            // Location property

            Assert.AreEqual(new Point(200, 300), circle.Location);

            // Center property

            Assert.AreEqual(new Vector2(200.0f, 300.0f), circle.Center);

            // Radius property

            Assert.AreEqual(100.0f, circle.Radius);

            // IsEmpty property

            Assert.AreEqual(false, circle.IsEmpty);
            Assert.AreEqual(true, new Circle().IsEmpty);

            // Empty - static property 

            Assert.AreEqual(new Circle(),Circle.Empty);
#endif
        }

#if !XNA


        [Test]
        public void ContainsPoint()
        {
            var circle = new Circle(new Vector2(200.0f, 300.0f), 100.0f);

            var p1 = new Point(-1, -1);
            var p2 = new Point(110, 300);
            var p3 = new Point(200, 300);
            var p4 = new Point(290, 300);
            var p5 = new Point(400, 400);

            bool result;

            circle.Contains(ref p1, out result);
            Assert.AreEqual(false, result);
            circle.Contains(ref p2, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p3, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p4, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p5, out result);
            Assert.AreEqual(false, result);

            Assert.AreEqual(false, circle.Contains(p1));
            Assert.AreEqual(true, circle.Contains(p2));
            Assert.AreEqual(true, circle.Contains(p3));
            Assert.AreEqual(true, circle.Contains(p4));
            Assert.AreEqual(false, circle.Contains(p5));
        }

        [Test]
        public void ContainsVector2()
        {
             var circle = new Circle(new Vector2(200.0f, 300.0f), 100.0f);

            var p1 = new Vector2(-1, -1);
            var p2 = new Vector2(110, 300);
            var p3 = new Vector2(200, 300);
            var p4 = new Vector2(290, 300);
            var p5 = new Vector2(400, 400);

            bool result;

            circle.Contains(ref p1, out result);
            Assert.AreEqual(false, result);
            circle.Contains(ref p2, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p3, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p4, out result);
            Assert.AreEqual(true, result);
            circle.Contains(ref p5, out result);
            Assert.AreEqual(false, result);

            Assert.AreEqual(false, circle.Contains(p1));
            Assert.AreEqual(true, circle.Contains(p2));
            Assert.AreEqual(true, circle.Contains(p3));
            Assert.AreEqual(true, circle.Contains(p4));
            Assert.AreEqual(false, circle.Contains(p5));
        }

        [Test]
        public void ContainsFloats()
        {
            var circle = new Circle(new Vector2(200.0f, 300.0f), 100.0f);

            float x1 = -1;   float y1 = -1;
            float x2 = 110; float y2 = 300;
            float x3 = 200; float y3 = 300;
            float x4 = 290; float y4 = 300;
            float x5 = 400; float y5 = 400;

            Assert.AreEqual(false, circle.Contains(x1, y1));
            Assert.AreEqual(true, circle.Contains(x2, y2));
            Assert.AreEqual(true, circle.Contains(x3, y3));
            Assert.AreEqual(true, circle.Contains(x4, y4));
            Assert.AreEqual(false, circle.Contains(x5, y5));
        }

        [Test]
        public void ContainsCircle()
        {
            var circle = new Circle(new Vector2(200.0f, 300.0f), 100.0f);

            var circ1 = new Circle(new Vector2(199.0f, 299.0f), 100.0f);
            var circ2 = new Circle(new Vector2(200.0f, 300.0f), 25.0f);
            var circ3 = new Circle(new Vector2(200.0f, 300.0f), 100.0f);
            var circ4 = new Circle(new Vector2(201.0f, 301.0f), 100.0f);

            bool result;

            circle.Contains(ref circ1, out result);

            Assert.AreEqual(false, result);

            circle.Contains(ref circ2, out result);

            Assert.AreEqual(true, result);

            circle.Contains(ref circ3, out result);

            Assert.AreEqual(true, result);

            circle.Contains(ref circ4, out result);

            Assert.AreEqual(false, result);

            Assert.AreEqual(false, circle.Contains(circ1));
            Assert.AreEqual(true, circle.Contains(circ2));
            Assert.AreEqual(true, circle.Contains(circ3));
            Assert.AreEqual(false, circle.Contains(circ4));
        }

        [Test]
        public void IntersectionTest()
        {
            var circle = new Circle(new Vector2(200.0f, 300.0f), 100.0f);

            var circ1 = new Circle(new Vector2(350.0f, 300.0f), 100.0f);
            var circ2 = new Circle(new Vector2(400.0f, 300.0f), 100.0f);

            var rect1 = new Rectangle(250, 300, 100, 100);
            var rect2 = new Rectangle(400, 300, 100, 100);

            bool result;

            circle.Intersects(ref circ1, out result);
            Assert.AreEqual(true, result);
            circle.Intersects(ref circ2, out result);
            Assert.AreEqual(false, result);

            circle.Intersects(ref rect1, out result);
            Assert.AreEqual(true, result);
            circle.Intersects(ref rect2, out result);
            Assert.AreEqual(false, result);

            Assert.AreEqual(true, circle.Intersects(circ1));
            Assert.AreEqual(false, circle.Intersects(circ2));

            Assert.AreEqual(true, circle.Intersects(rect1));
            Assert.AreEqual(false, circle.Intersects(rect2));  
        }

        [Test]
        public void Inflate()
        {
            var circle = new Circle(new Vector2(200.0f, 300.0f), 100.0f);
            circle.Inflate(100.0f);
            Assert.AreEqual(new Circle(new Vector2(100.0f, 200.0f), 300.0f), circle);
        }
        
        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual("{Center:{{X:200 Y:300}} Radius:100}", new Circle(new Vector2(200.0f, 300.0f), 100.0f).ToString());
        }

        [Test]
        public void ToRectangleTest()
        {
            Assert.AreEqual(new Rectangle(150, 250, 100, 100), new Circle(new Vector2(200.0f, 300.0f), 100.0f).ToRectangle());
        }

#endif
    }
}
