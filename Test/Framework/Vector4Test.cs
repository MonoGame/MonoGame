using Microsoft.Xna.Framework;
using NUnit.Framework;
using System.ComponentModel;
using System.Globalization;

namespace MonoGame.Tests.Framework
{
    class Vector4Test
    {
        [Test]
        public void TypeConverter()
        {
            var converter = TypeDescriptor.GetConverter(typeof(Vector4));
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            
            Assert.AreEqual(new Vector4(32, 64, 128, 255), converter.ConvertFromString(null, invariantCulture, "32, 64, 128, 255"));
            Assert.AreEqual(new Vector4(0.5f, 2.75f, 4.125f, 8.0625f), converter.ConvertFromString(null, invariantCulture, "0.5, 2.75, 4.125, 8.0625"));
            Assert.AreEqual(new Vector4(1024.5f, 2048.75f, 4096.125f, 8192.0625f), converter.ConvertFromString(null, invariantCulture, "1024.5, 2048.75, 4096.125, 8192.0625"));
            Assert.AreEqual("32, 64, 128, 255", converter.ConvertToString(null, invariantCulture, new Vector4(32, 64, 128, 255)));
            Assert.AreEqual("0.5, 2.75, 4.125, 8.0625", converter.ConvertToString(null, invariantCulture, new Vector4(0.5f, 2.75f, 4.125f, 8.0625f)));
            Assert.AreEqual("1024.5, 2048.75, 4096.125, 8192.0625", converter.ConvertToString(null, invariantCulture, new Vector4(1024.5f, 2048.75f, 4096.125f, 8192.0625f)));

            CultureInfo otherCulture = new CultureInfo("el-GR");

            Assert.AreEqual(new Vector4(1024.5f, 2048.75f, 4096.125f, 8192.0625f), converter.ConvertFromString(null, otherCulture, "1024,5; 2048,75; 4096,125; 8192,0625"));
            Assert.AreEqual("1024,5; 2048,75; 4096,125; 8192,0625", converter.ConvertToString(null, otherCulture, new Vector4(1024.5f, 2048.75f, 4096.125f, 8192.0625f)));
        }

        [Test]
        public void Constructors()
        {
            var expectedResult = new Vector4()
            {
                X = 1,
                Y = 2,
                Z = 3,
                W = 4
            };

            var expectedResult2 = new Vector4()
            {
                X = 2.2f,
                Y = 2.2f,
                Z = 2.2f,
                W = 2.2f
            };

            Assert.That(expectedResult, Is.EqualTo(new Vector4(1,2,3,4)).Using(Vector4Comparer.Epsilon));
            Assert.That(expectedResult, Is.EqualTo(new Vector4(new Vector2(1,2),3,4)).Using(Vector4Comparer.Epsilon));
            Assert.That(expectedResult, Is.EqualTo(new Vector4(new Vector3(1,2,3),4)).Using(Vector4Comparer.Epsilon));
            Assert.That(expectedResult2, Is.EqualTo(new Vector4(2.2f)).Using(Vector4Comparer.Epsilon));
        }

        [Test]
        public void Properties()
        {
            Assert.That(new Vector4(0,0,0,0), Is.EqualTo(Vector4.Zero).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(1,1,1,1), Is.EqualTo(Vector4.One).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(1,0,0,0), Is.EqualTo(Vector4.UnitX).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0,1,0,0), Is.EqualTo(Vector4.UnitY).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0,0,1,0), Is.EqualTo(Vector4.UnitZ).Using(Vector4Comparer.Epsilon));
            Assert.That(new Vector4(0,0,0,1), Is.EqualTo(Vector4.UnitW).Using(Vector4Comparer.Epsilon));
        }

        [Test]
        public void Dot()
        {
            var vector1 = new Vector4(1, 2, 3, 4);
            var vector2 = new Vector4(0.5f, 1.1f, -3.8f, 1.2f);
            var expectedResult = -3.89999962f;

            Assert.AreEqual(expectedResult, Vector4.Dot(vector1, vector2));

            float result;
            Vector4.Dot(ref vector1, ref vector2, out result);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public void Hermite()
        {
            var t1 = new Vector4(1.40625f, 1.40625f, 0.2f, 0.92f);
            var t2 = new Vector4(2.662375f, 2.26537514f,10.0f,2f);
            var v1 = new Vector4(1,2,3,4);
            var v2 = new Vector4(-1.3f,0.1f,30.0f,365.20f);
            var a = 2.234f;

            var result1 = Vector4.Hermite(v1, t1, v2, t2, a);
            var expected = new Vector4(39.0311f, 34.65557f, -132.5473f, -2626.85938f);
            Assert.That(expected, Is.EqualTo(result1).Using(Vector4Comparer.Epsilon));

            Vector4 result2;

            // same as result1 ? - it must be same

            Vector4.Hermite(ref v1, ref t1, ref v2, ref t2, a, out result2);
            Assert.That(result1, Is.EqualTo(result2).Using(Vector4Comparer.Epsilon));
        }

        [Test]
        public void Length()
        {
            var vector1 = new Vector4(1, 2, 3, 4);
            Assert.AreEqual(5.477226f,vector1.Length());
        }

        [Test]
        public void LengthSquared()
        {
            var vector1 = new Vector4(1, 2, 3, 4);
            Assert.AreEqual(30, vector1.LengthSquared());
        }

        [Test]
        public void Normalize()
        {
            var vector1 = new Vector4(1, 2, 3, 4);
            vector1.Normalize();
            var expected = new Vector4(0.1825742f,0.3651484f,0.5477225f,0.7302967f);
            Assert.That(expected, Is.EqualTo(vector1).Using(Vector4Comparer.Epsilon));
            var vector2 = new Vector4(1, 2, 3, 4);
            var result = Vector4.Normalize(vector2);
            Assert.That(expected, Is.EqualTo(result).Using(Vector4Comparer.Epsilon));
        }

        [Test]
        public void ToStringTest()
        {
            StringAssert.IsMatch("{X:10 Y:20 Z:3.5 W:-100}", new Vector4(10, 20, 3.5f, -100).ToString());
        }
    }
}
