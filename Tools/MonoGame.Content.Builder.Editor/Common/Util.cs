using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoGame.Tools.Pipeline
{
    public static class Util
    {
        [DllImport("libc")]
        private static extern string realpath(string path, IntPtr resolved_path);

        public static string GetRealPath(string path)
        {
            // resolve symlinks on Unix systems
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                return realpath(path, IntPtr.Zero);
            
            return path;
        }

        /// <summary>        
        /// Returns the path 'filspec' made relative path 'folder'.
        /// 
        /// If 'folder' is not an absolute path, throws ArgumentException.
        /// If 'filespec' is not an absolute path, returns 'filespec' unmodified.
        /// </summary>
        public static string GetRelativePath(string filespec, string folder)
        {
            if (!Path.IsPathRooted(filespec))
                return filespec;

            if (!Path.IsPathRooted(folder))
                throw new ArgumentException("Must be an absolute path.", "folder");

            filespec = Path.GetFullPath(filespec).TrimEnd(new[] { '/', '\\' });
            folder = Path.GetFullPath(folder).TrimEnd(new[] { '/', '\\' });

            if (filespec == folder)
                return string.Empty;

            var pathUri = new Uri(filespec);
            var folderUri = new Uri(folder + Path.DirectorySeparatorChar);
            var result = folderUri.MakeRelativeUri(pathUri).ToString();
            result = result.Replace('/', Path.DirectorySeparatorChar);
            result = Uri.UnescapeDataString(result);

            return result;
        }

        public static int Run(string command, string arguments, string workingDirectory)
        {
            var process = CreateProcess(command, arguments, workingDirectory, Encoding.UTF8, (s) => Console.WriteLine(s));
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
            return process.ExitCode;
        }

        public static Process CreateProcess(string command, string arguments, string workingDirectory, Encoding encoding, Action<string> output)
        {
            var exe = command;
            var args = arguments;
            if (command.EndsWith(".dll"))
            {
                // we are referencing the dll directly. We need to call dotnet to host.
                exe = Global.Unix ? "dotnet" : "dotnet.exe";
                args = $"\"{command}\" {arguments}";
            }
            var _buildProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    WorkingDirectory = workingDirectory,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = encoding
                }
            };
            _buildProcess.OutputDataReceived += (sender, args) => output(args.Data);
            return _buildProcess;
        }
    }
}
