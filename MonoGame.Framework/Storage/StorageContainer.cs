// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
    //	User storage is in the My Documents folder of the user who is currently logged in, in the SavedGames folder. 
    //	A subfolder is created for each game according to the titleName passed to the BeginOpenContainer method. 
    //	When no PlayerIndex is specified, content is saved in the AllPlayers folder. When a PlayerIndex is specified, 
    //	the content is saved in the Player1, Player2, Player3, or Player4 folder, depending on which PlayerIndex 
    //	was passed to BeginShowSelector.

    /// <summary>
    /// Contains a logical collection of files used for user-data storage.
    /// </summary>			
    /// <remarks>MSDN documentation contains related conceptual article: https://learn.microsoft.com/en-us/previous-versions/windows/xna/bb199074(v=xnagamestudio.40)</remarks>
    partial class StorageContainer : IDisposable
    {
        internal readonly string _storagePath;
        private readonly PlayerIndex? _playerIndex;

        /// <summary>
        /// Gets a bool value indicating whether the instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        private readonly string _displayName;
        /// <summary>
        /// Returns display name of the title.
        /// </summary>
        public string DisplayName
        {
            get { return _displayName; }
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
        /// <param name='displayName'> name.</param>
        /// <param name='playerIndex'>The <see cref="PlayerIndex"/> of the player to save the data.</param>
        internal StorageContainer(StorageDevice device, string displayName, PlayerIndex? playerIndex)
        {
            if (string.IsNullOrEmpty(_displayName))
                throw new ArgumentNullException("A title name has to be provided in parameter name.");

            _storageDevice = device;
            _displayName = displayName;
            _playerIndex = playerIndex;

            // TODO Do we want to be consistent and put everything under SavedGames across all platforms?
#if DESKTOPGL 
            // We already have a SaveData folder on Desktop.
            var savedGames = StorageDevice.StorageRoot;
#else
            var storageRoot = StorageDevice.StorageRoot;
            var savedGames = Path.Combine(storageRoot, "SavedGames");
#endif
            _storagePath = Path.Combine(savedGames, _displayName);

            // If we have a PlayerIndex use that, otherwise save to AllPlayers folder
            _storagePath = _playerIndex.HasValue ? Path.Combine(_storagePath, "Player" + (int)_playerIndex.Value) : Path.Combine(_storagePath, "AllPlayers");

            // Create the "device" if need be
            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);
        }

        /// <summary>
        /// Creates a new directory in the storage-container.
        /// </summary>
        /// <param name="directory">Relative path of the directory to be created.</param>
		public void CreateDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("Parameter directory must contain a value.");

            // relative so combine with our path
            var dirPath = Path.Combine(_storagePath, directory);

            // Now let's try to create it
            Directory.CreateDirectory(dirPath);
        }

        /// <summary>
        /// Creates a file in the storage-container.
        /// </summary>
        /// <param name="file">Relative path of the file to be created.</param>
        /// <returns>Returns <see cref="Stream"/> for the created file.</returns>
        public Stream CreateFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("Parameter file must contain a value.");

            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, file);

            // return A new file with read/write access.
            return File.Create(filePath);
        }

        /// <summary>
        /// Deletes specified directory for the storage-container.
        /// </summary>
        /// <param name="directory">The relative path of the directory to be deleted.</param>
        public void DeleteDirectory(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("Parameter directory must contain a value.");

            // relative so combine with our path
            var dirPath = Path.Combine(_storagePath, directory);

            // Now let's try to delete itd
            Directory.Delete(dirPath);
        }

        /// <summary>
        /// Deletes a file from the storage-container.
        /// </summary>
        /// <param name="file">The relative path of the file to be deleted.</param>
        public void DeleteFile(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("Parameter file must contain a value.");

            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, file);

            // Now let's try to delete it
            File.Delete(filePath);
        }


        /// <summary>
        /// Returns true if specified path exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="directory">The relative path of directory to query for.</param>
        /// <returns>True if queried directory exists, false otherwise.</returns>
        public bool DirectoryExists(string directory)
        {
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException("Parameter directory must contain a value.");

            // relative so combine with our path
            var dirPath = Path.Combine(_storagePath, directory);

            return Directory.Exists(dirPath);
        }

        /// <summary>
        /// Disposes un-managed objects referenced by this object.
        /// </summary>
        public void Dispose()
        {
            Disposing?.Invoke(this, null);

            IsDisposed = true;
        }

        /// <summary>
        /// Returns true if the specified file exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="file">The relative path of file to query for.</param>
        /// <returns>True if queried file exists, false otherwise.</returns>
        public bool FileExists(string file)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("Parameter file must contain a value.");

            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, file);

            // return A new file with read/write access.
            return File.Exists(filePath);
        }

        /// <summary>
        /// Returns list of directory names in the storage-container.
        /// </summary>
        /// <returns>List of directory names.</returns>
        public string[] GetDirectoryNames()
        {
            return Directory.GetDirectories(_storagePath);
        }

        /// <summary>
        /// Returns list of directory names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched directory names.</returns>
        public string[] GetDirectoryNames(string searchPattern)
        {
            return Directory.GetDirectories(_storagePath, searchPattern);
        }

        /// <summary>
        /// Returns list of file names in the storage-container.
        /// </summary>
        /// <returns>List of file names.</returns>
        public string[] GetFileNames()
        {
            return Directory.GetFiles(_storagePath);
        }

        /// <summary>
        /// Returns list of file names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched file names.</returns>
        public string[] GetFileNames(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException("Parameter searchPattern must contain a value.");

            return Directory.GetFiles(_storagePath, searchPattern);
        }


        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="file">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string file, FileMode fileMode)
        {
            return OpenFile(file, fileMode, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="file">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <param name="fileAccess"><see cref="FileAccess"/> that specifies access mode.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess)
        {
            return OpenFile(file, fileMode, fileAccess, FileShare.ReadWrite);
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="file">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <param name="fileAccess"><see cref="FileAccess"/> that specifies access mode.</param>
        /// <param name="fileShare">A bitwise combination of <see cref="FileShare"/> enumeration values that specifies access modes for other stream objects.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            if (string.IsNullOrEmpty(file))
                throw new ArgumentNullException("Parameter file must contain a value.");

            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, file);
            return File.Open(filePath, fileMode, fileAccess, fileShare);
        }
    }
}