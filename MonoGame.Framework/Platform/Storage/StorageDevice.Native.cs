// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using MonoGame.Interop;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageDevice
    {
        internal unsafe MG_StorageDevice* _handle;

        private unsafe void PlatformInitialize()
        {
            _handle = MG_Storage.OpenDevice(_titleName, _player.HasValue ? (int)_player.Value : -1);
        }

        private unsafe void PlatformDispose()
        {
            MG_Storage.CloseDevice(_handle);
            _handle = null;
        }

        private unsafe long PlatformGetTotalSpace()
        {
            return MG_Storage.GetTotalSpace(_handle);
        }

        private unsafe long PlatformGetFreeSpace()
        {
            return MG_Storage.GetFreeSpace(_handle);
        }

        private unsafe StorageContainer PlatformOpenContainer(string containerName, long requiredFreeBytes)
        {
            MG_StorageContainer* container = MG_Storage.OpenContainer(_handle, containerName, requiredFreeBytes);
            if (container == null)
                return null;

            var sc = new StorageContainer(this, containerName, _player);
            sc._handle = container;

            return sc;
        }

        private unsafe void PlatformDeleteContainer(string containerName)
        {
            MG_Storage.DeleteContainer(_handle, containerName);
        }
    }
}
