// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
#if WINDOWS
using System.Windows.Forms;
#endif
#if XWT
using Xwt;
#endif
#if GTK
using Gtk;
using IgeMacIntegration;
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
#if XWT
#if MONOMAC
            Application.Initialize(ToolkitType.Cocoa);
#endif
#if LINUX
            Application.Initialize(ToolkitType.Gtk);
#endif
			var view = new XwtView();
            if (args != null && args.Length > 0)
            {
                var projectFilePath = string.Join(" ", args);
                view.OpenProjectPath = projectFilePath;
            }

            var model = new PipelineProject();
            new PipelineController(view, model);   
			view.Show();
            Application.Run();
#endif
#if GTK
			Application.Init ();
			var win = new Gtk.Window ("Pipeline");
			win.SetFlag(WidgetFlags.Toplevel);
			win.SetPosition(WindowPosition.Center);
			win.Name = "MonoGame Content Pipeline";
			win.Title = "Pipeline";
			if (System.IO.File.Exists ("App.ico"))
				win.SetIconFromFile("App.ico");
			win.SetSizeRequest (800,600);

			var view = new GtkView(win);
			if (args != null && args.Length > 0)
			{
				var projectFilePath = string.Join(" ", args);
				projectFilePath = System.IO.Path.GetFullPath (projectFilePath);
				if (System.IO.File.Exists(projectFilePath))
					view.OpenProjectPath =  projectFilePath;
			}

			GtkView.CreateControllers (view);

			win.Add (view);
			win.DeleteEvent += (o, a) => {
				if (view.Exit())
					Application.Quit();
			};
			view.BuildUI();
			win.ShowAll ();
			Application.Run ();
#endif
        }
    }
}
