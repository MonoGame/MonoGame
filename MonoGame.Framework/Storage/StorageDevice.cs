using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageDevice
    {
        private readonly PlayerIndex? _player;
        private readonly int _sizeInBytes;
        private readonly int _directoryCount;
        private StorageContainer _storageContainer;

        internal StorageDevice(PlayerIndex? player, int sizeInBytes, int directoryCount)
        {
            _player = player;
            _sizeInBytes = sizeInBytes;
            _directoryCount = directoryCount;
        }

        public long FreeSpace
        {
            get
            {
                try
                {
                    return new DriveInfo(GetDevicePath).AvailableFreeSpace;
                }
                catch (Exception)
                {
                    /* TODO StorageDeviceHelper.Path = StorageRoot;
                    return StorageDeviceHelper.FreeSpace;*/
                    return -1;
                }
            }
        }

        public bool IsConnected
        {
            get
            {
                try
                {
                    return new DriveInfo(GetDevicePath).IsReady;
                }
                catch (Exception)
                {
                    return true;
                }
            }
        }

        public long TotalSpace
        {
            get
            {
                try
                {
                    return new DriveInfo(GetDevicePath).TotalSize;
                }
                catch (Exception)
                {
                    /* TODO StorageDeviceHelper.Path = StorageRoot;
                    return StorageDeviceHelper.TotalSpace; */
                    return -1;
                }
            }
        }

        private string GetDevicePath => _storageContainer?._storagePath ?? StorageRoot;

        public Task<StorageContainer> OpenContainerAsync(string displayName, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Open(displayName), cancellationToken);
        }

        private StorageContainer Open(string displayName)
        {
            _storageContainer = new StorageContainer(this, displayName, _player);
            return _storageContainer;
        }

        internal static string StorageRoot
        {
            get
            {
#if LINUX
                string osConfigDir = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
                if (string.IsNullOrEmpty(osConfigDir))
                {
                    osConfigDir = Environment.GetEnvironmentVariable("HOME");
                    if (string.IsNullOrEmpty(osConfigDir))
                    {
                        return "."; // Oh well.
                    }
                    osConfigDir += "/.local/share";
                }
                return osConfigDir;
#elif MAC
                string osConfigDir = Environment.GetEnvironmentVariable("HOME");
                if (string.IsNullOrEmpty(osConfigDir))
                {
                    return "."; // Oh well.
                }
                osConfigDir += "/Library/Application Support";
                return osConfigDir;
#else
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#endif
            }
        }
    }
}
