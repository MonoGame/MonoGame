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
            var platform = Platform.Detect;

            var app = new Application(platform);
            Styles.Load();

            var win = new MainWindow();
            var controller = PipelineController.Create(win);

            string project = null;

            if (Global.Unix && !Global.Linux)
                project = Environment.GetEnvironmentVariable("MONOGAME_PIPELINE_PROJECT");
            else if (args != null && args.Length > 0)
                project = string.Join(" ", args);

            if (!string.IsNullOrEmpty(project))
                controller.OpenProject(project);

            app.Run(win);
        }
    }
}
