// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
    //	User storage is usually in the "My Documents" folder of the user who is currently logged in, in the SavedGames folder.
    //	A subfolder is created for each game according to the titleName passed to the OpenContainer method.
    //	When no PlayerIndex is specified, content is saved in the AllPlayers folder. When a PlayerIndex is specified,
    //	the content is saved in the Player1, Player2, Player3, or Player4 folder, depending on which PlayerIndex
    //	was passed to BeginShowSelector.

    /// <summary>
    /// Contains a logical collection of files used for user-data storage.
    /// </summary>			
    /// <remarks>MSDN documentation contains related conceptual article: https://learn.microsoft.com/en-us/previous-versions/windows/xna/bb199074(v=xnagamestudio.40)</remarks>
    public partial class StorageContainer : IDisposable
    {
        private readonly PlayerIndex? _playerIndex;

        /// <summary>
        /// Gets a bool value indicating whether the instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        private readonly string _containerName;
        /// <summary>
        /// Returns container name of the title.
        /// </summary>
        public string ContainterName
        {
            get { return _containerName; }
        }

        private readonly StorageDevice _storageDevice;
        /// <summary>
        /// Returns the <see cref="StorageDevice"/> that holds logical files for the container.
        /// </summary>
		public StorageDevice StorageDevice
        {

            get { return _storageDevice; }
        }

        /// <summary>
        /// Fired when <see cref="Dispose"/> is called or object if finalized or collected by the garbage collector.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageContainer"/> class.
        /// </summary>
        /// <param name='device'>The attached storage-device.</param>
        /// <param name='containerName'> name.</param>
        /// <param name='playerIndex'>The <see cref="PlayerIndex"/> of the player to save the data.</param>
        internal StorageContainer(StorageDevice device, string containerName, PlayerIndex? playerIndex)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException(nameof(containerName), "A title name must be provided.");

            _storageDevice = device;
            _containerName = containerName;
            _playerIndex = playerIndex;

            PlatformInitialize();
        }

        /// <summary>
        /// Creates a new directory in the storage-container.
        /// </summary>
        /// <param name="directoryName">Relative path of the directory to be created.</param>
        public void CreateDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException(nameof(directoryName), "A directory name must be provided.");

            PlatformCreateDirectory(directoryName);
        }

        /// <summary>
        /// Creates a file in the storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file to be created.</param>
        /// <returns>Returns <see cref="Stream"/> for the created file.</returns>
        public Stream CreateFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName), "A file name must be provided.");

            return PlatformCreateFile(fileName);
        }

        /// <summary>
        /// Deletes specified directory for the storage-container.
        /// </summary>
        /// <param name="directory">The relative path of the directory to be deleted.</param>
        public void DeleteDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException(nameof(directoryName), "A directory name must be provided.");

            PlatformDeleteDirectory(directoryName);
        }

        /// <summary>
        /// Deletes a file from the storage-container.
        /// </summary>
        /// <param name="fileName">The relative path of the file to be deleted.</param>
        public void DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName), "A file name must be provided.");

            PlatformDeleteFile(fileName);
        }

        /// <summary>
        /// Returns true if specified path exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="directoryName">The relative path of directory to query for.</param>
        /// <returns>True if queried directory exists, false otherwise.</returns>
        public bool DirectoryExists(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException(nameof(directoryName), "A directory name must be provided.");

            return PlatformDirectoryExists(directoryName);
        }



        /// <summary>
        /// Returns true if the specified file exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="fileName">The relative path of file to query for.</param>
        /// <returns>True if queried file exists, false otherwise.</returns>
        public bool FileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName), "A file name must be provided.");

            return PlatformFileExists(fileName);

        }

        /// <summary>
        /// Returns list of directory names in the storage-container.
        /// </summary>
        /// <returns>List of directory names.</returns>
        public string[] GetDirectoryNames()
        {
            return PlatformGetDirectoryNames();
        }

        /// <summary>
        /// Returns list of directory names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched directory names.</returns>
        public string[] GetDirectoryNames(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException(nameof(searchPattern), "A search pattern must be provided.");

            return PlatformGetDirectoryNames(searchPattern);
        }

        /// <summary>
        /// Returns list of file names in the storage-container.
        /// </summary>
        /// <returns>List of file names.</returns>
        public string[] GetFileNames()
        {
            return PlatformGetFileNames();
        }

        /// <summary>
        /// Returns list of file names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched file names.</returns>
        public string[] GetFileNames(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException(nameof(searchPattern), "A search pattern must be provided.");

            return PlatformGetFileNames(searchPattern);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string fileName, FileMode fileMode)
        {
            return OpenFile(fileName, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <param name="fileAccess"><see cref="FileAccess"/> that specifies access mode.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string fileName, FileMode fileMode, FileAccess fileAccess)
        {
            return OpenFile(fileName, fileMode, fileAccess, FileShare.ReadWrite);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <param name="fileAccess"><see cref="FileAccess"/> that specifies access mode.</param>
        /// <param name="fileShare">A bitwise combination of <see cref="FileShare"/> enumeration values that specifies access modes for other stream objects.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName), "A file name must be provided.");

            return PlatformOpenFile(fileName, fileMode, fileAccess, fileShare);
        }

        /// <summary>
        /// Disposes un-managed objects referenced by this object.
        /// </summary>
        public void Dispose()
        {
            Disposing?.Invoke(this, null);

            IsDisposed = true;
        }
    }
}