using MonoGame.Utilities;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    public class ByteBufferPoolTest
    {
        [Test]
        public void CorrectPoolingTest()
        {
            var pool = new ByteBufferPool();

            var buf = pool.Get(5);
            pool.Return(buf);
            Assert.AreEqual(1, pool.FreeAmount);

            var buf2 = pool.Get(4);
            pool.Return(buf2);
            Assert.AreEqual(1, pool.FreeAmount);

            var buf3 = pool.Get(6);
            pool.Return(buf3);
            Assert.AreEqual(2, pool.FreeAmount);
        }
    }
}
