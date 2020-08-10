using MonoGame.Tools.Pipeline.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MonoGame.Tools.Pipeline
{
    public static class FileAssociation
    {
        // System directories.
        private static readonly string applicationDirectory = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/share/applications");
        private static readonly string iconRootDirectory = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/share/icons/hicolor");
        private const string iconType = "scalable/mimetypes";

        // Content files.
        private const string contentFolder = "Content";
        private const string iconFileName = "monogame.svg";
        private const string oldMimetypeFileName = "mgcb.xml";
        private const string newMimetypeFileName = "x-mgcb.xml";
        private const string desktopFileName = "mgcb-editor.desktop";
        private const string mimetype = "text/x-mgcb";

        public static void Associate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var contentDirectory = Path.Join(Path.GetDirectoryName(assembly.Location), contentFolder);
            InstallIcon(contentDirectory);
            InstallMimetype(contentDirectory);
            InstallApplication(contentDirectory, assembly);
        }

        public static void Unassociate()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var contentDirectory = Path.Join(Path.GetDirectoryName(assembly.Location), contentFolder);
            UninstallIcon();
            UninstallMimetype(contentDirectory);
            UninstallApplication();
        }

        private static void InstallIcon(string contentDirectory)
        {
            Console.WriteLine("Installing icon...");

            // Copy the icon.
            var iconPath = Path.Join(contentDirectory, iconFileName);
            var outputIconDirectory = Path.Join(iconRootDirectory, iconType);
            var outputIconPath = Path.Join(outputIconDirectory, iconFileName);
            Directory.CreateDirectory(outputIconDirectory);
            File.Copy(iconPath, outputIconPath, true);

            try
            {
                // Update the GTK icon cache.
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "gtk-update-icon-cache",
                        Arguments = $"{iconRootDirectory} -f",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }
                };
                process.Start();
                process.WaitForExit();
            }
            catch { }

            Console.WriteLine("Installation complete!");
        }

        private static void InstallMimetype(string contentDirectory)
        {
            Console.WriteLine("Installing mimetype...");

            var oldMimetypePath = Path.Join(contentDirectory, oldMimetypeFileName);
            var newMimetypePath = Path.Join(contentDirectory, newMimetypeFileName);

            RunXdgMime("uninstall", oldMimetypePath);
            RunXdgMime("install", newMimetypePath);

            Console.WriteLine("Installation complete!");
        }

        private static void InstallApplication(string contentDirectory, Assembly assembly)
        {
            Console.WriteLine("Installing application...");

            // Resolve a dotnet ProcessStartInfo to get the commands and arguments to register.
            var startInfo = new ProcessStartInfo
            {
                FileName = assembly.GetName().Name,
                Arguments = "%F"
            }.ResolveDotnetApp();

            // Read and update the .desktop file.
            var desktopFilePath = Path.Join(contentDirectory, desktopFileName);
            var desktopFileContent = File.ReadAllText(desktopFilePath)
                .Replace("{Exec}", $"\"{startInfo.FileName}\" {startInfo.Arguments}")
                .Replace("{TryExec}", startInfo.FileName);

            // Write and install the .desktop file.
            var outputDesktopFilePath = Path.Join(applicationDirectory, desktopFileName);
            File.WriteAllText(outputDesktopFilePath, desktopFileContent);
            RunXdgMime("default", $"\"{desktopFileName}\" {mimetype}");

            Console.WriteLine("Installation complete!");
        }

        private static void UninstallIcon()
        {
            Console.WriteLine("Uninstalling icon...");

            var outputIconPath = Path.Join(iconRootDirectory, iconType, iconFileName);
            File.Delete(outputIconPath);

            Console.WriteLine("Uninstallation complete!");
        }

        private static void UninstallMimetype(string contentDirectory)
        {
            Console.WriteLine("Uninstalling mimetype...");

            var newMimetypePath = Path.Join(contentDirectory, newMimetypeFileName);
            RunXdgMime("uninstall", newMimetypePath);

            Console.WriteLine("Uninstallation complete!");
        }

        private static void UninstallApplication()
        {
            Console.WriteLine("Uninstalling aplication...");

            var outputDesktopFilePath = Path.Join(applicationDirectory, desktopFileName);
            File.Delete(outputDesktopFilePath);

            Console.WriteLine("Uninstallation complete!");
        }

        private static void RunXdgMime(string command, string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "xdg-mime",
                    Arguments = $"{command} {arguments}",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };

            process.Start();
            process.WaitForExit();
        }
    }
}
