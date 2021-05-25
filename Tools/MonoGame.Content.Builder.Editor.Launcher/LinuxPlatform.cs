using System;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Process = System.Diagnostics.Process;

namespace MonoGame.Content.Builder.Editor.Launcher
{
    public class LinuxPlatform : IPlatform
    {
        private static readonly string _localPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "mgcb-editor-linux");
        private static readonly string _localApp = Path.Combine(_localPath, "mgcb-editor-linux.dll");
        private static readonly string _localIconPath = Path.Combine(_localPath, "monogame.svg");
        private static readonly string _localMimePath = Path.Combine(_localPath, "x-mgcb.xml");
        private static readonly string _localOldMimePath = Path.Combine(_localPath, "mgcb.xml");
        private static readonly string _localDesktopFilePath = Path.Combine(_localPath, "mgcb-editor.desktop");
        private static readonly string _systemIconPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/share/icons/hicolor/scalable/mimetypes/monogame.svg");
        private static readonly string _systemDesktopFilePath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local/share/applications/mgcb-editor.desktop");
        
        public void Register(InvocationContext context)
        {
            InstallIcon();
            InstallMimetype();
            InstallDesktopFile();
        }

        public void Unregister(InvocationContext context)
        {
            UninstallIcon();
            UninstallMimetype();
            UninstallDesktopFile();
        }

        public void Run(InvocationContext context, string project)
        {
            Process.Start("dotnet", $"\"{_localApp}\" \"{project}\"");
        }
        
        private static void InstallIcon()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_systemIconPath));
            File.Copy(_localIconPath, _systemIconPath);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "gtk-update-icon-cache",
                    Arguments = "~/.local/share/icons -f",
                    CreateNoWindow = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private static void UninstallIcon()
        {
            if (File.Exists(_systemIconPath))
                File.Delete(_systemIconPath);
        }

        private static void InstallMimetype()
        {
            RunXdgMime("uninstall", _localOldMimePath);
            RunXdgMime("install", _localMimePath);
        }

        private static void UninstallMimetype()
        {
            RunXdgMime("uninstall", _localOldMimePath);
            RunXdgMime("uninstall", _localMimePath);
        }

        private static void InstallDesktopFile()
        {
            // Resolve a dotnet ProcessStartInfo to get the commands and arguments to register.
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"\"{_localApp}\" %F"
            };

            // Read and update the .desktop file.
            var desktopFileContent = File.ReadAllText(_localDesktopFilePath)
                .Replace("{Exec}", $"\"{startInfo.FileName}\" {startInfo.Arguments}")
                .Replace("{TryExec}", startInfo.FileName);

            // Write and install the .desktop file.
            File.WriteAllText(_systemDesktopFilePath, desktopFileContent);
            RunXdgMime("default", Path.GetFileName(_systemDesktopFilePath) + " text/x-mgcb");
        }

        private static void UninstallDesktopFile()
        {
            if (File.Exists(_systemDesktopFilePath))
                File.Delete(_systemDesktopFilePath);
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
