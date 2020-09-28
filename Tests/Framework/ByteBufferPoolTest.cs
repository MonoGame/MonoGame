using MonoGame.Framework.Utilities;
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

            // ByteBufferPool removes a buffer when none of the buffers in the pool
            // is large enough to satisfy a Get, and it has at least 1 buffer in the pool.
            // This way the number of items in the pool does not grow beyond the number
            // of buffers that are in use simultaneously, lowering overall memory usage.

            // the following Get should remove the size 5 buffer from the pool because it is not in use
            var buf3 = pool.Get(6);
            pool.Return(buf3);
            Assert.AreEqual(1, pool.FreeAmount);
            buf3 = pool.Get(6);

            var buf4 = pool.Get(5);
            pool.Return(buf4);
            pool.Return(buf3);
            Assert.AreEqual(2, pool.FreeAmount);
        }
    }
}
