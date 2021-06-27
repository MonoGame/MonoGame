using System;
using System.IO;
using System.CommandLine.Invocation;

namespace MonoGame.Content.Builder.Editor.Launcher
{
    public class MacPlatform : IPlatform
    {
        private static readonly string _systemAppPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Applications/MGCB Editor.app");

        private static string LocalAppPath
        {
            get
            {
                var localAppPath = Path.Combine(Path.GetDirectoryName(typeof(MacPlatform).Assembly.Location), "MGCB Editor.app");

                if (!Directory.Exists(localAppPath))
                    throw new DirectoryNotFoundException(localAppPath);

                if (Path.GetExtension(localAppPath) != ".app")
                    throw new FileNotFoundException("Mac .app package not found :(");

                return localAppPath;
            }
        }

        public void Register(InvocationContext context) =>
            InstallApp(LocalAppPath, _systemAppPath);

        public void Unregister(InvocationContext context) =>
            Directory.Delete(_systemAppPath, true);

        public void Run(InvocationContext context, string project) => // TODO: Figure out how to pass the project argument
            Process.StartProcess("open", "\"" + LocalAppPath + "\"");

        private void InstallApp(string sourceDirName, string destDirName)
        {
            var dir = new DirectoryInfo(sourceDirName);

            if (!Directory.Exists(destDirName))
                Directory.CreateDirectory(destDirName);

            foreach (var file in dir.GetFiles())
            {
                // We change the name of the .app file so we also need to change the names of related files inside the bundle
                var fileName = file.Name.Replace(Path.GetFileNameWithoutExtension(sourceDirName), Path.GetFileNameWithoutExtension(_systemAppPath));
                file.CopyTo(Path.Combine(destDirName, fileName), false);
            }

            foreach (var subdir in dir.GetDirectories())
                InstallApp(subdir.FullName, Path.Combine(destDirName, subdir.Name));
        }
    }
}
