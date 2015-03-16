// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace MonoGame.Utilities
{
    internal static class Hash
    {
        /// <summary>
        /// Compute a hash from a byte array.
        /// </summary>
        /// <remarks>
        /// Modified FNV Hash in C#
        /// http://stackoverflow.com/a/468084
        /// </remarks>
        internal static int ComputeHash(params byte[] data)
        {
            unchecked
            {
                const int p = 16777619;
                var hash = (int)2166136261;

                for (var i = 0; i < data.Length; i++)
                    hash = (hash ^ data[i]) * p;

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
        /// <remarks>
        /// Modified FNV Hash in C#
        /// http://stackoverflow.com/a/468084
        /// </remarks>
        internal static int ComputeHash(Stream stream)
        {
            System.Diagnostics.Debug.Assert(stream.CanSeek);

            unchecked
            {
                const int p = 16777619;
                var hash = (int)2166136261;

                var prevPosition = stream.Position;
                stream.Position = 0;

                var data = new byte[1024];
                int length;
                while((length = stream.Read(data, 0, data.Length)) != 0)
                {
                    for (var i = 0; i < length; i++)
                        hash = (hash ^ data[i]) * p;
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
