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

    private long PlatformTotalSpace()
    {
        throw new NotImplementedException();
    }

    private bool PlatformIsConnected()
    {
        throw new NotImplementedException();
    }

    private long PlatformFreeSpace()
    {
        throw new NotImplementedException();
    }

    private StorageContainer PlatformOpenContainer(string containerName)
    {
        throw new NotImplementedException();
    }

    private void PlatformDeleteContainer(string containerName)
    {
        throw new NotImplementedException();
    }
}