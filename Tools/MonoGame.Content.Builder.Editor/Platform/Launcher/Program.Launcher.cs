// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoGame.Tools.Pipeline.Utilities;
using System;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

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

        /// <summary>
        /// The launcher's CLI wraps the platform-specific app and operates depending on the inputs:
        ///   - If CLI options were used, the launcher waits for the child to exit and pipes output and errors back out.
        ///   - If no options were used or only the 'project' argument was used, the launcher starts and detaches the child.
        /// </summary>
        private class CommandLineInterface : ICommandLineInterface
        {
            public void Register(InvocationContext context) => Launch(context, true);

            public void Unregister(InvocationContext context) => Launch(context, true);

            public void Run(InvocationContext context, string project) => Launch(context);

            private void Launch(InvocationContext context, bool waitForExit = false)
            {
                // Assemble the platform app process with the same arguments.
                var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
                var platform = GetPlatform();
                var appName = $"{assemblyName}-{platform}";
                var args = string.Join(" ", context.ParseResult.Tokens.Select(t => t.Value));

                var process = new System.Diagnostics.Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = appName,
                        Arguments = args,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    }.ResolveDotnetApp(waitForExit: waitForExit)
                };

                if (waitForExit)
                {
                    // If we're not detaching, pipe output back to the console.
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.OutputDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);
                    process.ErrorDataReceived += (sender, eventArgs) => Console.Error.WriteLine(eventArgs.Data);
                }

                process.Start();

                if (waitForExit)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    process.WaitForExit();
                }
            }

            private static string GetPlatform()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return "wpf";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return "mac";
                }
                else
                {
                    return "gtk";
                }
            }
        }
    }
}
