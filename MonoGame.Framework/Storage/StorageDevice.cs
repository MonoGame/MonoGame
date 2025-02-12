using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Storage
{
    public delegate Task<StorageDevice> ShowSelectorAsync(PlayerIndex? player, int sizeInBytes, int directoryCount, CancellationToken cancellationToken = default);
    public delegate Task<StorageContainer> OpenContainerAsync(string displayName, CancellationToken cancellationToken = default);

    public sealed class StorageDevice
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
                    StorageDeviceHelper.Path = StorageRoot;
                    return StorageDeviceHelper.FreeSpace;
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
                    StorageDeviceHelper.Path = StorageRoot;
                    return StorageDeviceHelper.TotalSpace;
                }
            }
        }

        private string GetDevicePath => _storageContainer?._storagePath ?? StorageRoot;

        public static event EventHandler<EventArgs> DeviceChanged;

        public Task<StorageContainer> OpenContainerAsync(string displayName, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Open(displayName), cancellationToken);
        }

        private StorageContainer Open(string displayName)
        {
            _storageContainer = new StorageContainer(this, displayName, _player);
            return _storageContainer;
        }

        public static Task<StorageDevice> ShowSelectorAsync(int sizeInBytes, int directoryCount, CancellationToken cancellationToken = default)
        {
            return ShowSelectorAsync(null, sizeInBytes, directoryCount, cancellationToken);
        }

        public static Task<StorageDevice> ShowSelectorAsync(PlayerIndex? player, int sizeInBytes, int directoryCount, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Show(player, sizeInBytes, directoryCount), cancellationToken);
        }

        private static StorageDevice Show(PlayerIndex? player, int sizeInBytes, int directoryCount)
        {
            return new StorageDevice(player, sizeInBytes, directoryCount);
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
