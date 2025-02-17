using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageDevice
    {
        private readonly PlayerIndex? _player;
        private StorageContainer _storageContainer;
        private static readonly DriveInfo _driveInfo = new DriveInfo(StorageRoot);

        /// <summary>
        /// Gets the amount of free space on the device.
        /// </summary>
        public long FreeSpace
        {
            get
            {
                try
                {
                    return _driveInfo.AvailableFreeSpace;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets whether the device is connected or not.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                try
                {
                    return _driveInfo.IsReady;
                }
                catch (Exception)
                {
                    return false; // TODO Should this be true?
                }
            }
        }

        /// <summary>
        /// Gets the total amount of space on the device.
        /// </summary>
        public long TotalSpace
        {
            get
            {
                try
                {
                    return _driveInfo.TotalSize;
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        internal StorageDevice(PlayerIndex? player)
        {
            _player = player;
        }

        public void DeleteContainerAsync(string titleName, CancellationToken cancellationToken = default)
        {
            Task.Run(() => DeleteContainer(titleName), cancellationToken);
        }

        public void DeleteContainer(string titleName)
        {
            ArgumentNullException.ThrowIfNull(titleName);

            // If we are not connected, the Container should is not being used,
            // therefore we can safely delete it.
            if (!IsConnected)
            {
                // TODO actually delete things.
            }
        }

        public Task<StorageContainer> OpenContainerAsync(string displayName, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Open(displayName), cancellationToken);
        }

        private StorageContainer Open(string displayName)
        {
            ArgumentNullException.ThrowIfNull(displayName);

            try
            {
                _storageContainer = new StorageContainer(this, displayName, _player);
                return _storageContainer;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}