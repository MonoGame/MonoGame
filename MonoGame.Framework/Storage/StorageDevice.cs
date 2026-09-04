// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Storage
{
    public sealed partial class StorageDevice : IDisposable
    {
        private string _titleName;
        private readonly PlayerIndex? _player;


        /// <summary>
        /// Gets a bool value indicating whether the instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the amount of free space on the device.
        /// </summary>
        public long FreeSpace
        {
            get
            {
                return PlatformGetFreeSpace();
            }
        }

        /// <summary>
        /// Gets the total amount of space on the device.
        /// </summary>
        public long TotalSpace
        {
            get
            {
                return PlatformGetTotalSpace();
            }
        }

        /// <summary>
        /// Creates a storage device for saving game data.
        /// </summary>
        /// <param name="titleName">The name of the game title used on some platforms for naming save data.</param>
        /// <param name="player">The optional player index.  TODO: Why?</param>
        public StorageDevice(string titleName, PlayerIndex? player)
        {
            // TODO: Validate titleName is Ascii.

            _titleName = titleName;

            // TODO: How do we go from a player index to an actual player on the console?  
            _player = player;

            PlatformInitialize();
        }

        ~StorageDevice()
        {
            PlatformDispose();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            PlatformDispose();
            GC.SuppressFinalize(this);

            IsDisposed = true;
        }

        /// <summary>
        /// Deletes the named container if it exists.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void DeleteContainer(string containerName)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException("containerName", "A container name must be provided.");

            PlatformDeleteContainer(containerName);
        }

        /// <summary>
        /// Opens and existing container or creates one.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="requiredFreeBytes">On container creation we check for this available space or return null.</param>
        /// <returns>The container or null if it could not be created.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public StorageContainer OpenContainer(string containerName, long requiredFreeBytes)
        {
            // TODO: Should containerName be "displayName" like in old XNA?

            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException("containerName", "A container name must be provided.");

            // TODO: Validate containerName is valid for storage file path!
            // I think this should be ASCII to support all platforms.


            if (requiredFreeBytes <= 0)
                throw new ArgumentOutOfRangeException("requiredFreeBytes", "Must be greater than 0.");

            try
            {
                var container = PlatformOpenContainer(containerName, requiredFreeBytes);
                if (container == null)
                {
                    Debug.WriteLine("Failed to open storage container: {0}", containerName);
                    return null;
                }
                return container;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to open storage container: {0}. Error {1}", containerName, ex.Message);
                return null;
            }
        }
    }
}
