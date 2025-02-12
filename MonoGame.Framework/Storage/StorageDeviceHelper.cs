// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Storage
{
    /// <summary>
    /// This is a helper class to obtain the native file system information.
    /// </summary>
    /// <remarks>Look at the Mac implementation.</remarks>
    internal class StorageDeviceHelper
    {
        static string path = string.Empty;

        static StorageDeviceHelper() { }

        /// <summary>
        /// Gets or sets path for root of the <see cref="StorageDevice"/>.
        /// </summary>
		internal static string Path
        {
            get
            {
                return path;
            }

            set
            {
                if (path != value)
                {
                    path = value;
                }
            }
        }

        internal static long FreeSpace
        {
            get
            {
                long free = 0;
                return free;
            }
        }

        internal static long TotalSpace
        {
            get
            {
                long space = 0;
                return space;
            }
        }
    }
}
