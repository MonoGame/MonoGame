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
        public static int Run(string command, string arguments)
        {
            string stdout, stderr;
            var result = Run(command, arguments, out stdout, out stderr);
            if (result < 0)
                throw new Exception(string.Format("{0} returned exit code {1}", command, result));

            return result;
        }

        public static int Run(string command, string arguments, out string stdout, out string stderr)
        {
            // This particular case is likely to be the most common and thus
            // warrants its own specific error message rather than falling
            // back to a general exception from Process.Start()
            var fullPath = FindCommand(command);
            if (string.IsNullOrEmpty(fullPath))
                throw new Exception(string.Format("Couldn't locate external tool '{0}'.", command));

            var processInfo = new ProcessStartInfo
            {
                Arguments = arguments,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                ErrorDialog = false,
                FileName = fullPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using (var process = new Process())
            {
                process.StartInfo = processInfo;

                process.Start();

                stdout = process.StandardOutput.ReadToEnd();
                stderr = process.StandardError.ReadToEnd();

                process.WaitForExit();

                return process.ExitCode;
            }
        }

        /// <summary>
        /// Returns the fully-qualified path for a command, searching the system path if necessary.
        /// </summary>
        /// <remarks>
        /// It's apparently necessary to use the full path when running on some systems.
        /// </remarks>
        private static string FindCommand(string command)
        {
            // If we have a full path just pass it through.
            if (File.Exists(command))
                return command;

            // We don't have a full path, so try running through the system path to find it.
            var paths = AppDomain.CurrentDomain.BaseDirectory +
                Path.PathSeparator +
                Environment.GetEnvironmentVariable("PATH");

            var justTheName = Path.GetFileName(command);
            foreach (var path in paths.Split(Path.PathSeparator))
            {
                var fullName = Path.Combine(path, justTheName);
                if (File.Exists(fullName))
                    return fullName;

#if WINDOWS
                var fullExeName = string.Concat(fullName, ".exe");
                if (File.Exists(fullExeName))
                    return fullExeName;
#endif
            }

            return null;
        }
    }
}
