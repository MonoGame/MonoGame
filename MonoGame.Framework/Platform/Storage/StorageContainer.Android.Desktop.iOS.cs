using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Xna.Framework.Storage
{
    public partial class StorageContainer
    {
        internal string _storagePath;
        internal const string SAVE_DATA_FILENAME = "save.data";

        private void PlatformCreateDirectory(string directoryName)
        {
            lock (_processingLock)
            {
                if (!_directories.Contains(directoryName))
                {
                    _directories.Add(directoryName);
                }
            }
        }

        private void PlatformDeleteDirectory(string directoryName)
        {
            lock (_processingLock)
            {
                _directories.Remove(directoryName);
            }
        }

        private bool PlatformDirectoryExists(string directoryName)
        {
            lock (_processingLock)
            {
                return _directories.Contains(directoryName);
            }
        }

        private string[] PlatformGetDirectoryNames()
        {
            lock (_processingLock)
            {
                return _directories.ToArray();
            }
        }

        private string[] PlatformGetDirectoryNames(string searchPattern)
        {
            lock (_processingLock)
            {
                if (string.IsNullOrEmpty(searchPattern) || searchPattern == "*")
                    return _directories.ToArray();
                var pattern = searchPattern.Replace("*", "");
                return _directories.Where(d => d.Contains(pattern)).ToArray();
            }
        }

        private string[] PlatformGetFileNames()
        {
            lock (_processingLock)
            {
                // Return all file names (relative, not full path)
                return _containers.Keys.Select(f => Path.GetFileName(f)).Distinct().ToArray();
            }
        }

        private string[] PlatformGetFileNames(string searchPattern)
        {
            lock (_processingLock)
            {
                if (string.IsNullOrEmpty(searchPattern) || searchPattern == "*")
                    return _containers.Keys.Select(f => Path.GetFileName(f)).Distinct().ToArray();
                var pattern = searchPattern.Replace("*", "");
                return _containers.Keys.Select(f => Path.GetFileName(f))
                    .Where(name => name.Contains(pattern))
                    .Distinct()
                    .ToArray();
            }
        }

        private Stream PlatformOpenFile(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            // Use relative fileName as key
            lock (_processingLock)
            {
                byte[] data = null;
                _containers.TryGetValue(fileName, out data);

                switch (fileMode)
                {
                    case FileMode.Create:
                    case FileMode.CreateNew:
                    case FileMode.Truncate:
                        // Always create a new empty file
                        data = new byte[0];
                        break;
                    case FileMode.OpenOrCreate:
                        if (data == null)
                            data = new byte[0];
                        break;
                    case FileMode.Open:
                    case FileMode.Append:
                        if (data == null)
                            throw new FileNotFoundException($"File '{fileName}' not found in container.");
                        break;
                }

                if (fileAccess == FileAccess.Read)
                {
                    // Read-only: use buffer constructor
                    return new CallbackStream(new MemoryStream(data ?? new byte[0]), _ => { });
                }
                else
                {
                    // Write or ReadWrite: use expandable stream
                    var ms = new MemoryStream();
                    if (fileMode == FileMode.Append && data != null && data.Length > 0)
                    {
                        ms.Write(data, 0, data.Length);
                        ms.Seek(0, SeekOrigin.End);
                    }
                    return new CallbackStream(ms, updatedData =>
                    {
                        _containers[fileName] = updatedData;
                        if (!_isContainerDirty.Contains(_containerName))
                            _isContainerDirty.Add(_containerName);
                    });
                }
            }
        }

        private byte[] PlatformReadContainer(bool mount) // mount variable used in other platforms
        {
            try
            {
                var savePath = Path.Combine(_storagePath, SAVE_DATA_FILENAME);
                if (File.Exists(savePath))
                {
                    // Read the file directly from disk
                    return File.ReadAllBytes(savePath);
                }
                else
                {
                    Console.WriteLine("File: {0}, does NOT exist.", savePath);
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log or handle unexpected errors
                Console.WriteLine("Error reading {0}. Error: {1}", Path.Combine(_storagePath, SAVE_DATA_FILENAME), ex.Message);
                return null;
            }
        }

        private void PlatformWriteContainer(byte[] data, bool mount) // mount variable used in other platforms
        {
            if (data == null || data.Length == 0)
                return;

            // Ensure the physical directory exists
            if (!Directory.Exists(_storagePath))
                Directory.CreateDirectory(_storagePath);

            // Write the data directly to the physical file (overwrite if exists)
            var savePath = Path.Combine(_storagePath, SAVE_DATA_FILENAME);
            File.WriteAllBytes(savePath, data);
        }

        public static string PlatformSanitizeFileName(string fileName)
        {
            var truncatedFileName = TruncateFileName(fileName);

            // Replace invalid filename characters with underscores
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitizedFileName = string.Concat(truncatedFileName.Select(c => invalidChars.Contains(c) ? '_' : c));

            // Optionally trim trailing periods/spaces (common issue in Windows)
            return sanitizedFileName.Trim().TrimEnd('.');
        }

        private const int MAXFILENAMELENGTH = 240;

        public static string TruncateFileName(string fileName)
        {
            if (fileName.Length > MAXFILENAMELENGTH)
            {
                return fileName.Substring(0, MAXFILENAMELENGTH);
            }
            return fileName;
        }

        private Stream PlatformCreateFile(string fileName)
        {
            lock (_processingLock)
            {
                _containers[fileName] = new byte[0];
            }
            var ms = new MemoryStream();
            ms.Position = 0;
            ms.SetLength(0);
            return new CallbackStream(ms, data =>
            {
                lock (_processingLock)
                {
                    _containers[fileName] = data;
                    if (!_isContainerDirty.Contains(_containerName))
                        _isContainerDirty.Add(_containerName);
                }
            });
        }

        private void PlatformDeleteFile(string fileName)
        {
            lock (_processingLock)
            {
                _containers.Remove(fileName);
            }
        }

        private bool PlatformFileExists(string fileName)
        {
            lock (_processingLock)
            {
                return _containers != null && _containers.ContainsKey(fileName);
            }
        }

        private void PlatformInitialize()
        {
            var savedGames = Path.Combine(StorageDevice.StorageRoot, "SavedGames");
            _storagePath = Path.Combine(savedGames, _containerName);
            _storagePath = _playerIndex.HasValue ? Path.Combine(_storagePath, "Player" + (int)_playerIndex.Value) : Path.Combine(_storagePath, "AllPlayers");
        }
    }

    // Helper class for stream with callback on close/dispose
    internal class CallbackStream : Stream
    {
        private readonly MemoryStream _inner;
        private readonly Action<byte[]> _onClose;
        private bool _disposed;

        public CallbackStream(MemoryStream inner, Action<byte[]> onClose)
        {
            _inner = inner;
            _onClose = onClose;
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position { get => _inner.Position; set => _inner.Position = value; }
        public override void Flush() => _inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
        public override int ReadByte() => _inner.ReadByte();
        public override void WriteByte(byte value) => _inner.WriteByte(value);
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _onClose(_inner.ToArray());
                _inner.Dispose();
                _disposed = true;
            }
            base.Dispose(disposing);
        }
    }
}