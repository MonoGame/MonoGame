using System;
using System.IO;

namespace Microsoft.Xna.Framework.Storage
{
    public partial class StorageContiner
    {
        private void PlatformInitialize()
        {
            var storageRoot = StorageDevice.StorageRoot;
            return Path.Combine(storageRoot, "SavedGames");
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
    }
}
