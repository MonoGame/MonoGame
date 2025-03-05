using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Storage
{
    public partial class StorageContainer
    {
        internal string _storagePath;

        private void PlatformCreateDirectory(string directoryName)
        {
            // relative so combine with our path
            var dirPath = Path.Combine(_storagePath, directoryName);

            // Create the "directory" if need be
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
        }

        private Stream PlatformCreateFile(string fileName)
        {
            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, fileName);

            // return A new file with read/write access.
            return File.Create(filePath);
        }

        private void PlatformDeleteDirectory(string directory)
        {
            // relative so combine with our path
            var dirPath = Path.Combine(_storagePath, directory);

            // Now let's try to delete itd
            Directory.Delete(dirPath);
        }

        private void PlatformDeleteFile(string file)
        {
            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, file);

            // Now let's delete it if it exists
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private bool PlatformDirectoryExists(string directory)
        {
            // relative so combine with our path
            var dirPath = Path.Combine(_storagePath, directory);

            return Directory.Exists(dirPath);
        }

        private bool PlatformFileExists(string file)
        {
            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, file);

            // return A boolean relating to the file's existence
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
            var savedGames = StorageDevice.StorageRoot;

            _storagePath = Path.Combine(savedGames, _containerName);

            // If we have a PlayerIndex use that, otherwise save to AllPlayers folder
            _storagePath = _playerIndex.HasValue ? Path.Combine(_storagePath, "Player" + (int)_playerIndex.Value) : Path.Combine(_storagePath, "AllPlayers");
        }

        private Stream PlatformOpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            // relative so combine with our path
            var filePath = Path.Combine(_storagePath, file);
            return File.Open(filePath, fileMode, fileAccess, fileShare);
        }
    }
}