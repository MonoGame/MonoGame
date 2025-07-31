using System;
using System.Diagnostics;
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

        public void DeleteContainerAsync(string containerName, CancellationToken cancellationToken)
        {
            Task.Run(() => DeleteContainer(containerName), cancellationToken);
        }

        public void DeleteContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException("containerName", "A container name must be provided.");

            // safely delete it.
            PlatformDeleteContainer(containerName);
        }

        public Task<StorageContainer> OpenContainerAsync(string containerName, CancellationToken cancellationToken)
        {
            return Task.Run(() => OpenContainer(containerName), cancellationToken);
        }

        public StorageContainer OpenContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException("containerName", "A container name must be provided.");

            try
            {
                _storageContainer = PlatformOpenContainer(containerName);
                if (_storageContainer == null)
                {
                    Debug.WriteLine("Failed to open storage container: {0}", containerName);
                    return null;
                }
                return _storageContainer;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to open storage container: {0}. Error {1}", containerName, ex.Message);
                return null;
            }
        }
    }
}