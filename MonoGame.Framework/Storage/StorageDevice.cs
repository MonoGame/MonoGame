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
    }
}