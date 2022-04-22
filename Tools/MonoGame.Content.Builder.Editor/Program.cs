// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using Eto;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
#if MONOMAC
            if (!CheckInAppBundle(args))
                return;
#endif
            string project = null;

            if (args.Length == 1)
                project = args[0];

            Styles.Load();

#if GTK
            var app = new Application(Platforms.Gtk);
#elif WPF
            var app = new Application(Platforms.Wpf);
#else
            var app = new Application(Platforms.Mac64);
#endif

            app.Style = "PipelineTool";

            PipelineSettings.Default.Load();

            if (!string.IsNullOrEmpty(PipelineSettings.Default.ErrorMessage))
            {
                var logwin = new LogWindow();
                logwin.LogText = PipelineSettings.Default.ErrorMessage;
                app.Run(logwin);
                return;
            }

#if !DEBUG
            try
#endif
            {
                var win = new MainWindow();
                var controller = PipelineController.Create(win);

#if GTK
                Global.Application.AddWindow(win.ToNative() as Gtk.Window);
#endif

#if GTK && !DEBUG
                GLib.ExceptionManager.UnhandledException += (e) =>
                {
                    var logwin = new LogWindow();
                    logwin.LogText = e.ExceptionObject.ToString();

                    logwin.Show();
                    win.Close();
                };
#endif

                if (!string.IsNullOrEmpty(project))
                    controller.OpenProject(project);

                app.Run(win);
            }
#if !DEBUG
            catch (Exception ex)
            {
                PipelineSettings.Default.ErrorMessage = ex.ToString();
                PipelineSettings.Default.Save();
                app.Restart();
            }
#endif
        }

#if MONOMAC
        private static bool CheckInAppBundle(string[] args)
        {
            var assemblyLocation = typeof(Program).Assembly.Location ?? "";
            var currentDir = Path.GetDirectoryName(assemblyLocation);
		    var appBundlePath = Path.GetDirectoryName(Path.GetDirectoryName(currentDir));
            var appBundleName = Path.GetFileName(appBundlePath);

            if (!appBundleName.EndsWith(".app"))
            {
                // We are not in the app bundle! Launch the app!
                var appPath = Path.Combine(currentDir, "MGCB Editor.app");

                if (!Directory.Exists(appPath))
                {
                    // App bundle does not exist, generate the app bundle using current directory contents!
                    var contentsPath = Path.Combine(appPath, "Contents");

                    var macosDir = Path.Combine(contentsPath, "MacOS");
                    Directory.CreateDirectory(contentsPath);
                    CopyDirectory(
                        currentDir,
                        macosDir
                    );

                    var resourcesDir = Path.Combine(contentsPath, "Resources");
                    Directory.CreateDirectory(resourcesDir);
                    File.Copy(
                        Path.Combine(currentDir, "Icon.icns"),
                        Path.Combine(resourcesDir, "Icon.icns")
                    );
                    
                    File.Copy(
                        Path.Combine(currentDir, "Info.plist"),
                        Path.Combine(contentsPath, "Info.plist")
                    );
                }

                Process.Start("open", $"-a \"{appPath}\"" + (args.Length > 0 ? $" \"{args[0]}\"" : ""));

                return false;
            }

            return true;
        }

        private static void CopyDirectory(string sourcePath, string destPath)
        {
            var directories = Directory.GetDirectories(sourcePath);

            if (!Directory.Exists(destPath))
                Directory.CreateDirectory(destPath);

            foreach (var dir in directories)
            {
                var dirName = Path.GetFileName(dir);
                var newDirPath = Path.Combine(destPath, dirName);

                if (!dirName.EndsWith(".app"))
                    CopyDirectory(dir, newDirPath);
            }

            var files = Directory.GetFiles(sourcePath);

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var newFilePath = Path.Combine(destPath, fileName);

                File.Copy(file, newFilePath);
            }
        }
#endif
    }
}
