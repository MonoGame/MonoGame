using System;
using System.Diagnostics;
using System.IO;

namespace MonoGame.Tools.Pipeline
{
    public static class FileAssociation
    {
        private readonly static string appPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Applications/MGCB Editor.app");

        public static void Associate()
        {
            InstallApplication();
        }

        public static void Unassociate()
        {
            UninstallApplication();
        }

        private static void InstallApplication()
        {
            Console.WriteLine("Installing application...");

            var baseAppPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "../.."));
            if (Path.GetExtension(baseAppPath) != ".app")
            {
                throw new FileNotFoundException("Not running from within the app package");
            }

            CopyDirectory(baseAppPath, appPath);

            Console.WriteLine("Installation complete!");
        }

        private static void UninstallApplication()
        {
            Console.WriteLine("Uninstalling aplication...");

            if (Directory.Exists(appPath))
            {
                Directory.Delete(appPath, true);
            }

            Console.WriteLine("Uninstallation complete!");
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
