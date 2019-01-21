using Microsoft.Xna.Framework;
using NUnit.Framework;
using System.ComponentModel;
using System.Globalization;

namespace MonoGame.Tests.Framework
{
    class Vector3Test
    {
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
        public void DistanceSquared()
        {
            var v1 = new Vector3(0.1f, 100.0f, -5.5f);
            var v2 = new Vector3(1.1f, -2.0f, 5.5f);
            var d = Vector3.DistanceSquared(v1, v2);
            var expectedResult = 10526f;
            Assert.AreEqual(expectedResult, d);
        }

        [Test]
        public void Normalize()
        {
            Vector3 v1 = new Vector3(-10.5f, 0.2f, 1000.0f);
            Vector3 v2 = new Vector3(-10.5f, 0.2f, 1000.0f);
            v1.Normalize();
            var expectedResult = new Vector3(-0.0104994215f, 0.000199988979f, 0.999944866f);
            Assert.That(expectedResult, Is.EqualTo(v1).Using(Vector3Comparer.Epsilon));
            v2 = Vector3.Normalize(v2);
            Assert.That(expectedResult, Is.EqualTo(v2).Using(Vector3Comparer.Epsilon));
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

            Assert.That(expectedResult1, Is.EqualTo(Vector3.Transform(v1, m1)).Using(Vector3Comparer.Epsilon));
            Assert.That(expectedResult2, Is.EqualTo(Vector3.Transform(v2, q1)).Using(Vector3Comparer.Epsilon));

            // OUTPUT OVERLOADS TEST

            Vector3.Transform(ref v1, ref m1, out result1);
            Vector3.Transform(ref v2, ref q1, out result2);

            Assert.That(expectedResult1, Is.EqualTo(result1).Using(Vector3Comparer.Epsilon));
            Assert.That(expectedResult2, Is.EqualTo(result2).Using(Vector3Comparer.Epsilon));
        }

        [Test]
        public void HashCode() {
            // Checking for overflows in hash calculation.
            var max = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var min = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Assert.AreNotEqual(max.GetHashCode(), Vector3.Zero.GetHashCode());
            Assert.AreNotEqual(min.GetHashCode(), Vector3.Zero.GetHashCode());

            // Common values
            var a = new Vector3(0f, 0f, 0f);
            Assert.AreEqual(a.GetHashCode(), Vector3.Zero.GetHashCode());
            Assert.AreNotEqual(a.GetHashCode(), Vector3.One.GetHashCode());

            // Individual properties alter hash
            var xa = new Vector3(2f, 1f, 1f);
            var xb = new Vector3(3f, 1f, 1f);
            var ya = new Vector3(1f, 2f, 1f);
            var yb = new Vector3(1f, 3f, 1f);
            var za = new Vector3(1f, 1f, 2f);
            var zb = new Vector3(1f, 1f, 3f);
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

#if !XNA
        [Test]
        public void Deconstruct()
        {
            Vector3 vector3 = new Vector3(float.MinValue, float.MaxValue, float.MinValue);

            float x, y, z;

            vector3.Deconstruct(out x, out y, out z);

            Assert.AreEqual(x, vector3.X);
            Assert.AreEqual(y, vector3.Y);
            Assert.AreEqual(z, vector3.Z);
        }

        [Test]
        public void Round()
        {
            Vector3 vector3 = new Vector3(0.4f, 0.6f, 1.0f);

            // CEILING

            Vector3 ceilMember = vector3;
            ceilMember.Ceiling();

            Vector3 ceilResult;
            Vector3.Ceiling(ref vector3, out ceilResult);

            Assert.AreEqual(new Vector3(1.0f, 1.0f, 1.0f), ceilMember);
            Assert.AreEqual(new Vector3(1.0f, 1.0f, 1.0f), Vector3.Ceiling(vector3));
            Assert.AreEqual(new Vector3(1.0f, 1.0f, 1.0f), ceilResult);

            // FLOOR

            Vector3 floorMember = vector3;
            floorMember.Floor();

            Vector3 floorResult;
            Vector3.Floor(ref vector3, out floorResult);

            Assert.AreEqual(new Vector3(0.0f, 0.0f, 1.0f), floorMember);
            Assert.AreEqual(new Vector3(0.0f, 0.0f, 1.0f), Vector3.Floor(vector3));
            Assert.AreEqual(new Vector3(0.0f, 0.0f, 1.0f), floorResult);

            // ROUND

            Vector3 roundMember = vector3;
            roundMember.Round();

            Vector3 roundResult;
            Vector3.Round(ref vector3, out roundResult);

            Assert.AreEqual(new Vector3(0.0f, 1.0f, 1.0f), roundMember);
            Assert.AreEqual(new Vector3(0.0f, 1.0f, 1.0f), Vector3.Round(vector3));
            Assert.AreEqual(new Vector3(0.0f, 1.0f, 1.0f), roundResult);
        }
#endif
    }
}
