// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MonoGame.Tools.Pipeline.Utilities
{
    static class ProcessStartInfoExtensions
    {
        public static ProcessStartInfo ResolveDotnetApp(this ProcessStartInfo startInfo, IEnumerable<string> searchPaths = null)
        {
            string filePath = FindDotnetApp(startInfo.FileName, searchPaths);

            if (Path.GetExtension(filePath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                startInfo.FileName = DotNetMuxer.MuxerPathOrDefault();
                startInfo.Arguments = string.IsNullOrEmpty(startInfo.Arguments)
                    ? $"\"{filePath}\""
                    : $"\"{filePath}\" {startInfo.Arguments}";
            }
            else
            {
                startInfo.FileName = filePath;
            }

            return startInfo;
        }

        private static string FindDotnetApp(string fileName, IEnumerable<string> searchPaths = null)
        {
            string filePath = null;
            searchPaths ??= new string[] { "" };
            foreach (string searchPath in searchPaths)
            {
                string testPath = Path.GetFullPath(Path.Combine(searchPath, fileName));
                string unixTestPath = Path.ChangeExtension(testPath, null);
                string windowsTestPath = Path.ChangeExtension(testPath, "exe");
                string dotnetTestPath = Path.ChangeExtension(testPath, "dll");
                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    if (File.Exists(windowsTestPath))
                    {
                        filePath = windowsTestPath;
                        break;
                    }
                }
                else
                {
                    if (File.Exists(unixTestPath))
                    {
                        filePath = unixTestPath;
                        break;
                    }
                }

                if (File.Exists(dotnetTestPath))
                {
                    filePath = dotnetTestPath;
                    break;
                }
            }

            if (filePath == null)
            {
                throw new FileNotFoundException($"{fileName} is not in the search path!");
            }

            return filePath;
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
