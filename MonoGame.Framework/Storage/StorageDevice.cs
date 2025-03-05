using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Storage
{
    public sealed partial class StorageDevice
    {
        private readonly PlayerIndex? _player;
        private StorageContainer _storageContainer;

        /// <summary>
        /// Gets the amount of free space on the device.
        /// </summary>
        public long FreeSpace
        {
            get
            {
                try
                {
                    return PlatformFreeSpace();
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets whether the device is connected, therefore ready or not.
        /// </summary>
        public bool IsConnected
        {
            get
            {
                try
                {
                    return PlatformIsConnected();
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
                    return PlatformTotalSpace();
                }
                catch (Exception)
                {
                    return 0;
                }
            }
        }

        public StorageDevice(PlayerIndex? player)
        {
            _player = player;
        }

        public void DeleteContainerAsync(string containerName, CancellationToken cancellationToken = default)
        {
            Task.Run(() => DeleteContainer(containerName), cancellationToken);
        }

        public void DeleteContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException(nameof(containerName), "A container name must be provided.");

            // If we are not connected, the Container should is not being used,
            // therefore we can safely delete it.
            if (!IsConnected)
            {
                PlatformDeleteContainer(containerName);
            }
        }

        public Task<StorageContainer> OpenContainerAsync(string containerName, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => OpenContainer(containerName), cancellationToken);
        }

        public StorageContainer OpenContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException(nameof(containerName), "A container name must be provided.");

            try
            {
                return PlatformOpenContainer(containerName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to open storage container: {ex.Message}");
                return null;
            }
        }
    }
}