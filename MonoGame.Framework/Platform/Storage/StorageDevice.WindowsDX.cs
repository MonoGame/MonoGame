using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageDevice
    {
        internal static string StorageRoot
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
        }
    }
}