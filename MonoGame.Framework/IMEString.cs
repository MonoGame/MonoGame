using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Garbageless string implementation only for IME usage.
    /// </summary>
    public unsafe struct IMEString : IEnumerable<char>
    {
        internal const int IMECharBufferSize = 64;

        /// <summary>
        /// Default empty IME string.
        /// </summary>
        public static readonly IMEString Empty = new IMEString((List<char>)null);

        internal struct Enumerator : IEnumerator<char>
        {
            private IMEString _imeString;
            private char _currentCharacter;
            private int _currentIndex;

            public Enumerator(IMEString imeString)
            {
                _imeString = imeString;
                _currentCharacter = '\0';
                _currentIndex = -1;
            }

            public bool MoveNext()
            {
                int size = _imeString.Count;

                _currentIndex++;

                if (_currentIndex == size)
                    return false;

                fixed (char* ptr = _imeString.buffer)
                {
                    _currentCharacter = *(ptr + _currentIndex);
                }

                return true;
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public void Dispose()
            {
            }

            public char Current { get { return _currentCharacter; } }
            object IEnumerator.Current { get { return Current; } }
        }

        /// <summary>
        /// IME string length.
        /// </summary>
        public int Count { get { return _size; } }

        /// <summary>
        /// Indexer.
        /// </summary>
        public char this[int index]
        {
            get
            {
                if (index >= Count || index < 0)
                    throw new ArgumentOutOfRangeException("index");

                fixed (char* ptr = buffer)
                {
                    return *(ptr + index);
                }
            }
        }

        private int _size;

        fixed char buffer[IMECharBufferSize];

        /// <summary>
        /// Construct a IME string with a normal string.
        /// </summary>
        public IMEString(string characters)
        {
            if (string.IsNullOrEmpty(characters))
            {
                _size = 0;
                return;
            }

            _size = characters.Length;
            if (_size > IMECharBufferSize)
                _size = IMECharBufferSize - 1;

            fixed (char* _ptr = buffer)
            {
                char* ptr = _ptr;
                for (var i = 0; i < _size; i++)
                {
                    *ptr = characters[i];
                    ptr++;
                }
            }
        }

        /// <summary>
        /// Construct a IME string with char List.
        /// </summary>
        public IMEString(List<char> characters)
        {
            if (characters == null || characters.Count == 0)
            {
                _size = 0;
                return;
            }

            _size = characters.Count;
            if (_size > IMECharBufferSize)
                _size = IMECharBufferSize - 1;

            fixed (char* _ptr = buffer)
            {
                char* ptr = _ptr;
                for (var i = 0; i < _size; i++)
                {
                    *ptr = characters[i];
                    ptr++;
                }
            }
        }

        /// <summary>
        /// Construct a IME string with char array and char count.
        /// </summary>
        public IMEString(char[] characters, int count)
        {
            if (characters == null || count <= 0)
            {
                _size = 0;
                return;
            }

            _size = count;
            if (_size > IMECharBufferSize)
                _size = IMECharBufferSize - 1;

            if (_size > characters.Length)
                _size = characters.Length;

            fixed (char* _ptr = buffer)
            {
                char* ptr = _ptr;
                for (var i = 0; i < _size; i++)
                {
                    *ptr = characters[i];
                    ptr++;
                }
            }
        }

        /// <summary>
        /// Construct a IME string with IntPtr of a string.
        /// </summary>
        public IMEString(IntPtr bStrPtr)
        {
            if (bStrPtr == IntPtr.Zero)
            {
                _size = 0;
                return;
            }

            var ptrSrc = (char*)bStrPtr;

            int i = 0;

            fixed (char* _ptr = buffer)
            {
                char* ptr = _ptr;

                while (ptrSrc[i] != '\0')
                {
                    *ptr = ptrSrc[i];
                    i++;
                    ptr++;
                }
            }

            _size = i;
        }

        /// <summary>
        /// To normal string.
        /// </summary>
        public override string ToString()
        {
            fixed (char* ptr = buffer)
            {
                return new string(ptr, 0, _size);
            }
        }

        /// <summary>
        /// Get the enumerator.
        /// </summary>
        public IEnumerator<char> GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
