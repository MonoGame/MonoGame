﻿// MonoGame - Copyright (C) The MonoGame Team
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
#if WINDOWS
            Xwt.Application.InitializeAsGuest(Xwt.ToolkitType.Wpf);
#elif LINUX
            Xwt.Application.InitializeAsGuest(Xwt.ToolkitType.Gtk3);
#endif
            app.Style = "PipelineTool";

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
