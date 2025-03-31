using System;
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

            // Now let's try to delete it
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

        private byte[] PlatformReadContainers(bool mount) // mount variable used in other platforms
        {
            try
            {
                if (PlatformFileExists(SAVE_DATA_FILENAME))
                {
                    // Open the file in read-only mode with shared access for reading
                    using (var stream = PlatformOpenFile(SAVE_DATA_FILENAME, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        if (stream.Length == 0)
                            return null; // Handle empty file case gracefully

                        // Read entire stream into a byte array
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                }
                else
                {
                    Console.WriteLine("File: {0}, does NOT exist.", Path.Combine(_storagePath, SAVE_DATA_FILENAME));
                    return null;
                }
            }
            catch (Exception ex)
            {
                // Log or handle unexpected errors
                Console.WriteLine("Error reading {0}. Error: {ex.Message}", Path.Combine(_storagePath, SAVE_DATA_FILENAME));
                return null;
            }
        }

        private void PlatformWriteContainers(byte[] data, bool mount) // mount variable used in other platforms
        {
            if (data == null || data.Length == 0)
                return;

            // If the file exists, delete it to ensure clean data
            PlatformDeleteFile(SAVE_DATA_FILENAME);

            // Create a new file and write the data
            using (var fileStream = PlatformCreateFile(SAVE_DATA_FILENAME))
            {
                fileStream.Write(data, 0, data.Length);
            }
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
    }
}