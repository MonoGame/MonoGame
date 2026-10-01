// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

#if NATIVE

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
    /// <summary>
    /// A named container for saving user data on the platform's local storage.
    /// </summary>
    /// <remarks>
    /// This is class is in preview and not final.
    /// </remarks>
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
        /// Returns true if the instance has been disposed.
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
        /// Returns the <see cref="StorageDevice"/> this container was created from.
        /// </summary>
        public StorageDevice StorageDevice
        {
            get
            {
                return _device;
            }
        }

        internal StorageContainer(StorageDevice device, string containerName, PlayerIndex? playerIndex)
        {
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
        /// Creates a new directory.
        /// </summary>
        /// <param name="directoryName">Relative path of the directory to be created.</param>
        /// <remarks>You must call commit to flush this to storage.</remarks>
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
        /// Creates a new file or appends to an existing one.
        /// </summary>
        /// <param name="fileName">Relative path of the file to be created.</param>
        /// <param name="truncate">If the file exists set it to 0 bytes otherwise append.</param>
        /// <returns>Returns <see cref="Stream"/> for the created file.</returns>
        /// <remarks>You must call commit to flush this to storage.</remarks>
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
        /// Deletes a directory and all the content it contains.
        /// </summary>
        /// <param name="directoryName">The relative path of the directory to be deleted.</param>
        /// <remarks>You must call commit to flush this to storage.</remarks>
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
        /// Deletes a file if it exists.
        /// </summary>
        /// <param name="fileName">The relative path of the file to be deleted.</param>
        /// <remarks>You must call commit to flush this to storage.</remarks>
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
        /// Returns true if the directory exists.
        /// </summary>
        /// <param name="directoryName">The relative path to the directory.</param>
        /// <returns>True if the directory exists.</returns>
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
        /// Returns true if the file exists.
        /// </summary>
        /// <param name="fileName">The relative path to the file.</param>
        /// <returns>True if the file exists.</returns>
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
        /// Returns list of all directories in the container.
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

		private unsafe bool MatchWildcards(char *match, char* file)
        {
            // TODO: Maybe re-write this to not need unsafe code
            // and to not recurse on itself!

			// Early out on a match all.
			if (match[0] == '*' && match[1] == 0 && file[0] != 0)
				return true;

			while (match[0] != 0)
			{
				if (match[0] == '*')
				{
					if (MatchWildcards(match + 1, file))
						return true;

					if (file[0] != 0 && MatchWildcards(match, file + 1))
						return true;

					return false;
				}
					
				if (match[0] == '?')
				{
					if (file[0] == 0)
						return false;

					++file;
					++match;
					continue;
				}

				if (char.ToLowerInvariant(match[0]) != char.ToLowerInvariant(file[0]))
					return false;

				++match;
				++file;
			}

			// If we didn't get the end of both we didn't match.
			return file[0] != 0 && match[0] != 0;
		}

        /// <summary>
        /// Returns list of all directories that match the search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of relative directory paths.</returns>
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

                var name = pair.Key.TrimEnd('/');

                unsafe
                {
                    fixed (char* match = searchPattern)
                    fixed (char* key = name)
                        if (MatchWildcards(match, key))
                            dirs.Add(name);
                }
            }

            return dirs.ToArray();
        }

        /// <summary>
        /// Returns list of all files in the container.
        /// </summary>
        /// <returns>List of relative file paths.</returns>
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
        /// Returns list of all file paths with given search pattern.
        /// </summary>
        /// <param name="searchPattern">A search pattern that supports single-character ("?") and multicharacter ("*") wildcards.</param>
        /// <returns>List of relative file paths.</returns>
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

                var name = pair.Key;

                unsafe
                {
                    fixed (char* match = searchPattern)
                    fixed (char* key = name)
                        if (MatchWildcards(match, key))
                            files.Add(name);
                }
            }

            return files.ToArray();
        }

        /// <summary>
        /// Opens a file.
        /// </summary>
        /// <param name="fileName">Relative path to the file.</param>
        /// <param name="fileMode"><see cref="FileMode"/> that specifies how the file is opened.</param>
        /// <returns><see cref="Stream"/> object for the opened file or null.</returns>
        /// <remarks>You must call commit to flush this to storage if writing to the file.</remarks>
        public Stream OpenFile(string fileName, FileMode fileMode)
        {
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
        /// The container can then be reused for future operations or disposed.
        /// </summary>
        /// <remarks>
        /// This call guarantees to not partially write data and corrupt your saves.
        /// If the game shuts down during commit the system will try to recover
        /// next time the container is opened.
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
        /// Frees resources held by the container.
        /// </summary>
        /// <remarks>This does not flush changes to local storage.</remarks>
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

#endif 
