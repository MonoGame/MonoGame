// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Windows.Forms;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var view = new MainView();
            var controller = new PipelineController(view);
            if (args.Length > 0)
            {
                view.Shown += (e, f) =>
                                  {
                                      controller.OpenProject(args[0]);                                      
                                  }; 
            }

            Application.Run(view);
        }
    }
}
