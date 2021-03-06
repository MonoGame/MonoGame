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

            InstallApp(_localAppPath, _systemAppPath);
        }

        public static void Unassociate()
        {
            if (Directory.Exists(_systemAppPath))
                Directory.Delete(_systemAppPath, true);
        }

        private static void InstallApp(string sourceDirName, string destDirName)
        {
            var dir = new DirectoryInfo(sourceDirName);

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            foreach (var file in dir.GetFiles())
            {
                // We change the name of the .app file so we also need to change the names of related files inside the bundle
                var fileName = file.Name.Replace(Path.GetFileNameWithoutExtension(_localAppPath), Path.GetFileNameWithoutExtension(_systemAppPath));
                file.CopyTo(Path.Combine(destDirName, fileName), false);
            }

            foreach (var subdir in dir.GetDirectories())
                InstallApp(subdir.FullName, Path.Combine(destDirName, subdir.Name));
        }
    }
}
