// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoGame.Utilities
{
    internal static class InteropHelpers
    {
        /// <summary>
        /// Convert a pointer to a Utf8 null-terminated string to a .NET System.String
        /// </summary>
        public static unsafe string Utf8ToString(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                return string.Empty;

            var ptr = (byte*) handle;
            while (*ptr != 0)
                ptr++;

            var len = ptr - (byte*) handle;
            if (len == 0)
                return string.Empty;

            var bytes = new byte[len];
            Marshal.Copy(handle, bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
