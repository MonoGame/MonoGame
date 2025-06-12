using System.Collections.Generic;

namespace MonoGame.Framework.Utilities
{
    internal class ByteBufferPool
    {
        private readonly int _minBufferSize;
        private readonly int _maxBuffers;

        public int FreeAmount
        {
            get { return _freeBuffers.Count; }
        }

        private readonly List<byte[]> _freeBuffers;

        public ByteBufferPool(int minBufferSize = 0, int maxBuffers = int.MaxValue)
        {
            _minBufferSize = minBufferSize;
            _maxBuffers = maxBuffers;
            _freeBuffers = new List<byte[]>();
        }

        /// <summary>
        /// Get a buffer that is at least as big as size.
        /// </summary>
        public byte[] Get(int size)
        {
            if (size < _minBufferSize)
                size = _minBufferSize;

            byte[] result;
            lock (_freeBuffers)
            {
                var index = FirstLargerThan(size);

                if (index == -1)
                {
                    if (_freeBuffers.Count > 0)
                        _freeBuffers.RemoveAt(0);
                    result = new byte[size];
                }
                else
                {
                    result = _freeBuffers[index];
                    _freeBuffers.RemoveAt(index);
                }
            }
            return result;
        }

        /// <summary>
        /// Return the given buffer to the pool.
        /// </summary>
        public void Return(byte[] buffer)
        {
            lock (_freeBuffers)
            {
                if (FreeAmount >= _maxBuffers)
                    return;
                var index = FirstLargerThan(buffer.Length);
                if (index == -1)
                    _freeBuffers.Add(buffer);
                else
                    _freeBuffers.Insert(index, buffer);
            }
        }

        // Find the smallest buffer that is larger than or equally large as size or -1 if none exist
        private int FirstLargerThan(int size)
        {
            if (_freeBuffers.Count == 0) return -1;

            var l = 0;
            var r = _freeBuffers.Count - 1;

            while (l <= r)
            {
                var m = (l + r)/2;
                var buffer = _freeBuffers[m];
                if (buffer.Length < size)
                {
                    l = m + 1;
                }
                else if (buffer.Length > size)
                {
                    r = m;
                    if (l == r) return l;
                }
                else
                {
                    return m;
                }
            }

            return -1;
        }
    }
}
