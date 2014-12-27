﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
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

            if (args.Length == 0)
            {
                // This simple hack is fix for irritating bug described in https://github.com/mono/MonoGame/issues/3230

                string missedContentPath = Directory.GetCurrentDirectory().ToString() + @"\Content.mgcb";

                if (File.Exists(missedContentPath))
                {
                    args = new string[1];
                    args[0] = missedContentPath;
                }
            }

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
			Gtk.Application.Run ();

#endif
        }
    }
}
