// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Helper to run an external tool installed in the system. Useful for when
    /// we don't want to package the tool ourselves (ffmpeg) or it's provided
    /// by a third party (console manufacturer).
    /// </summary>
    internal class ExternalTool
    {
        public static void Run(string command, string arguments)
        {
            // This particular case is likely to be the most common and thus
            // warrants its own specific error message rather than falling
            // back to a general exception from Process.Start()
            if (!FindInPath(command))
                throw new Exception(string.Format("The external tool '{0}' was not found in the system path.", command));

            var processInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false,
                FileName = command,
            };

            var process = new Process
            {
                StartInfo = processInfo
            };

            process.Start();
            process.WaitForExit();
            if (process.ExitCode < 0)
                throw new Exception(string.Format("{0} returned exit code {1}", processInfo.FileName, process.ExitCode));
        }

        /// <summary>
        /// Makes sure we can actually run the command before trying to start
        /// any processes.
        /// </summary>
        private static bool FindInPath(string fileName)
        {
            // Get just the executable name
            var command = Path.GetFileName(fileName);
            if (File.Exists(command))
                return true;

            // Search system paths
            var paths = Environment.GetEnvironmentVariable("PATH");
            foreach (var path in paths.Split(Path.PathSeparator))
            {
                var fullName = Path.Combine(path, command);
                if (File.Exists(fullName))
                    return true;
            }

            return false;
        }
    }
}
