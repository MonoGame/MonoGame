// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if NATIVE

using System;
using System.Linq;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Storage
{
    /// <summary>
    /// The storage system for saving user data on the platform's local storage.
    /// On some platforms this may also be backed up to cloud storage and
    /// synchronized across multiple devices.
    /// </summary>
    /// <remarks>This is class is in preview and not final.</remarks>
    public sealed partial class StorageDevice : IDisposable
    {
        private string _titleName;
        private readonly PlayerIndex? _player;

        private WeakReference<StorageContainer> _current;

        /// <summary>
        /// Returns true if the instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets the free space on the device in bytes.
        /// </summary>
        public long FreeSpace
        {
            get
            {
                return PlatformGetFreeSpace();
            }
        }

        /// <summary>
        /// Gets the total space on the device in bytes.
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
        /// <param name="player">The optional player index.</param>
        public StorageDevice(string titleName, PlayerIndex? player = PlayerIndex.One)
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

        private StorageContainer GetActiveContainer()
        {
            if (_current == null)
                return null;

            if (!_current.TryGetTarget(out var container))
                return null;

            if (container.IsDisposed)
                return null;

            return container;
        }

        /// <summary>
        /// Disposes the device and closes all containers.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            // Cleanup the last container if we have one still alive.
            var container = GetActiveContainer();
            if (container != null)
                container.Dispose();
            _current = null;

            PlatformDispose();
            GC.SuppressFinalize(this);

            IsDisposed = true;
        }

        private Exception ValidateContainerName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return new ArgumentNullException("containerName", "A container name must be provided.");

            // Containers cannot begin with ~ as we used this internally on some platforms.
            if (name[0] == '~')
                return new ArgumentNullException("containerName", "A container name cannot begin with ~.");

            // Container names cannot include paths.
            if (name.Contains('\\') || name.Contains('/'))
                return new ArgumentNullException("containerName", "A container name cannot contain a path separator.");

            // Container names cannot be special reserved names.
            if (name == "." || name == "..")
                return new ArgumentNullException("containerName", "A container name cannot be a reserved identifier '.' or '..'.");

            // Don't allow strings outside the ascii range as not all platforms can support it.
            if (name.All(c => c <= 0x7F) == false)
                return new ArgumentNullException("containerName", "A container name must contain only ascii characters.");

            return null;
        }

        /// <summary>
        /// Deletes the named container and all its content if it exists.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public void DeleteContainer(string containerName)
        {
            var container = GetActiveContainer();
            if (container != null)
                throw new NotSupportedException("You cannot access multiple containers at once.");

            var exception = ValidateContainerName(containerName);
            if (exception != null)
                throw exception;

            PlatformDeleteContainer(containerName);
        }

        /// <summary>
        /// Opens an existing container or creates a new one.
        /// </summary>
        /// <param name="containerName">The name of the container.</param>
        /// <param name="requiredFreeBytes">On container creation we check for this available space or return null.</param>
        /// <returns>The container or null if it could not be created.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public StorageContainer OpenContainer(string containerName, long requiredFreeBytes)
        {
            // TODO: Should containerName be "displayName" like in old XNA?

            var exception = ValidateContainerName(containerName);
            if (exception != null)
                throw exception;

            if (requiredFreeBytes <= 0)
                throw new ArgumentOutOfRangeException("requiredFreeBytes", "Must be greater than 0.");

            var container = GetActiveContainer();
            if (container != null)
                throw new NotSupportedException("You cannot access multiple containers at once.");

            try
            {
                container = PlatformOpenContainer(containerName, requiredFreeBytes);
                if (container == null)
                {
                    Debug.WriteLine("Failed to open storage container: {0}", containerName);
                    return null;
                }

                _current = new WeakReference<StorageContainer>(container);

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

#endif
