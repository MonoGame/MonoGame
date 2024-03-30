// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
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
    }
}
