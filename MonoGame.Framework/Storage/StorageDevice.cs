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

        private string GetDevicePath => _storageContainer?._storagePath ?? StorageRoot;

        internal StorageDevice(PlayerIndex? player, int sizeInBytes, int directoryCount)
        {
            _player = player;
            _sizeInBytes = sizeInBytes;
            _directoryCount = directoryCount;
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

        public Task<StorageContainer> OpenContainerAsync(string titleName, CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Open(titleName), cancellationToken);
        }

        private StorageContainer Open(string titleName)
        {
            ArgumentNullException.ThrowIfNull(titleName);

            try
            {
                _storageContainer = new StorageContainer(this, titleName, _player);
                File.Exists(GetDevicePath);
                return _storageContainer;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}