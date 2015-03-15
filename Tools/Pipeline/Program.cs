// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
#if WINDOWS
using System.Windows.Forms;
#endif
#if MONOMAC
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

            History.Default.Load();

			var view = new MainView();
            if (args != null && args.Length > 0)
            {
                var projectFilePath = string.Join(" ", args);
                view.OpenProjectPath = projectFilePath;
            }

            var model = new PipelineProject();
            var controller = new PipelineController(view, model);   
            Application.Run(view);
#endif
#if LINUX || MONOMAC

			Gtk.Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show (); 
			var model = new PipelineProject();
			new PipelineController(win, model);  
			if (args != null && args.Length > 0)
			{
				var projectFilePath = string.Join(" ", args);
				win.OpenProjectPath = projectFilePath;
			}
			win.OnShowEvent ();
			Gtk.Application.Run ();
#endif
        }
    }
}
