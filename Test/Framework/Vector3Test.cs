using Microsoft.Xna.Framework;
using NUnit.Framework;
using System.ComponentModel;
using System.Globalization;

namespace MonoGame.Tests.Framework
{
    class Vector3Test
    {
        private void Compare(Vector3 expected, Vector3 source)
        {
            Assert.That(expected, Is.EqualTo(source).Using(Vector3Comparer.Epsilon));
        }

        private void Compare(float expected, float source)
        {
            Assert.That(expected, Is.EqualTo(source).Using(FloatComparer.Epsilon));
        }

        [Test]
        public void TypeConverter()
        {
            var converter = TypeDescriptor.GetConverter(typeof(Vector3));
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;

            Assert.AreEqual(new Vector3(32, 64, 128), converter.ConvertFromString(null, invariantCulture, "32, 64, 128"));
            Assert.AreEqual(new Vector3(0.5f, 2.75f, 4.125f), converter.ConvertFromString(null, invariantCulture, "0.5, 2.75, 4.125"));
            Assert.AreEqual(new Vector3(1024.5f, 2048.75f, 4096.125f), converter.ConvertFromString(null, invariantCulture, "1024.5, 2048.75, 4096.125"));
            Assert.AreEqual("32, 64, 128", converter.ConvertToString(null, invariantCulture, new Vector3(32, 64, 128)));
            Assert.AreEqual("0.5, 2.75, 4.125", converter.ConvertToString(null, invariantCulture, new Vector3(0.5f, 2.75f, 4.125f)));
            Assert.AreEqual("1024.5, 2048.75, 4096.125", converter.ConvertToString(null, invariantCulture, new Vector3(1024.5f, 2048.75f, 4096.125f)));

            CultureInfo otherCulture = new CultureInfo("el-GR");

            Assert.AreEqual(new Vector3(1024.5f, 2048.75f, 4096.125f), converter.ConvertFromString(null, otherCulture, "1024,5; 2048,75; 4096,125"));
            Assert.AreEqual("1024,5; 2048,75; 4096,125", converter.ConvertToString(null, otherCulture, new Vector3(1024.5f, 2048.75f, 4096.125f)));
        }

        [Test]
        public void Properties()
        {
            Compare(new Vector3(0, 0, 1), Vector3.Backward);
            Compare(new Vector3(0, -1, 0), Vector3.Down);
            Compare(new Vector3(0, 0, -1), Vector3.Forward);
            Compare(new Vector3(-1, 0, 0), Vector3.Left);
            Compare(new Vector3(1, 1, 1), Vector3.One);
            Compare(new Vector3(1, 0, 0), Vector3.Right);
            Compare(new Vector3(1, 0, 0), Vector3.UnitX);
            Compare(new Vector3(0, 1, 0), Vector3.UnitY);
            Compare(new Vector3(0, 0, 1), Vector3.UnitZ);
            Compare(new Vector3(0, 1, 0), Vector3.Up);
            Compare(new Vector3(0, 0, 0), Vector3.Zero);
        }

        [Test]
        public void Operators()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(1, 2, 3);
            var v3 = new Vector3(-1, -2, -3);
            var v4 = new Vector3(5, 5, 5);

            var addResult = new Vector3(2, 4, 6);
            var zero = new Vector3(0, 0, 0);
            var negResult = new Vector3(-1, -2, -3);
            var mulResult = new Vector3(1, 4, 9);
            var mulResult2 = new Vector3(2, 4, 6);
            var divResult = new Vector3(0.2f, 0.4f, 0.6f);

            Assert.IsTrue(v1 == v2); // comparsion 1
            Assert.IsTrue(v1 != v3); // comparsion 2

            Compare(addResult, v1 + v2); // add
            Compare(zero, v1 - v2); // sub
            Compare(negResult, -v1); // negation
            Compare(mulResult, v1 * v2); // mul
            Compare(mulResult2, v1 * 2); // mul scalar
            Compare(mulResult2, 2 * v1); // scalar mul
            Compare(divResult, v1 / v4); // div
            Compare(divResult, v1 / 5.0f); // div scalar
        }

        [Test]
        public void Cross()
        {
            var v1 = new Vector3(-1, 2, 3);
            var v2 = new Vector3(4, -10, -11);
            var v3 = new Vector3(22.55f, 1.5f, -3.14f);
            var v4 = new Vector3(-2.1f, 1.5f, -0.01f);

            var expected1 = new Vector3(8, 1, 2);
            var expected2 = new Vector3(4.695f, 6.8195f, 36.975f);

            Vector3 result1;
            Vector3 result2;

            Compare(expected1, Vector3.Cross(v1, v2));
            Compare(expected2, Vector3.Cross(v3, v4));

            Vector3.Cross(ref v1, ref v2, out result1);
            Vector3.Cross(ref v3, ref v4, out result2);

            Compare(expected1, result1);
            Compare(expected2, result2);
        }

        [Test]
        public void Distance()
        {
            Vector3 v1 = new Vector3(10.5f, 20, -3.4f);
            Vector3 v2 = new Vector3(-0.01f, -10, -20);

            var expected = 35.8611221f;

            float result;

            Compare(expected, Vector3.Distance(v1, v2));

            Vector3.Distance(ref v1, ref v2, out result);

            Compare(expected, result);
        }

        [Test]
        public void DistanceSquared()
        {
            var v1 = new Vector3(0.1f, 100.0f, -5.5f);
            var v2 = new Vector3(1.1f, -2.0f, 5.5f);
            var d = Vector3.DistanceSquared(v1, v2);
            var expectedResult = 10526f;
            Assert.AreEqual(expectedResult, d);
        }

        [Test]
        public void Equals()
        {
            object vo = new Vector3(1, 2, 3);
            Vector3 v = new Vector3(1, 2, 3);
            Assert.IsTrue(new Vector3(1, 2, 3).Equals(vo));
            Assert.IsTrue(new Vector3(1, 2, 3).Equals(v));
        }

        [Test]
        public void Negate()
        {
            var expected1 = new Vector3(-1,-2, -3);
            var expected2 = new Vector3(1, 2, 3);

            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(-1, -2, -3);

            Vector3 result1;
            Vector3 result2;

            Compare(expected1, Vector3.Negate(v1));
            Compare(expected2, Vector3.Negate(v2));

            Vector3.Negate(ref v1, out result1);
            Vector3.Negate(ref v2, out result2);

            Compare(expected1, result1);
            Compare(expected2, result2);
        }

        [Test]
        public void Normalize()
        {
            var v1 = new Vector3(-10.5f, 0.2f, 1000.0f);
            var v2 = new Vector3(-10.5f, 0.2f, 1000.0f);

            Vector3 result;

            var expectedResult = new Vector3(-0.0104994215f, 0.000199988979f, 0.999944866f);

            v1.Normalize();

            Compare(expectedResult, v1);
            
            Compare(expectedResult, Vector3.Normalize(v2));

            Vector3.Normalize(ref v2, out result);

            Compare(expectedResult, result);
        }

        [Test]
        public void Reflect()
        {
            var expected = new Vector3(-15.5479984f, -20.6239986f, 15.448f);

            var v1 = new Vector3(5.1f, 0.024f, -5.2f);
            var v2 = new Vector3(1f, 1f, -1f);

            Vector3 result;

            Compare(expected, Vector3.Reflect(v1, v2));

            Vector3.Reflect(ref v1, ref v2, out result);

            Compare(expected, result);
        }

        [Test]
        public void ToStringTest()
        {
            StringAssert.IsMatch("{X:0 Y:1 Z:2.2}", new Vector3(0, 1, 2.2f).ToString());
        }

        [Test]
        public void Transform()
        {
            // STANDART OVERLOADS TEST

            var expectedResult1 = new Vector3(51, 58, 65);
            var expectedResult2 = new Vector3(33, -14, -1);

            var v1 = new Vector3(1, 2, 3);
            var m1 = new Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);

            var v2 = new Vector3(1, 2, 3);
            var q1 = new Quaternion(2, 3, 4, 5);

            Vector3 result1;
            Vector3 result2;

            Compare(expectedResult1, Vector3.Transform(v1, m1));
            Compare(expectedResult2, Vector3.Transform(v2, q1));

            // OUTPUT OVERLOADS TEST

            Vector3.Transform(ref v1, ref m1, out result1);
            Vector3.Transform(ref v2, ref q1, out result2);

            Compare(expectedResult1, result1);
            Compare(expectedResult2, result2);
        }

        [Test]
        public void TransformNormal()
        {
            var v1 = new Vector3(0.5f, 1.0f, 0.9f);
            var m1 = new Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
            
            var expected = new Vector3(13.5999994f, 16, 18.4f);

            Vector3 result;

            Compare(expected, Vector3.TransformNormal(v1, m1));

            Vector3.TransformNormal(ref v1, ref m1, out result);

            Compare(expected, result);
        }
    }
}
