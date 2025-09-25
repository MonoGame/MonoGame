using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Xna.Framework.Storage
{
    partial class StorageContainer
    {
        private string _storagePath;
        internal string StoragePath { get => _storagePath; }

        private string PlatformInitialize()
        {
            throw new NotImplementedException();
        }

        private Stream PlatformOpenFile(string file, FileMode fileMode, FileAccess fileAccess, FileShare fileShare)
        {
            throw new NotImplementedException();
        }

        private string[] PlatformGetFileNames(string searchPattern)
        {
            throw new NotImplementedException();
        }

        private string[] PlatformGetFileNames()
        {
            throw new NotImplementedException();
        }

        private string[] PlatformGetDirectoryNames(string searchPattern)
        {
            throw new NotImplementedException();
        }

        private string[] PlatformGetDirectoryNames()
        {
            throw new NotImplementedException();
        }

        private bool PlatformFileExists(string file)
        {
            throw new NotImplementedException();
        }

        private bool PlatformDirectoryExists(string directory)
        {
            throw new NotImplementedException();
        }

        private void PlatformDeleteFile(string file)
        {
            throw new NotImplementedException();
        }

        private void PlatformDeleteDirectory(string directory)
        {
            throw new NotImplementedException();
        }

        private Stream PlatformCreateFile(string fileName)
        {
            throw new NotImplementedException();
        }

        private void PlatformCreateDirectory(string directoryName)
        {
            throw new NotImplementedException();
        }

        private byte[] PlatformReadContainer(bool value)
        {
            throw new NotImplementedException();
        }

        private void PlatformWriteContainer(byte[] data, bool value)
        {
            throw new NotImplementedException();
        }

        public static string PlatformSanitizeFileName(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
