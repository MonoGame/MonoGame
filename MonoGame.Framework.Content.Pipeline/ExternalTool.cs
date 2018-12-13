// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using MonoGame.Utilities;

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

        public static int Run(string command, string arguments, out string stdout, out string stderr, string stdin = null)
        {
            // This particular case is likely to be the most common and thus
            // warrants its own specific error message rather than falling
            // back to a general exception from Process.Start()
            var fullPath = FindCommand(command);
            if (string.IsNullOrEmpty(fullPath))
                throw new Exception(string.Format("Couldn't locate external tool '{0}'.", command));

            // We can't reference ref or out parameters from within
            // lambdas (for the thread functions), so we have to store
            // the data in a temporary variable and then assign these
            // variables to the out parameters.
            var stdoutTemp = string.Empty;
            var stderrTemp = string.Empty;

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
                RedirectStandardInput = true,
            };

            EnsureExecutable(fullPath);

            using (var process = new Process())
            {
                process.StartInfo = processInfo;

                process.Start();

                // We have to run these in threads, because using ReadToEnd
                // on one stream can deadlock if the other stream's buffer is
                // full.
                var stdoutThread = new Thread(new ThreadStart(() =>
                {
                    var memory = new MemoryStream();
                    process.StandardOutput.BaseStream.CopyTo(memory);
                    var bytes = new byte[memory.Position];
                    memory.Seek(0, SeekOrigin.Begin);
                    memory.Read(bytes, 0, bytes.Length);
                    stdoutTemp = System.Text.Encoding.ASCII.GetString(bytes);
                }));
                var stderrThread = new Thread(new ThreadStart(() =>
                {
                    var memory = new MemoryStream();
                    process.StandardError.BaseStream.CopyTo(memory);
                    var bytes = new byte[memory.Position];
                    memory.Seek(0, SeekOrigin.Begin);
                    memory.Read(bytes, 0, bytes.Length);
                    stderrTemp = System.Text.Encoding.ASCII.GetString(bytes);
                }));

                stdoutThread.Start();
                stderrThread.Start();

                if (stdin != null)
                {
                    process.StandardInput.Write(System.Text.Encoding.ASCII.GetBytes(stdin));
                }

                // Make sure interactive prompts don't block.
                process.StandardInput.Close();

                process.WaitForExit();

                stdoutThread.Join();
                stderrThread.Join();

                stdout = stdoutTemp;
                stderr = stderrTemp;

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
            // Expand any environment variables.
            command = Environment.ExpandEnvironmentVariables(command);

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

                if (CurrentPlatform.OS == OS.Windows)
                {
                    var fullExeName = string.Concat(fullName, ".exe");
                    if (File.Exists(fullExeName))
                        return fullExeName;
                }
            }

            return null;
        }

        /// <summary>   
        /// Ensures the specified executable has the executable bit set.  If the    
        /// executable doesn't have the executable bit set on Linux or Mac OS, then 
        /// Mono will refuse to execute it. 
        /// </summary>  
        /// <param name="path">The full path to the executable.</param> 
        private static void EnsureExecutable(string path)
        {
#if LINUX || MACOS
            if (path == "/bin/bash")
                return;

            try
            {

                var p = Process.Start("chmod", "u+x '" + path + "'");
                p.WaitForExit();
            }
            catch
            {
                // This platform may not have chmod in the path, in which case we can't 
                // do anything reasonable here. 
            }
#endif
        }

        /// <summary>
        /// Safely deletes the file if it exists.
        /// </summary>
        /// <param name="filePath">The path to the file to delete.</param>
        public static void DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception)
            {                    
            }
        }
    }
}
