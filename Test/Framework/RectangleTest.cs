using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class RectangleTest
    {
        [Test]
        public void ConstructorsAndProperties()
        {
            var rectangle = new Rectangle(10, 20, 64, 64);

            // Constructor

            Assert.AreEqual(new Rectangle(){X = 10, Y = 20, Width = 64, Height = 64}, rectangle);
#if !XNA
            // Constructor 2

            Assert.AreEqual(new Rectangle() { X = 1, Y = 2, Width = 4, Height = 45 }, new Rectangle(new Point(1, 2), new Point(4, 45)));
#endif
            // Left property

            Assert.AreEqual(10, rectangle.Left);

            // Right property

            Assert.AreEqual(64 + 10, rectangle.Right);

            // Top property

            Assert.AreEqual(20, rectangle.Top);

            // Bottom property

            Assert.AreEqual(64 + 20, rectangle.Bottom);

            // Location property

            Assert.AreEqual(new Point(10, 20), rectangle.Location);

            // Center property

            Assert.AreEqual(new Point(10+32,20+32), rectangle.Center);

#if !XNA
            // Size property

            Assert.AreEqual(new Point(64,64), rectangle.Size);
#endif

            // IsEmpty property

            Assert.AreEqual(false, rectangle.IsEmpty);
            Assert.AreEqual(true, new Rectangle().IsEmpty);

            // Empty - static property 

            Assert.AreEqual(new Rectangle(),Rectangle.Empty);
        }

        [Test]
        public void ContainsPoint()
        {
            Rectangle rectangle = new Rectangle(0,0,64,64);

            var p1 = new Point(-1, -1);
            var p2 = new Point(0, 0);
            var p3 = new Point(32, 32);
            var p4 = new Point(63, 63);
            var p5 = new Point(64, 64);

            bool result;

            rectangle.Contains(ref p1, out result);
            Assert.AreEqual(false, result);
            rectangle.Contains(ref p2, out result);
            Assert.AreEqual(true, result);
            rectangle.Contains(ref p3, out result);
            Assert.AreEqual(true, result);
            rectangle.Contains(ref p4, out result);
            Assert.AreEqual(true, result);
            rectangle.Contains(ref p5, out result);
            Assert.AreEqual(false, result);

            Assert.AreEqual(false, rectangle.Contains(p1));
            Assert.AreEqual(true, rectangle.Contains(p2));
            Assert.AreEqual(true, rectangle.Contains(p3));
            Assert.AreEqual(true, rectangle.Contains(p4));
            Assert.AreEqual(false, rectangle.Contains(p5));
        }
#if !XNA
        [Test]
        public void ContainsVector2()
        {
            Rectangle rectangle = new Rectangle(0, 0, 64, 64);
            
            var p1 = new Vector2(-1, -1);
            var p2 = new Vector2(0, 0);
            var p3 = new Vector2(32, 32);
            var p4 = new Vector2(63, 63);
            var p5 = new Vector2(64, 64);

            bool result;

            rectangle.Contains(ref p1, out result);
            Assert.AreEqual(false, result);
            rectangle.Contains(ref p2, out result);
            Assert.AreEqual(true, result);
            rectangle.Contains(ref p3, out result);
            Assert.AreEqual(true, result);
            rectangle.Contains(ref p4, out result);
            Assert.AreEqual(true, result);
            rectangle.Contains(ref p5, out result);
            Assert.AreEqual(false, result);

            Assert.AreEqual(false, rectangle.Contains(p1));
            Assert.AreEqual(true, rectangle.Contains(p2));
            Assert.AreEqual(true, rectangle.Contains(p3));
            Assert.AreEqual(true, rectangle.Contains(p4));
            Assert.AreEqual(false, rectangle.Contains(p5));
        }

        [Test]
        public void ContainsInts()
        {
            Rectangle rectangle = new Rectangle(0, 0, 64, 64);

            int x1 = -1; int y1 = -1;
            int x2 = 0;  int y2 = 0;
            int x3 = 32; int y3 = 32;
            int x4 = 63; int y4 = 63;
            int x5 = 64; int y5 = 64;

            Assert.AreEqual(false, rectangle.Contains(x1,y1));
            Assert.AreEqual(true, rectangle.Contains(x2,y2));
            Assert.AreEqual(true, rectangle.Contains(x3,y3));
            Assert.AreEqual(true, rectangle.Contains(x4,y4));
            Assert.AreEqual(false, rectangle.Contains(x5,y5));
        }

        [Test]
        public void ContainsFloats()
        {
            Rectangle rectangle = new Rectangle(0, 0, 64, 64);

            float x1 = -1; float y1 = -1;
            float x2 = 0;  float y2 = 0;
            float x3 = 32; float y3 = 32;
            float x4 = 63; float y4 = 63;
            float x5 = 64; float y5 = 64;

            Assert.AreEqual(false, rectangle.Contains(x1, y1));
            Assert.AreEqual(true, rectangle.Contains(x2, y2));
            Assert.AreEqual(true, rectangle.Contains(x3, y3));
            Assert.AreEqual(true, rectangle.Contains(x4, y4));
            Assert.AreEqual(false, rectangle.Contains(x5, y5));
        }
#endif
        [Test]
        public void ContainsRectangle()
        {
            var rectangle = new Rectangle(0, 0, 64, 64);
            var rect1 = new Rectangle(-1, -1, 32, 32);
            var rect2 = new Rectangle(0, 0, 32, 32);
            var rect3 = new Rectangle(0, 0, 64, 64);
            var rect4 = new Rectangle(1, 1, 64, 64);

            bool result;

            rectangle.Contains(ref rect1, out result);

            Assert.AreEqual(false, result);

            rectangle.Contains(ref rect2, out result);

            Assert.AreEqual(true, result);

            rectangle.Contains(ref rect3, out result);

            Assert.AreEqual(true, result);

            rectangle.Contains(ref rect4, out result);

            Assert.AreEqual(false, result);

            Assert.AreEqual(false, rectangle.Contains(rect1));
            Assert.AreEqual(true, rectangle.Contains(rect2));
            Assert.AreEqual(true, rectangle.Contains(rect3));
            Assert.AreEqual(false, rectangle.Contains(rect4));
        }

        [Test]
        public void Inflate()
        {
            Rectangle rectangle = new Rectangle(0,0,64,64);
            rectangle.Inflate(10,-10);
            Assert.AreEqual(new Rectangle(-10, 10, 84, 44),rectangle);
#if !XNA
            Rectangle rectangleF = new Rectangle(0, 0, 64, 64);
            rectangleF.Inflate(10.0f, -10.0f);
            Assert.AreEqual(new Rectangle(-10, 10, 84, 44), rectangleF);
#endif
        }

        [Test]
        public void Intersect()
        {
            var first = new Rectangle(0, 0, 64, 64);
            var second = new Rectangle(-32, -32, 64, 64);
            var expected = new Rectangle(0, 0, 32, 32);

            // First overload testing(forward and backward)

            Rectangle result;
            Rectangle.Intersect(ref first, ref second, out result);

            Assert.AreEqual(expected, result);

            Rectangle.Intersect(ref second, ref first, out result);

            Assert.AreEqual(expected, result);

            // Second overload testing(forward and backward)

            Assert.AreEqual(expected, Rectangle.Intersect(first, second));
            Assert.AreEqual(expected, Rectangle.Intersect(second, first));           
        }

        [Test]
        public void Union()
        {
            var first = new Rectangle(-64, -64, 64, 64);
            var second = new Rectangle(0, 0, 64, 64);
            var expected = new Rectangle(-64, -64, 128, 128);

            // First overload testing(forward and backward)

            Rectangle result;
            Rectangle.Union(ref first, ref second, out result);

            Assert.AreEqual(expected, result);

            Rectangle.Union(ref second, ref first, out result);

            Assert.AreEqual(expected, result);

            // Second overload testing(forward and backward)

            Assert.AreEqual(expected, Rectangle.Union(first, second));
            Assert.AreEqual(expected, Rectangle.Union(second, first));
        }
        
        [Test]
        public void ToStringTest()
        {
            StringAssert.IsMatch("{X:-10 Y:10 Width:100 Height:1000}",new Rectangle(-10,10,100,1000).ToString());
        }
    }
}
