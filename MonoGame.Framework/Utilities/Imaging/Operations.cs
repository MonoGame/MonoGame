using System.Collections.Concurrent;

namespace MonoGame.Utilities
{
    internal static unsafe class Operations
    {
        internal static ConcurrentDictionary<long, Pointer> _pointers = new ConcurrentDictionary<long, Pointer>();

        public static long AllocatedTotal
        {
            get { return Pointer.AllocatedTotal; }
        }

        public static void* Malloc(long size)
        {
            var result = new PinnedArray<byte>(size);
            _pointers[(long) result.Ptr] = result;

            return result.Ptr;
        }

        public static void Memcpy(void* a, void* b, long size)
        {
            var ap = (byte*) a;
            var bp = (byte*) b;
            for (long i = 0; i < size; ++i)
            {
                *ap++ = *bp++;
            }
        }

        public static void MemMove(void* a, void* b, long size)
        {
            using (var temp = new PinnedArray<byte>(size))
            {
                Memcpy(temp.Ptr, b, size);
                Memcpy(a, temp.Ptr, size);
            }
        }

        public static void Free(void* a)
        {
            Pointer pointer;
            if (!_pointers.TryRemove((long) a, out pointer))
            {
                return;
            }

            pointer.Dispose();
        }

        public static void* Realloc(void* a, long newSize)
        {
            Pointer pointer;
            if (!_pointers.TryGetValue((long) a, out pointer))
            {
                // Allocate new
                return Malloc(newSize);
            }

            if (newSize <= pointer.Size)
            {
                // Realloc not required
                return a;
            }

            var result = Malloc(newSize);
            Memcpy(result, a, pointer.Size);

            _pointers.TryRemove((long) pointer.Ptr, out pointer);
            pointer.Dispose();

            return result;
        }

        public static int Memcmp(void* a, void* b, long size)
        {
            var result = 0;
            var ap = (byte*) a;
            var bp = (byte*) b;
            for (long i = 0; i < size; ++i)
            {
                if (*ap != *bp)
                {
                    result += 1;
                }
                ap++;
                bp++;
            }

            return result;
        }
    }
}