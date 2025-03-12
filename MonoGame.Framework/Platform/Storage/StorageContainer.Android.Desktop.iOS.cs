using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
    public partial class StorageContainer
    {
        internal string _storagePath;

        private void PlatformCreateDirectory(string directoryName)
        {
            // relative so combine with our path
            var directoryPath = Path.Combine(_storagePath, directoryName);

            // Create the "directory" if need be
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private Stream PlatformCreateFile(string fileName)
        {
            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, fileName);

            // return A new file with read/write access.
            return File.Create(filePath);
        }

        private void PlatformDeleteDirectory(string directoryName)
        {
            // relative so combine with our path
            var directoryPath = Path.Combine(_storagePath, directoryName);

            // Now let's try to delete itd
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath);
            }
        }

        private void PlatformDeleteFile(string fileName)
        {
            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private bool PlatformDirectoryExists(string directoryName)
        {
            // relative so combine with our path
            var directoryPath = Path.Combine(_storagePath, directoryName);

            return Directory.Exists(directoryPath);
        }

        private bool PlatformFileExists(string fileName)
        {
            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, fileName);

            return File.Exists(filePath);
        }

        private string[] PlatformGetDirectoryNames()
        {
            return Directory.GetDirectories(_storagePath);
        }

        private string[] PlatformGetDirectoryNames(string searchPattern)
        {
            return Directory.GetDirectories(_storagePath, searchPattern);
        }

        private string[] PlatformGetFileNames()
        {
            return Directory.GetFiles(_storagePath);
        }

        private string[] PlatformGetFileNames(string searchPattern)
        {
            return Directory.GetFiles(_storagePath, searchPattern);
        }

        private void PlatformInitialize()
        {
            var savedGames = Path.Combine(StorageDevice.StorageRoot, "SavedGames");

            _storagePath = Path.Combine(savedGames, _containerName);

            // If we have a PlayerIndex use that, otherwise save to AllPlayers folder
            _storagePath = _playerIndex.HasValue ? Path.Combine(_storagePath, "Player" + (int)_playerIndex.Value) : Path.Combine(_storagePath, "AllPlayers");

            // Create the "directory" if need be
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
        }

        private Stream PlatformOpenFile(string fileName, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, fileName);

            return File.Open(filePath, fileMode, fileAccess, fileShare);
        }

        private byte[] PlatformReadContainers(bool value)
        {
            throw new NotImplementedException();
        }

        private void PlatformWriteContainers(byte[] data, bool value)
        {
            throw new NotImplementedException();
        }
    }
}