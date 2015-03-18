using Microsoft.Xna.Framework;
using NUnit.Framework;
using System.ComponentModel;
using System.Globalization;

namespace MonoGame.Tests.Framework
{
    class Vector3Test
    {
        [Test]
        public void Properties()
        {
            Assert.AreEqual(new Vector3(0, 0, 0), Vector3.Zero);
            Assert.AreEqual(new Vector3(1, 1, 1), Vector3.One);
            Assert.AreEqual(new Vector3(1, 0, 0), Vector3.UnitX);
            Assert.AreEqual(new Vector3(0, 1, 0), Vector3.UnitY);
            Assert.AreEqual(new Vector3(0, 0, 1), Vector3.UnitZ);
            Assert.AreEqual(new Vector3(0, 1, 0), Vector3.Up);
            Assert.AreEqual(new Vector3(0, -1, 0), Vector3.Down);
            Assert.AreEqual(new Vector3(-1, 0, 0), Vector3.Left);
            Assert.AreEqual(new Vector3(1, 0, 0), Vector3.Right);
            Assert.AreEqual(new Vector3(0, 0, -1), Vector3.Forward);
            Assert.AreEqual(new Vector3(0, 0, 1), Vector3.Backward);
        }

        [Test]
        public void Cross()
        {
            Vector3 vOut;
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(2, 3.5f, -1.1f);
            var result = new Vector3(-12.7f, 7.1f, -0.5f);
            Vector3.Cross(ref v1, ref v2, out vOut);

            Assert.AreEqual(result, vOut);
            Assert.AreEqual(vOut, Vector3.Cross(v1, v2));
        }

        [Test]
        public void Distance()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(2, 3.5f, -1.1f);
            float fOut;
            const float result = 4.478839f;
            Vector3.Distance(ref v1, ref v2, out fOut);

            Assert.AreEqual(result, fOut);
            Assert.AreEqual(fOut, Vector3.Distance(v1, v2));
        }

        [Test]
        public void DistanceSquared()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(2, 3.5f, -1.1f);
            float fOut;
            const float result = 20.06f;
            Vector3.DistanceSquared(ref v1, ref v2, out fOut);

            Assert.AreEqual(result, fOut);
            Assert.AreEqual(fOut, Vector3.DistanceSquared(v1, v2));
        }

        [Test]
        public void Dot()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(2, 3.5f, -1.1f);
            float fOut;
            const float result = 5.7f;
            Vector3.Dot(ref v1, ref v2, out fOut);

            Assert.AreEqual(result, fOut);
            Assert.AreEqual(fOut, Vector3.Dot(v1, v2));
        }

        [Test]
        public void Hermite()
        {
            var v1 = new Vector3(1, 2, 3);
            var t1 = new Vector3(0.4f, 0.2f, 0.1f);
            var v2 = new Vector3(4, 2, 1);
            var t2 = new Vector3(10, 3, 1.4f);
            Vector3 vOut;
            const float amount = 0.5f;
            var result = new Vector3(1.3f,1.65f,1.8375f);
            Vector3.Hermite(ref v1,ref t1,ref v2,ref t2, amount,out vOut);

            Assert.AreEqual(result, vOut);
            Assert.AreEqual(vOut,Vector3.Hermite(v1,t1,v2,t2,amount));
        }

        [Test]
        public void Length()
        {
            var result = 3.7416575f;
            Assert.AreEqual(result, new Vector3(1, 2, 3).Length());
        }

        [Test]
        public void LengthSquared()
        {
            var result = 14.0f;
            Assert.AreEqual(result, new Vector3(1, 2, 3).LengthSquared());
        }

        [Test]
        public void Multiply()
        {
            var v1 = new Vector3(1, 2, 3);
            var v2 = new Vector3(1, 2, 3);
            Vector3 vOut;
            Vector3.Multiply(ref v1, ref v2, out vOut);
            Assert.AreEqual(new Vector3(1, 4, 9), vOut);
            Assert.AreEqual(vOut,Vector3.Multiply(v1,v2));
        }

        [Test]
        public void ToStringTest()
        {
            Assert.AreEqual("{X:0 Y:1 Z:2}", new Vector3(0, 1, 2).ToString());
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
    }
}
