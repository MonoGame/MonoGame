using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace MonoGame.Tests.Framework
{
    class RGBA64Test
    {
        Rgba64[] subject=new Rgba64[2];
        float r=64764;
        float g=53970;
        float b=10794;
        float a=20303;
        float div=0xFFFF;
        ulong packed=5714832815570484476;
        [SetUp]
        public void preTestCleaning()
        {
            subject[0] = new Rgba64();
        }
        [Test]
        public void PackagingTest()
        {
            subject[0] = new Rgba64(r / div, g / div, b / div, a / div);
            Assert.AreEqual(packed, subject[0].PackedValue);
            Vector4 output = subject[0].ToVector4();
            Assert.AreEqual(r, (int) (output.X * div));
            Assert.AreEqual(g, (int) (output.Y * div));
            Assert.AreEqual(b, (int) (output.Z * div));
            Assert.AreEqual(a, (int) (output.W * div));
        }
        [Test]
        public void MemCopyTest()
        {
            byte[] data = new byte[] { 252, 252, 210, 210, 42, 42, 79, 79 };
            var dataHandle = GCHandle.Alloc(subject, GCHandleType.Pinned);
            var dataPtr = (IntPtr) dataHandle.AddrOfPinnedObject().ToInt64();

            Marshal.Copy(data, 0, dataPtr, data.Length);

            dataHandle.Free();
            Vector4 output = subject[0].ToVector4();
            Assert.AreEqual(r, (int)(output.X * div));
            Assert.AreEqual(g, (int) (output.Y * div));
            Assert.AreEqual(b, (int) (output.Z * div));
            Assert.AreEqual(a, (int) (output.W * div));


            Assert.AreEqual(packed, subject[0].PackedValue);
        }
    }
}
