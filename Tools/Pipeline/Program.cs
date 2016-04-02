// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
#if WINDOWS
using System.Diagnostics;
using System.Windows.Forms;
#else
using Gtk;
#endif

namespace MonoGame.Tools.Pipeline
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string [] args)
        {
#if WINDOWS
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            PipelineSettings.Default.Load();

			var view = new MainView();
            if (args != null && args.Length > 0)
            {
                var projectFilePath = string.Join(" ", args);
                view.OpenProjectPath = projectFilePath;
            }

            var controller = new PipelineController(view);
            Application.Run(view);
#else
            Global.Initalize ();
            Application.Init ();

            var win = new MainWindow ();
            new PipelineController(win);

#if GTK3
            if(Global.UseHeaderBar && Global.App != null)
                Global.App.AddWindow(win);
#endif

#if LINUX
            if (args != null && args.Length > 0)
            {
            	var projectFilePath = string.Join(" ", args);
            	win.OpenProjectPath = projectFilePath;
            }
#elif MONOMAC
            var project = Environment.GetEnvironmentVariable("MONOGAME_PIPELINE_PROJECT");
            if (!string.IsNullOrEmpty (project)) {
            	win.OpenProjectPath = project;
            }
#endif
            
            win.Show (); 
            win.OnShowEvent ();
            
            Application.Run ();
#endif
        }
    }
}
