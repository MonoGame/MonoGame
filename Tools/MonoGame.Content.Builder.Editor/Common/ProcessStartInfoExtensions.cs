// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoGame.Tools.Pipeline.Utilities
{
    static class ProcessStartInfoExtensions
    {
        /// <summary>
        /// Modifies the <see cref="ProcessStartInfo"/> FileName and Arguments to call the .NET Core App in the best way for the platform.
        /// A different method may be chosen if the caller intends to wait for the process to exit
        /// (e.g. choosing a .app's inner executable on Mac).
        /// </summary>
        public static ProcessStartInfo ResolveDotnetApp(this ProcessStartInfo startInfo, IEnumerable<string> searchPaths = null, bool waitForExit = false)
        {
            // Resolve the app.
            string appName = startInfo.FileName;
            searchPaths = GetSearchPaths(searchPaths);
            var (command, commandArgs) = ResolveCommand(appName, searchPaths, waitForExit);

            // Set the command and arguments.
            startInfo.FileName = command;
            if (commandArgs != null)
            {
                startInfo.Arguments = string.IsNullOrEmpty(startInfo.Arguments)
                    ? commandArgs
                    : $"{commandArgs} {startInfo.Arguments}";
            }

            return startInfo;
        }

        private static IEnumerable<string> GetSearchPaths(IEnumerable<string> extraSearchPaths)
        {
            var searchPaths = new List<string>
            {
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // In case we're running in the .app/Contents/MacOS folder, search back up in the root folder.
                // Since the dotnet assemblies can be unpacked from the app in different locations, use the process file instead.
                searchPaths.Add(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "../../../"));
            }

            if (extraSearchPaths != null)
            {
                searchPaths.AddRange(extraSearchPaths);
            }

            return searchPaths;
        }

        private static (string, string) ResolveCommand(string fileName, IEnumerable<string> searchPaths, bool waitForExit)
        {
            string appName = Path.ChangeExtension(fileName, null);

            string command = null;
            string commandArgs = null;
            foreach (string searchPath in searchPaths)
            {
                string testPath = Path.GetFullPath(Path.Combine(searchPath, appName));

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    string windowsTestPath = Path.ChangeExtension(testPath, "exe");
                    if (File.Exists(windowsTestPath))
                    {
                        command = windowsTestPath;
                        break;
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string macTestPath = Path.ChangeExtension(testPath, "app");
                    if (waitForExit)
                    {
                        // If waitForExit, get the executable out of the app bundle.
                        macTestPath = Path.Combine(macTestPath, "Contents", "MacOS", appName);
                        if (File.Exists(macTestPath))
                        {
                            command = macTestPath;
                            break;
                        }
                    }
                    else
                    {
                        // Otherwise use the .app itself.
                        if (Directory.Exists(macTestPath))
                        {
                            command = "open";
                            commandArgs = $"-n \"{macTestPath}\" --args";
                            break;
                        }
                    }
                }
                else
                {
                    string linuxTestPath = Path.ChangeExtension(testPath, null);
                    if (File.Exists(linuxTestPath))
                    {
                        command = linuxTestPath;
                        break;
                    }
                }

                string dotnetTestPath = Path.ChangeExtension(testPath, "dll");
                if (File.Exists(dotnetTestPath))
                {
                    command = DotNetMuxer.MuxerPathOrDefault();
                    commandArgs = $"\"{dotnetTestPath}\"";
                    break;
                }
            }

            if (command == null)
            {
                throw new FileNotFoundException($"{appName} is not in the search path!");
            }

            return (command, commandArgs);
        }

        /// <summary>
        /// Utilities for finding the "dotnet.exe" file from the currently running .NET Core application
        /// <see cref="https://github.com/dotnet/aspnetcore/blob/master/src/Shared/CommandLineUtils/Utilities/DotNetMuxer.cs"/>
        /// </summary>
        static class DotNetMuxer
        {
            private const string MuxerName = "dotnet";

            static DotNetMuxer()
            {
                MuxerPath = TryFindMuxerPath();
            }

            /// <summary>
            /// The full filepath to the .NET Core muxer.
            /// </summary>
            public static string MuxerPath { get; }

            /// <summary>
            /// Finds the full filepath to the .NET Core muxer,
            /// or returns a string containing the default name of the .NET Core muxer ('dotnet').
            /// </summary>
            /// <returns>The path or a string named 'dotnet'.</returns>
            public static string MuxerPathOrDefault() => MuxerPath ?? MuxerName;

            private static string TryFindMuxerPath()
            {
                var fileName = MuxerName;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    fileName += ".exe";
                }

                var mainModule = Process.GetCurrentProcess().MainModule;
                if (!string.IsNullOrEmpty(mainModule?.FileName)
                    && Path.GetFileName(mainModule.FileName).Equals(fileName, StringComparison.OrdinalIgnoreCase))
                {
                    return mainModule.FileName;
                }

                return null;
            }
        }
    }
}
