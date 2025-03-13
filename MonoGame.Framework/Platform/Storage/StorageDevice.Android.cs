using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageDevice
    {
        private static DriveInfo _driveInfo;

        internal static string StorageRoot
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            }
        }

        private void PlatformDeleteContainer(string containerName)
        {
            _storageContainer.DeleteDirectory(containerName);
        }

        private long PlatformFreeSpace()
        {
            return _driveInfo.AvailableFreeSpace;
        }

        private bool PlatformIsConnected()
        {
            return _driveInfo.IsReady;
        }

        private StorageContainer PlatformOpenContainer(string containerName)
        {
            _storageContainer = new StorageContainer(this, containerName, _player);

            if (_driveInfo == null)
                _driveInfo = new DriveInfo(StorageRoot);

            return _storageContainer;
        }

        private long PlatformTotalSpace()
        {
            return _driveInfo.TotalSize;
        }
    }
}