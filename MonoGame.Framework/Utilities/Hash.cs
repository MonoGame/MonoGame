// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;

namespace MonoGame.Utilities
{
    internal static class Hash
    {
        // Modified FNV Hash in C#
        // http://stackoverflow.com/a/468084
        internal static int ComputeHash(params byte[] data)
        {
            unchecked
            {
                const int p = 16777619;
                int hash = (int)2166136261;

                for (int i = 0; i < data.Length; i++)
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
        /// Compute Hash from a Stream's current position to the end.
        /// Stream position is restored.
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
                int hash = (int)2166136261;

                long prevPosition = stream.Position;
                byte[] data = new byte[1024];
                int length = 0;
                while((length = stream.Read(data, 0, data.Length)) != 0)
                {
                    for (int i = 0; i < length; i++)
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
