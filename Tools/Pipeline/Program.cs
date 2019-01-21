// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Eto;
using Eto.Forms;

namespace MonoGame.Tools.Pipeline
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Styles.Load();

            var app = new Application(Platform.Detect);
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

#if LINUX
                Global.Application.AddWindow(win.ToNative() as Gtk.Window);
#endif

#if LINUX && !DEBUG

                GLib.ExceptionManager.UnhandledException += (e) =>
                {
                    var logwin = new LogWindow();
                    logwin.LogText = e.ExceptionObject.ToString();

                    logwin.Show();
                    win.Close();
                };
#endif

                string project = null;

                if (Global.Unix && !Global.Linux)
                    project = Environment.GetEnvironmentVariable("MONOGAME_PIPELINE_PROJECT");
                else if (args != null && args.Length > 0)
                    project = string.Join(" ", args);

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
# endif
        }
    }
}
