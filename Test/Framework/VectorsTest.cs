using Microsoft.Xna.Framework;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGame.Tests.Framework
{
    /// <summary>
    /// Tests for <see cref="Vector2"/>, <see cref="Vector3"/>, and <see cref="Vector4"/>.
    /// </summary>
    [TestFixture(Description = "Microsoft.Xml.Framework.Vector2, Vector3, and Vector4")]
    class VectorsTest
    {
        readonly Random Rng = new Random(0);
        float RngNextFloat() { return (float)Rng.NextDouble(); }
        readonly Matrix TestMatrix = Matrix.CreatePerspective(100, 20, 3, 450) * Matrix.CreateScale(6, 4, -3) * Matrix.CreateTranslation(432, 555, 734);

        Matrix CreateGoofyMatrix()
        {
            return Matrix.CreatePerspective(RngNextFloat() * 100, RngNextFloat() * 100, RngNextFloat(), RngNextFloat() * 10 + 1) *
                Matrix.CreateScale(RngNextFloat(), RngNextFloat(), -RngNextFloat()) *
                Matrix.CreateTranslation(RngNextFloat(), RngNextFloat(), RngNextFloat());
        }

        static bool Equal(Vector2 v, float x, float y)
        {
            return v.X == x && v.Y == y;
        }

        static bool Equal(Vector3 v, float x, float y, float z)
        {
            return v.X == x && v.Y == y && v.Z == z;
        }

        static bool Equal(Vector4 v, float x, float y, float z, float w)
        {
            return v.X == x && v.Y == y && v.Z == z && v.W == w;
        }

        static bool Equal(Vector2 v1, Vector2 v2) { return Equal(v1, v2.X, v2.Y); }
        static bool Equal(Vector3 v1, Vector3 v2) { return Equal(v1, v2.X, v2.Y, v2.Z); }
        static bool Equal(Vector4 v1, Vector4 v2) { return Equal(v1, v2.X, v2.Y, v2.Z, v2.W); }

        Vector4 Transform(float x, float y, float z, float w, Matrix m)
        {
            return new Vector4(
                x * m.M11 + y * m.M21 + z * m.M31 + w * m.M41,
                x * m.M12 + y * m.M22 + z * m.M32 + w * m.M42,
                x * m.M13 + y * m.M23 + z * m.M33 + w * m.M43,
                x * m.M14 + y * m.M24 + z * m.M34 + w * m.M44);
        }

        Vector3 Transform(float x, float y, float z, Matrix m)
        {
            Vector4 value = Transform(x, y, z, 1, m);
            return new Vector3(value.X, value.Y, value.Z);
        }

        Vector3 Transform(Vector3 value, Matrix matrix) { return Transform(value.X, value.Y, value.Z, matrix); }

        // Also tests accessors
        [Test]
        public void Constructors()
        {
            Vector2 a = new Vector2(1, 2);
            Vector3 b = new Vector3(3, 4, 5), c = new Vector3(new Vector2(6, 7), 8);
            Vector4 d = new Vector4(9, 10, 11, 12), e = new Vector4(new Vector2(13, 14), 15, 16), f = new Vector4(new Vector3(17, 18, 19), 20);

            Assert.IsTrue(Equal(a, 1, 2));
            Assert.IsTrue(Equal(b, 3, 4, 5));
            Assert.IsTrue(Equal(c, 6, 7, 8));
            Assert.IsTrue(Equal(d, 9, 10, 11, 12));
            Assert.IsTrue(Equal(e, 13, 14, 15, 16));
            Assert.IsTrue(Equal(f, 17, 18, 19, 20));
        }

        [Test]
        public void Properties()
        {
            Assert.IsTrue(Equal(Vector2.Zero, 0, 0));
            Assert.IsTrue(Equal(Vector3.Zero, 0, 0, 0));
            Assert.IsTrue(Equal(Vector4.Zero, 0, 0, 0, 0));
            Assert.IsTrue(Equal(Vector2.One, 1, 1));
            Assert.IsTrue(Equal(Vector3.One, 1, 1, 1));
            Assert.IsTrue(Equal(Vector4.One, 1, 1, 1, 1));
            Assert.IsTrue(Equal(Vector2.UnitX, 1, 0));
            Assert.IsTrue(Equal(Vector3.UnitX, 1, 0, 0));
            Assert.IsTrue(Equal(Vector4.UnitX, 1, 0, 0, 0));
            Assert.IsTrue(Equal(Vector2.UnitY, 0, 1));
            Assert.IsTrue(Equal(Vector3.UnitY, 0, 1, 0));
            Assert.IsTrue(Equal(Vector4.UnitY, 0, 1, 0, 0));
            Assert.IsTrue(Equal(Vector3.Backward, 0, 0, 1));
            Assert.IsTrue(Equal(Vector3.Down, 0, -1, 0));
            Assert.IsTrue(Equal(Vector3.Forward, 0, 0, -1));
            Assert.IsTrue(Equal(Vector3.Left, -1, 0, 0));
            Assert.IsTrue(Equal(Vector3.Right, 1, 0, 0));
            Assert.IsTrue(Equal(Vector3.Up, 0, 1, 0));
        }

        #region Public static methods

        // Mostly this is a test for the static templates. Because those are automatically generated, we assume everyone of them is done the same way (because they are).
        [Test]
        public void Barycentric()
        {
            Vector3 v1 = new Vector3(1, 2, 3), v2 = new Vector3(4, 5, 6), v3 = new Vector3(7, 8, 9);
            float a1 = 0.4344f, a2 = 0.7858f;
            Vector3 r = Vector3.Barycentric(v1, v2, v3, a1, a2);
            Assert.AreEqual(MathHelper.Barycentric(v1.X, v2.X, v3.X, a1, a2), r.X);
            Assert.AreEqual(MathHelper.Barycentric(v1.Y, v2.Y, v3.Y, a1, a2), r.Y);
            Assert.AreEqual(MathHelper.Barycentric(v1.Z, v2.Z, v3.Z, a1, a2), r.Z);

            // Test reference form once.
            Vector3.Barycentric(ref v1, ref v2, ref v3, a1, a2, out r);
            Assert.AreEqual(MathHelper.Barycentric(v1.X, v2.X, v3.X, a1, a2), r.X);
            Assert.AreEqual(MathHelper.Barycentric(v1.Y, v2.Y, v3.Y, a1, a2), r.Y);
            Assert.AreEqual(MathHelper.Barycentric(v1.Z, v2.Z, v3.Z, a1, a2), r.Z);
        }

        [Test]
        public void CatmullRom()
        {
            Vector2 v1 = new Vector2(1, 2), v2 = new Vector2(3, 4), v3 = new Vector2(5, 6), v4 = new Vector2(7, 8);
            float a = 0.23748f;
            Vector2 r = Vector2.CatmullRom(v1, v2, v3, v4, a);
            Assert.AreEqual(MathHelper.CatmullRom(v1.X, v2.X, v3.X, v4.X, a), r.X);
            Assert.AreEqual(MathHelper.CatmullRom(v1.Y, v2.Y, v3.Y, v4.Y, a), r.Y);
        }

        [Test]
        public void Clamp()
        {
            Vector2 v1 = new Vector2(10, 0), min = new Vector2(0, 1), max = new Vector2(8, 0);
            Vector2 r = Vector2.Clamp(v1, min, max);
            Assert.AreEqual(MathHelper.Clamp(v1.X, min.X, max.X), r.X);
            Assert.AreEqual(MathHelper.Clamp(v1.Y, min.Y, max.Y), r.Y);
        }

        [Test]
        public void Distance()
        {
            Vector4 v1 = new Vector4(1, 2, 3, 4), v2 = new Vector4(5, 6, 7, 8);
            float r = Vector4.Distance(v1, v2);
            Assert.AreEqual((float)Math.Sqrt((v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y) + (v2.Z - v1.Z) * (v2.Z - v1.Z) + (v2.W - v1.W) * (v2.W - v1.W)), r);
        }

        [Test]
        public void DistanceSquared()
        {
            Vector4 v1 = new Vector4(9, 10, 11, 12), v2 = new Vector4(13, 14, 15, 16);
            float r = Vector4.DistanceSquared(v1, v2);
            Assert.AreEqual((v2.X - v1.X) * (v2.X - v1.X) + (v2.Y - v1.Y) * (v2.Y - v1.Y) + (v2.Z - v1.Z) * (v2.Z - v1.Z) + (v2.W - v1.W) * (v2.W - v1.W), r);
        }

        [Test]
        public void DotProduct()
        {
            Vector3 v1 = new Vector3(1, -2, 3), v2 = new Vector3(-4, -5, -6);
            float r = Vector3.Dot(v1, v2);
            Assert.AreEqual(v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z, r);
        }

        [Test]
        public void Hermite()
        {
            Vector2 v1 = new Vector2(1, 2), v2 = new Vector2(3, 4), v3 = new Vector2(5, 6), v4 = new Vector2(7, 8);
            float a = 0.2384f;
            Vector2 r = Vector2.Hermite(v1, v2, v3, v4, a);
            Assert.AreEqual(MathHelper.Hermite(v1.X, v2.X, v3.X, v4.X, a), r.X);
            Assert.AreEqual(MathHelper.Hermite(v1.Y, v2.Y, v3.Y, v4.Y, a), r.Y);
        }

        [Test]
        public void Lerp()
        {
            Vector2 v1 = new Vector2(1, 2), v2 = new Vector2(3, 4);
            float a = 0.83744f;
            Vector2 r = Vector2.Lerp(v1, v2, a);

            Assert.AreEqual(MathHelper.Lerp(v1.X, v2.X, a), r.X);
            Assert.AreEqual(MathHelper.Lerp(v1.Y, v2.Y, a), r.Y);
        }

        [Test]
        public void Max()
        {
            Vector2 v1 = new Vector2(1, 3), v2 = new Vector2(4, 2);
            Vector2 r = Vector2.Max(v1, v2);

            Assert.AreEqual(MathHelper.Max(v1.X, v2.X), r.X);
            Assert.AreEqual(MathHelper.Max(v1.Y, v2.Y), r.Y);
        }

        [Test]
        public void Min()
        {
            Vector2 v1 = new Vector2(1, 3), v2 = new Vector2(4, 2);
            Vector2 r = Vector2.Min(v1, v2);

            Assert.AreEqual(MathHelper.Min(v1.X, v2.X), r.X);
            Assert.AreEqual(MathHelper.Min(v1.Y, v2.Y), r.Y);
        }

        [Test]
        public void Negate()
        {
            Vector4 v = new Vector4(1, -2, 400, -999);
            Vector4 r = Vector4.Negate(v);

            Assert.AreEqual(-v.X, r.X);
            Assert.AreEqual(-v.Y, r.Y);
            Assert.AreEqual(-v.Z, r.Z);
            Assert.AreEqual(-v.W, r.W);
            Assert.AreEqual(-v, r);
            Assert.AreEqual(+v, -r);
        }

        [Test]
        public void Normalize()
        {
            Vector3 v1 = new Vector3(1, 2, 3), v2 = new Vector3(0, 0, 0);
            Vector3 r1 = Vector3.Normalize(v1), r2 = Vector3.Normalize(v2);

            float l1 = 1.0f / (float)Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);

            Assert.AreEqual(v1.X * l1, r1.X);
            Assert.AreEqual(v1.Y * l1, r1.Y);
            Assert.AreEqual(v1.Z * l1, r1.Z);

            Assert.IsTrue(float.IsNaN(r2.X));
            Assert.IsTrue(float.IsNaN(r2.Y));
            Assert.IsTrue(float.IsNaN(r2.Z));
        }

        static float Reflect(float i, float n, float dotProduct) { return i - 2 * n * dotProduct; }

        [Test]
        public void Reflect()
        {
            Vector4 v1 = new Vector4(1, 2, 3, 4), v2 = Vector4.Normalize(new Vector4(5, 6, 7, 8));
            Vector4 r = Vector4.Reflect(v1, v2);

            float dotProduct = v1.X * v2.X + v1.Y * v2.Y + v1.Z * v2.Z + v1.W * v2.W;
            Assert.AreEqual(Reflect(v1.X, v2.X, dotProduct), r.X);
            Assert.AreEqual(Reflect(v1.Y, v2.Y, dotProduct), r.Y);
            Assert.AreEqual(Reflect(v1.Z, v2.Z, dotProduct), r.Z);
            Assert.AreEqual(Reflect(v1.W, v2.W, dotProduct), r.W);
        }

        [Test]
        public void SmoothStep()
        {
            Vector3 v1 = new Vector3(1, 2, 3), v2 = new Vector3(7, 11, 13);
            float a = 0.3847f;
            Vector3 r = Vector3.SmoothStep(v1, v2, a);

            Assert.AreEqual(MathHelper.SmoothStep(v1.X, v2.X, a), r.X);
            Assert.AreEqual(MathHelper.SmoothStep(v1.Y, v2.Y, a), r.Y);
            Assert.AreEqual(MathHelper.SmoothStep(v1.Z, v2.Z, a), r.Z);
        }

        [Test]
        public void Transform()
        {
            // Transform's special logic demands more testing.
            Vector2 v2 = new Vector2(4, 5);
            Vector3 v3 = new Vector3(1, 2, 3);
            Vector4 v4 = new Vector4(6, 7, 8, 9);
            Matrix m = TestMatrix;
            Vector2 r2 = Vector2.Transform(v2, m);
            Vector3 r3 = Vector3.Transform(v3, m);
            Vector4 r4 = Vector4.Transform(v4, m);

            Vector4 m2 = Transform(v2.X, v2.Y, 0, 1, m);
            Vector4 m3 = Transform(v3.X, v3.Y, v3.Z, 1, m);
            Vector4 m4 = Transform(v4.X, v4.Y, v4.Z, v4.W, m);

            Assert.IsTrue(Equal(r2, m2.X, m2.Y));
            Assert.IsTrue(Equal(r3, m3.X, m3.Y, m3.Z));
            Assert.IsTrue(Equal(r4, m4.X, m4.Y, m4.Z, m4.W));
        }

        [Test]
        public void TransformArrayMatrix()
        {
            int count = 64;
            Vector3[] input = new Vector3[count], output = new Vector3[count];
            Matrix matrix = TestMatrix;

            for(var index = 0; index < count; index++)
                input[index] = new Vector3(index * 3, index * 3 + 1, index * 3 + 2);

            // Do the basic test.
            Vector3.Transform(input, ref matrix, output);
            for (int index = 0; index < count; index++)
                Assert.AreEqual(output[index], Transform(input[index].X, input[index].Y, input[index].Z, matrix));

            // Now try transforming overlaps
            input.CopyTo(output, 0);
            Vector3.Transform(output, 0, ref matrix, output, 16, 32);
            for (int index = 0; index < 32; index++)
                Assert.AreEqual(Transform(input[index], matrix), output[index + 16]);

            // The other overlap's normally safe but could be broken
            input.CopyTo(output, 0);
            Vector3.Transform(output, 16, ref matrix, output, 0, 32);
            for (int index = 0; index < 32; index++)
                Assert.AreEqual(Transform(input[index + 16], matrix), output[index]);
        }

        [Test]
        public void TransformArrayQuaternion()
        {
            int count = 64;
            Vector3[] input = new Vector3[count], output = new Vector3[count];
            Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(1, 2, 3);

            for (var index = 0; index < count; index++)
                input[index] = new Vector3(index * 3, index * 3 + 1, index * 3 + 2);

            // Do the basic test.
            Vector3.Transform(input, ref quaternion, output);
            for (int index = 0; index < count; index++)
                Assert.AreEqual(output[index], Vector3.Transform(input[index], quaternion));

            // Now try transforming overlaps
            input.CopyTo(output, 0);
            Vector3.Transform(output, 0, ref quaternion, output, 16, 32);
            for (int index = 0; index < 32; index++)
                Assert.AreEqual(Vector3.Transform(input[index], quaternion), output[index + 16]);

            // The other overlap's normally safe but could be broken
            input.CopyTo(output, 0);
            Vector3.Transform(output, 16, ref quaternion, output, 0, 32);
            for (int index = 0; index < 32; index++)
                Assert.AreEqual(Vector3.Transform(input[index + 16], quaternion), output[index]);
        }

        #endregion Public static methods

        [Test]
        public void Equals()
        {
            Vector3 v1 = new Vector3(1, 2, 3), v2 = new Vector3(1, 2, 4);

            Assert.IsFalse(v1.Equals(4));
            Assert.IsFalse(v1.Equals((object)v2));
            Assert.IsFalse(v1.Equals(v2));
            Assert.IsTrue(v1.Equals(v1));
            Assert.IsFalse(v1 == v2);
            Assert.IsTrue(v1 != v2);
            Assert.IsTrue(v1 == new Vector3(1, 2, 3));
            Assert.IsFalse(v2 != new Vector3(1, 2, 4));
        }

        [Test]
        public void Length()
        {
            Vector3 v = new Vector3(1, 2, 3);
            float r1 = v.Length(), r2 = v.LengthSquared();

            Assert.AreEqual(r1, (float)Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z));
            Assert.AreEqual(r2, v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        [Test]
        public void NormalizeInstance()
        {
            Vector3 v1 = new Vector3(1, 2, 3), v2 = new Vector3(0, 0, 0), o1 = v1;

            float l = 1.0f / (float)Math.Sqrt(v1.X * v1.X + v1.Y * v1.Y + v1.Z * v1.Z);
            v1.Normalize();
            v2.Normalize();

            Assert.IsTrue(Equal(v1, o1.X * l, o1.Y * l, o1.Z * l));
            Assert.IsTrue(float.IsNaN(v2.X));
            Assert.IsTrue(float.IsNaN(v2.Y));
            Assert.IsTrue(float.IsNaN(v2.Z));
        }

        [Test]
        public void ToStringInstance()
        {
            Assert.AreEqual("{X:0 Y:1 Z:2 W:3}", new Vector4(0, 1, 2, 3).ToString());
        }

        [Test]
        public void Add()
        {
            Vector2 v1 = new Vector2(1, 2), v2 = new Vector2(3, 4);
            float s = 5;

            Vector2 c1 = new Vector2(v1.X + v2.X, v1.Y + v2.Y);
            Vector2 c2 = new Vector2(v1.X + s, v1.Y + s);
            Vector2 c3 = new Vector2(s + v1.X, s + v1.Y);

            Assert.AreEqual(c1, v1 + v2);
            Assert.AreEqual(c1, Vector2.Add(v1, v2));
            Assert.AreEqual(c2, v1 + s);
            Assert.AreEqual(c2, Vector2.Add(v1, s));
            Assert.AreEqual(c3, s + v1);
            Assert.AreEqual(c3, Vector2.Add(s, v1));
        }

        [Test]
        public void Subtract()
        {
            Vector2 v1 = new Vector2(1, 2), v2 = new Vector2(3, 4);
            float s = 5;

            Vector2 c1 = new Vector2(v1.X - v2.X, v1.Y - v2.Y);
            Vector2 c2 = new Vector2(v1.X - s, v1.Y - s);
            Vector2 c3 = new Vector2(s - v1.X, s - v1.Y);

            Assert.AreEqual(c1, v1 - v2);
            Assert.AreEqual(c1, Vector2.Subtract(v1, v2));
            Assert.AreEqual(c2, v1 - s);
            Assert.AreEqual(c2, Vector2.Subtract(v1, s));
            Assert.AreEqual(c3, s - v1);
            Assert.AreEqual(c3, Vector2.Subtract(s, v1));
        }

        [Test]
        public void Multiply()
        {
            Vector2 v1 = new Vector2(1, 2), v2 = new Vector2(3, 4);
            float s = 5;

            Vector2 c1 = new Vector2(v1.X * v2.X, v1.Y * v2.Y);
            Vector2 c2 = new Vector2(v1.X * s, v1.Y * s);
            Vector2 c3 = new Vector2(s * v1.X, s * v1.Y);

            Assert.AreEqual(c1, v1 * v2);
            Assert.AreEqual(c1, Vector2.Multiply(v1, v2));
            Assert.AreEqual(c2, v1 * s);
            Assert.AreEqual(c2, Vector2.Multiply(v1, s));
            Assert.AreEqual(c3, s * v1);
            Assert.AreEqual(c3, Vector2.Multiply(s, v1));
        }

        [Test]
        public void Divide()
        {
            Vector2 v1 = new Vector2(1, 2), v2 = new Vector2(3, 4);
            float s = 5;

            Vector2 c1 = new Vector2(v1.X / v2.X, v1.Y / v2.Y);
            Vector2 c2 = new Vector2(v1.X / s, v1.Y / s);
            Vector2 c3 = new Vector2(s / v1.X, s / v1.Y);

            Assert.AreEqual(c1, v1 / v2);
            Assert.AreEqual(c1, Vector2.Divide(v1, v2));
            Assert.AreEqual(c2, v1 / s);
            Assert.AreEqual(c2, Vector2.Divide(v1, s));
            Assert.AreEqual(c3, s / v1);
            Assert.AreEqual(c3, Vector2.Divide(s, v1));
        }

        [Test]
        public void Modulo()
        {
            Vector2 v1 = new Vector2(1, 2), v2 = new Vector2(3, 4);
            float s = 5;

            Vector2 c1 = new Vector2(v1.X % v2.X, v1.Y % v2.Y);
            Vector2 c2 = new Vector2(v1.X % s, v1.Y % s);
            Vector2 c3 = new Vector2(s % v1.X, s % v1.Y);

            Assert.AreEqual(c1, v1 % v2);
            Assert.AreEqual(c1, Vector2.Modulo(v1, v2));
            Assert.AreEqual(c2, v1 % s);
            Assert.AreEqual(c2, Vector2.Modulo(v1, s));
            Assert.AreEqual(c3, s % v1);
            Assert.AreEqual(c3, Vector2.Modulo(s, v1));
        }
    }
}
