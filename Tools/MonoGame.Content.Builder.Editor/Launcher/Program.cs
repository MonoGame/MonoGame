// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
//using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
//using MGCB.Editor.CommandLine;

namespace MonoGame.Tools.Pipeline.Launcher
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static /*async Task<int>*/void Main(string[] args)
        {
            //// Even though the launcher doesn't use the parameters, parse them here so that:
            ////   1. The launcher will know whether to detach or to pipe output and errors from the Editor.
            ////   2. The parser gets full native console features, including word wrapping and text color.
            //var parser = new CommandLineParser((string project, bool register, bool unregister) =>
            //{
            //    bool detach = !register && !unregister;
            //    Launch(detach);
            //});

            //return await parser.InvokeAsync(args);
        }

        //private static void Launch(bool detach)
        //{
        //    // Get the dotnet command, preferring the dotnet command that was used to execute this app.
        //    var processPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        //    var dotnetCommand = Path.GetFileName(processPath).Contains("dotnet") ? processPath : "dotnet";

        //    // Get the argument string by replacing the current assembly with the platform assembly in the current argument string.
        //    var assembly = Assembly.GetExecutingAssembly();
        //    var assemblyName = assembly.GetName().Name;
        //    var platform = GetPlatform();
        //    var arguments = Environment.CommandLine.Replace(assemblyName, $"{assemblyName}.{platform}");

        //    var process = new System.Diagnostics.Process();
        //    process.StartInfo.FileName = dotnetCommand;
        //    process.StartInfo.Arguments = arguments;
        //    process.StartInfo.CreateNoWindow = true;
        //    process.StartInfo.UseShellExecute = false;

        //    if (!detach)
        //    {
        //        process.StartInfo.RedirectStandardOutput = true;
        //        process.StartInfo.RedirectStandardError = true;
        //        process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => Console.WriteLine(e.Data);
        //        process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => Console.Error.WriteLine(e.Data);
        //    }

        //    process.Start();

        //    if (!detach)
        //    {
        //        process.BeginOutputReadLine();
        //        process.BeginErrorReadLine();
        //        process.WaitForExit();
        //    }
        //}

        //private static string GetPlatform()
        //{
        //    //switch (Environment.OSVersion.Platform)
        //    //{
        //    //    case PlatformID.Unix:
        //    //        return "Gtk";
        //    //    case PlatformID.MacOSX: // TODO: fix this. Mac's return Unix intentionally
        //    //        return "Mac";
        //    //    default:
        //    //        return "Windows";
        //    //}

        //    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //    {
        //        return "Windows";
        //    }
        //    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        //    {
        //        return "Mac"; // TODO: join Mac and Linux under Gtk here if that works
        //    }
        //    else
        //    {
        //        return "Gtk";
        //    }
        //}
    }
}
