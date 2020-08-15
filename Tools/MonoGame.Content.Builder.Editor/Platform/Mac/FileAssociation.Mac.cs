using System;
using System.Diagnostics;
using System.IO;

namespace MonoGame.Tools.Pipeline
{
    public static class FileAssociation
    {
        private static readonly string _localAppPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "../.."));
        private static readonly string _systemAppPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Applications/MGCB Editor.app");

        public static void Associate()
        {
            if (Path.GetExtension(_localAppPath) != ".app")
                throw new FileNotFoundException("Not running from within the app package");

            CopyDirectory(_localAppPath, _systemAppPath);
        }

        public static void Unassociate()
        {
            if (Directory.Exists(_systemAppPath))
                Directory.Delete(_systemAppPath, true);
        }

        private static void CopyDirectory(string sourceDirName, string destDirName)
        {
            var dir = new DirectoryInfo(sourceDirName);

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            foreach (var file in dir.GetFiles())
                file.CopyTo(Path.Combine(destDirName, file.Name), false);

            foreach (var subdir in dir.GetDirectories())
                CopyDirectory(subdir.FullName, Path.Combine(destDirName, subdir.Name));
        }
    }
}
