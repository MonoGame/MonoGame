// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using System.Reflection;

namespace MonoGame.Framework.Utilities
{
    /// This works similar to .NET System.HashCode
    /// for building hash values incrementally.
    /// <remarks>
    /// Uses a modified FNV Hash in C#: http://stackoverflow.com/a/468084
    /// </remarks>
    internal struct Hash
    {
        private const int Prime = 16777619;
        private const int Default = unchecked((int)(2166136261));

        private bool _initialize;
        private int _hash;

        // The currently calculated hash.
        public readonly int Value => _hash;

        private void Init()
        {
            if (!_initialize)
            {
                _initialize = true;
                _hash = Default;
            }
        }

        /// <summary>
        /// Adds an integer to the hash.
        /// </summary>
        public void Add(int value)
        {
            Init();

            unchecked
            {
                _hash = (_hash ^ value) * Prime;
                _hash += _hash << 13;
                _hash ^= _hash >> 7;
                _hash += _hash << 3;
                _hash ^= _hash >> 17;
                _hash += _hash << 5;
            }
        }

        /// <summary>
        /// Adds a string to the hash.
        /// </summary>
        public void Add(string value)
        {
            Init();

            unchecked
            {
                for (var i = 0; i < value.Length; i++)
                    _hash = (_hash ^ value[i]) * Prime;

                _hash += _hash << 13;
                _hash ^= _hash >> 7;
                _hash += _hash << 3;
                _hash ^= _hash >> 17;
                _hash += _hash << 5;
            }
        }

        /// <summary>
        /// Compute a hash from a byte array.
        /// </summary>
        public static int ComputeHash(params byte[] data)
        {
            unchecked
            {
                var hash = Default;

                for (var i = 0; i < data.Length; i++)
                    hash = (hash ^ data[i]) * Prime;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
        
        /// <summary>
        /// Compute a hash from the content of a stream and restore the position.
        /// </summary>
        public static int ComputeHash(Stream stream)
        {
            System.Diagnostics.Debug.Assert(stream.CanSeek);

            unchecked
            {
                var hash = Default;

                var prevPosition = stream.Position;
                stream.Position = 0;

                var data = new byte[1024];
                int length;
                while((length = stream.Read(data, 0, data.Length)) != 0)
                {
                    for (var i = 0; i < length; i++)
                        hash = (hash ^ data[i]) * Prime;
                }

                // Restore stream position.
                stream.Position = prevPosition;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
    }
}
