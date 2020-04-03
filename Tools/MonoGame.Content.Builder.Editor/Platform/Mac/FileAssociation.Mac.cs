using MonoGame.Tools.Pipeline.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MonoGame.Tools.Pipeline
{
    public static class FileAssociation
    {
        private const string lsregisterPath = "/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister";

        public static void Associate()
        {
            RunLsregister("-v -f");
        }

        public static void Unassociate()
        {
            RunLsregister("-v -f -u");
        }

        private static void RunLsregister(string arguments)
        {
            // Assuming we're running in .app/Contents/MacOS, go back up to the .app
            var appPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), "../.."));
            if (Path.GetExtension(appPath) != ".app")
            {
                throw new FileNotFoundException("Not running from within the app package");
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = lsregisterPath,
                    Arguments = $"{arguments} \"{appPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.OutputDataReceived += (sender, eventArgs) => Console.WriteLine(eventArgs.Data);
            process.ErrorDataReceived += (sender, eventArgs) => Console.Error.WriteLine(eventArgs.Data);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}
