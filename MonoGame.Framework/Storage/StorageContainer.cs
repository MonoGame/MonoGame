// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Xna.Framework.Storage
{
    // NOTE: This is the original design for Windows support for Storage from XNA 4:
    //
    //	User storage is usually in the "My Documents" folder of the user who is currently logged in, in the SavedGames folder.
    //	A subfolder is created for each game according to the titleName passed to the OpenContainer method.
    //	When no PlayerIndex is specified, content is saved in the AllPlayers folder. When a PlayerIndex is specified,
    //	the content is saved in the Player1, Player2, Player3, or Player4 folder, depending on which PlayerIndex
    //	was passed to BeginShowSelector.
    //

    /// <summary>
    /// Contains a logical collection of files used for user-data storage.
    /// </summary>			
    /// <remarks>MSDN documentation contains related conceptual article: https://learn.microsoft.com/en-us/previous-versions/windows/xna/bb199074(v=xnagamestudio.40)</remarks>
    public partial class StorageContainer : IDisposable
    {
        private readonly StorageDevice _device;
        private readonly PlayerIndex? _playerIndex;
        private readonly string _containerName;

        class Blob
        {
            public MemoryStream content;
            public bool directory;
            public bool deleted;
            public bool dirty;
        }

        private Dictionary<string, Blob> _cache;

        /// <summary>
        /// Gets a bool value indicating whether the instance has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Returns container identifier name.
        /// </summary>
        public string ContainerName
        {
            get
            {
                return _containerName;
            }
        }

        /// <summary>
        /// Returns the <see cref="StorageDevice"/> that holds logical files for the container.
        /// </summary>
        public StorageDevice StorageDevice
        {
            get
            {
                return _device;
            }
        }

        /// <summary>
        /// Fired when <see cref="Dispose"/> is called or object if finalized or collected by the garbage collector.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Initializes a new instance of the <see cref="StorageContainer"/> class.
        /// </summary>
        /// <param name='device'>The attached storage-device.</param>
        /// <param name='containerName'> The identifier for the container.</param>
        /// <param name='playerIndex'>The <see cref="PlayerIndex"/> of the player to save the data.</param>
        internal StorageContainer(StorageDevice device, string containerName, PlayerIndex? playerIndex)
        {
            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException("containerName", "A title name must be provided.");

            _device = device;
            _containerName = containerName;
            _playerIndex = playerIndex;
        }

        private static string SanitizeDirPath(string path)
        {
            path = SanitizeFilePath(path);
            if (!path.EndsWith('/'))
                path += "/";
            return path;
        }

        private static string SanitizeFilePath(string path)
        {
            path = path.Replace('\\', '/');
            path = path.ToLower();
            return path;
        }

        /// <summary>
        /// Creates a new directory in the storage-container.
        /// </summary>
        /// <param name="directoryName">Relative path of the directory to be created.</param>
        public void CreateDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName", "A directory name must be provided.");

            var path = SanitizeDirPath(directoryName);

            if (_cache == null)
                PlatformUpdateCache();

            // TODO: If the directory is in a subfolder folder, should
            // i throw if the parent directory has not been created?

            if (!_cache.TryGetValue(path, out var data))
            {
                data = new Blob();
                data.directory = true;
                data.dirty = true;
                _cache[path] = data;
            }
            else
            {
                if (data.deleted)
                    data.deleted = false;
            }
        }

        /// <summary>
        /// Creates a file in the storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file to be created.</param>
        /// <param name="truncate">If the file exists set it to 0 bytes otherwise append.</param>
        /// <returns>Returns <see cref="Stream"/> for the created file.</returns>
        public Stream CreateFile(string fileName, bool truncate = true)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "A file name must be provided.");

            var path = SanitizeFilePath(fileName);

            if (_cache == null)
                PlatformUpdateCache();

            // TODO: If the file is in a folder, should
            // i throw if the directory has not been created?

            if (!_cache.TryGetValue(path, out var blob))
            {
                blob = new Blob();
                blob.content = new MemoryStream();
                _cache[path] = blob;
            }
            else
            {
                if (blob.deleted)
                    blob.deleted = false;

                if (truncate)
                {
                    if (blob.content == null)
                        blob.content = new MemoryStream();
                    else
                    {
                        // Reset the stream.
                        blob.content.Position = 0;
                        blob.content.SetLength(0);
                    }
                }
                else
                {
                    // We're appending... so load the file from disk first.
                    blob.content = PlatformLoadFile(path);
                    blob.content.Position = blob.content.Length;
                }
            }

            blob.dirty = true;

            return new StorageStream(blob.content, true, true);
        }

        /// <summary>
        /// Deletes specified directory for the storage-container.
        /// </summary>
        /// <param name="directoryName">The relative path of the directory to be deleted.</param>
        public void DeleteDirectory(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName", "A directory name must be provided.");

            var path = SanitizeDirPath(directoryName);

            if (_cache == null)
                PlatformUpdateCache();

            if (!_cache.TryGetValue(path, out var blob))
                return;

            blob.deleted = true;

            // Remove all the files and directories that are
            // contained within this directory.
            foreach (var key in _cache.Keys.ToList())
            {
                if (key.StartsWith(path) && path.Length != key.Length)
                    _cache.Remove(key);
            }
            
            blob.dirty = true;
        }

        /// <summary>
        /// Deletes a file from the storage-container.
        /// </summary>
        /// <param name="fileName">The relative path of the file to be deleted.</param>
        public void DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "A file name must be provided.");

            var path = SanitizeFilePath(fileName);

            if (_cache == null)
                PlatformUpdateCache();

            if (!_cache.TryGetValue(path, out var blob))
                return;

            blob.content = null;
            blob.deleted = true;
            blob.dirty = true;
        }

        /// <summary>
        /// Returns true if specified path exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="directoryName">The relative path of directory to query for.</param>
        /// <returns>True if queried directory exists, false otherwise.</returns>
        public bool DirectoryExists(string directoryName)
        {
            if (string.IsNullOrEmpty(directoryName))
                throw new ArgumentNullException("directoryName", "A directory name must be provided.");

            var path = SanitizeDirPath(directoryName);

            if (_cache == null)
                PlatformUpdateCache();

            return _cache.TryGetValue(path, out var blob) && !blob.deleted;
        }

        /// <summary>
        /// Returns true if the specified file exists in the storage-container, false otherwise.
        /// </summary>
        /// <param name="fileName">The relative path of file to query for.</param>
        /// <returns>True if queried file exists, false otherwise.</returns>
        public bool FileExists(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "A file name must be provided.");

            var path = SanitizeFilePath(fileName);

            if (_cache == null)
                PlatformUpdateCache();

            return _cache.TryGetValue(path, out var blob) && !blob.deleted;
        }

        /// <summary>
        /// Returns list of directory names in the storage-container.
        /// </summary>
        /// <returns>List of directory names.</returns>
        public string[] GetDirectoryNames()
        {
            var dirs = new List<string>();

            if (_cache == null)
                PlatformUpdateCache();

            foreach (var pair in _cache)
            {
                if (!pair.Value.directory)
                    continue;
                if (pair.Value.deleted)
                    continue;

                dirs.Add(pair.Key.TrimEnd('/'));
            }

            return dirs.ToArray();
        }

        /// <summary>
        /// Returns list of directory names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched directory names.</returns>
        public string[] GetDirectoryNames(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException("searchPattern", "A search pattern must be provided.");

            if (_cache == null)
                PlatformUpdateCache();

            var dirs = new List<string>();

            foreach (var pair in _cache)
            {
                if (!pair.Value.directory)
                    continue;
                if (pair.Value.deleted)
                    continue;

                // TODO: How do i simply do the search pattern matching?
                //dirs.Add(pair.Key);
            }

            return dirs.ToArray();
        }

        /// <summary>
        /// Returns list of file names in the storage-container.
        /// </summary>
        /// <returns>List of file names.</returns>
        public string[] GetFileNames()
        {
            if (_cache == null)
                PlatformUpdateCache();

            var files = new List<string>();

            foreach (var pair in _cache)
            {
                if (pair.Value.directory)
                    continue;
                if (pair.Value.deleted)
                    continue;

                files.Add(pair.Key);
            }

            return files.ToArray();
        }

        /// <summary>
        /// Returns list of file names with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of matched file names.</returns>
        public string[] GetFileNames(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
                throw new ArgumentNullException("searchPattern", "A search pattern must be provided.");

            if (_cache == null)
                PlatformUpdateCache();

            var files = new List<string>();

            foreach (var pair in _cache)
            {
                if (pair.Value.directory)
                    continue;
                if (pair.Value.deleted)
                    continue;

                // TODO: How do i simply do the search pattern matching?
                //files.Add(pair.Key);
            }

            return files.ToArray();
        }

        /// <summary>
        /// Opens a file contained in storage-container.
        /// </summary>
        /// <param name="fileName">Relative path of the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <returns><see cref="Stream"/> object for the opened file.</returns>
        public Stream OpenFile(string fileName, FileMode fileMode)
        {
            // TODO: Need a custom file stream object that handles
            // detecting changes to files on write.  Also need to enforce
            // read only streams.

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName", "A file name must be provided.");

            var path = SanitizeFilePath(fileName);

            if (_cache == null)
                PlatformUpdateCache();

            bool exists = _cache.TryGetValue(path, out var blob) && blob.deleted == false;

            if (fileMode == FileMode.CreateNew)
            {
                if (exists)
                    throw new IOException(); // This seems silly.

                return CreateFile(fileName, true);
            }

            if (fileMode == FileMode.Create)
                return CreateFile(fileName, true);

            if (fileMode == FileMode.OpenOrCreate)
            {
                if (exists)
                {
                    blob.content.Position = 0;
                    return new StorageStream(blob.content, true, true);
                }

                return CreateFile(fileName, false);
            }

            if (fileMode == FileMode.Truncate)
            {
                if (!exists)
                    throw new FileNotFoundException();

                blob.content.Position = 0;
                blob.content.SetLength(0);
                return new StorageStream(blob.content, true, true);
            }

            if (fileMode == FileMode.Append)
            {
                if (!exists)
                    throw new FileNotFoundException();
                return new StorageStream(blob.content, true, true);
            }

            if (!exists)
                throw new FileNotFoundException();

            // Load the file from disk if we haven't before.
            if (blob.content == null)
                blob.content = PlatformLoadFile(path);

            blob.content.Position = 0;
            return new StorageStream(blob.content, true, true);
        }

        /// <summary>
        /// Flushes the changes made to the container to storage.
        /// </summary>
        /// <remarks>
        /// This call guarantees to not partially write data and corrupt your saves.
        /// </remarks>
        public void Commit()
        {
            if (_cache != null)
                PlatformCommit();
        }        


        ~StorageContainer()
        {
            PlatformDispose();
        }

        /// <summary>
        /// Frees allocations made by the container without applying pending storage operations.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                PlatformDispose();
                IsDisposed = true;
            }
        }
    }
}
