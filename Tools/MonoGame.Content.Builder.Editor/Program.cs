// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.CommandLine.Invocation;
using Eto;
using Eto.Forms;
using MonoGame.Tools.Pipeline.Utilities;

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
            new CommandLineParser(new CommandLineInterface()).Invoke(args);
        }

        private class CommandLineInterface : ICommandLineInterface
        {
            public void Register(InvocationContext context)
            {
                try
                {
                    FileAssociation.Associate();
                    Console.WriteLine("");
                    Console.WriteLine("Registered MGCB Editor!");
                }
                catch
                {
                    Console.WriteLine("");
                    Console.Error.WriteLine("Failed to register MGCB Editor");
                    throw;
                }
            }

            public void Unregister(InvocationContext context)
            {
                try
                {
                    FileAssociation.Unassociate();
                    Console.WriteLine("");
                    Console.WriteLine("Unregistered MGCB Editor!");
                }
                catch
                {
                    Console.WriteLine("");
                    Console.Error.WriteLine("Failed to unregister MGCB Editor");
                    throw;
                }
            }

            public void Run(InvocationContext context, string project)
            {
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
}
