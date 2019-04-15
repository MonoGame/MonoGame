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

        [Test]
        public void HashCode() {
            // Checking for overflows in hash calculation.
            var max = new Vector4(float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue);
            var min = new Vector4(float.MinValue, float.MinValue, float.MinValue, float.MinValue);
            Assert.AreNotEqual(max.GetHashCode(), Vector4.Zero.GetHashCode());
            Assert.AreNotEqual(min.GetHashCode(), Vector4.Zero.GetHashCode());

            // Common values
            var a = new Vector4(0f, 0f, 0f, 0f);
            Assert.AreEqual(a.GetHashCode(), Vector4.Zero.GetHashCode(), "Shouldn't do object id compare.");
            Assert.AreNotEqual(a.GetHashCode(), Vector4.One.GetHashCode());

            // Individual properties alter hash
            var xa = new Vector4(2f, 1f, 1f, 1f);
            var xb = new Vector4(3f, 1f, 1f, 1f);
            var ya = new Vector4(1f, 2f, 1f, 1f);
            var yb = new Vector4(1f, 3f, 1f, 1f);
            var za = new Vector4(1f, 1f, 2f, 1f);
            var zb = new Vector4(1f, 1f, 3f, 1f);
            var wa = new Vector4(1f, 1f, 1f, 2f);
            var wb = new Vector4(1f, 1f, 1f, 3f);
            Assert.AreNotEqual(xa.GetHashCode(), xb.GetHashCode(), "Different properties should change hash.");
            Assert.AreNotEqual(ya.GetHashCode(), yb.GetHashCode(), "Different properties should change hash.");
            Assert.AreNotEqual(za.GetHashCode(), zb.GetHashCode(), "Different properties should change hash.");
            Assert.AreNotEqual(wa.GetHashCode(), wb.GetHashCode(), "Different properties should change hash.");
#if !XNA
            Assert.AreNotEqual(xa.GetHashCode(), ya.GetHashCode(), "Identical values on different properties should have different hashes.");
            Assert.AreNotEqual(xb.GetHashCode(), yb.GetHashCode(), "Identical values on different properties should have different hashes.");
            Assert.AreNotEqual(xb.GetHashCode(), zb.GetHashCode(), "Identical values on different properties should have different hashes.");
            Assert.AreNotEqual(yb.GetHashCode(), zb.GetHashCode(), "Identical values on different properties should have different hashes.");
            Assert.AreNotEqual(xb.GetHashCode(), wb.GetHashCode(), "Identical values on different properties should have different hashes.");
            Assert.AreNotEqual(yb.GetHashCode(), wb.GetHashCode(), "Identical values on different properties should have different hashes.");
#endif
            Assert.AreNotEqual(xa.GetHashCode(), yb.GetHashCode());
            Assert.AreNotEqual(ya.GetHashCode(), xb.GetHashCode());
            Assert.AreNotEqual(xa.GetHashCode(), zb.GetHashCode());
            Assert.AreNotEqual(xa.GetHashCode(), wb.GetHashCode());
        }

#if !XNA
        [Test]
        public void Deconstruct()
        {
            Vector4 vector4 = new Vector4(float.MinValue, float.MaxValue, float.MinValue, float.MaxValue);

            float x, y, z, w;

            vector4.Deconstruct(out x, out y, out z, out w);

            Assert.AreEqual(x, vector4.X);
            Assert.AreEqual(y, vector4.Y);
            Assert.AreEqual(z, vector4.Z);
            Assert.AreEqual(w, vector4.W);
        }

        [Test]
        public void Round()
        {
            Vector4 vector4 = new Vector4(0.0f, 0.4f, 0.6f, 1.0f);

            // CEILING

            Vector4 ceilMember = vector4;
            ceilMember.Ceiling();

            Vector4 ceilResult;
            Vector4.Ceiling(ref vector4, out ceilResult);

            Assert.AreEqual(new Vector4(0.0f, 1.0f, 1.0f, 1.0f), ceilMember);
            Assert.AreEqual(new Vector4(0.0f, 1.0f, 1.0f, 1.0f), Vector4.Ceiling(vector4));
            Assert.AreEqual(new Vector4(0.0f, 1.0f, 1.0f, 1.0f), ceilResult);

            // FLOOR

            Vector4 floorMember = vector4;
            floorMember.Floor();

            Vector4 floorResult;
            Vector4.Floor(ref vector4, out floorResult);

            Assert.AreEqual(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), floorMember);
            Assert.AreEqual(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), Vector4.Floor(vector4));
            Assert.AreEqual(new Vector4(0.0f, 0.0f, 0.0f, 1.0f), floorResult);

            // ROUND

            Vector4 roundMember = vector4;
            roundMember.Round();

            Vector4 roundResult;
            Vector4.Round(ref vector4, out roundResult);

            Assert.AreEqual(new Vector4(0.0f, 0.0f, 1.0f, 1.0f), roundMember);
            Assert.AreEqual(new Vector4(0.0f, 0.0f, 1.0f, 1.0f), Vector4.Round(vector4));
            Assert.AreEqual(new Vector4(0.0f, 0.0f, 1.0f, 1.0f), roundResult);
        }
#endif
    }
}
