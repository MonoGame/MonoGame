// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;

namespace MonoGame.Effect.Compiler
{
    public static class WineHelper
    {
        static string wineExecutable = "wine";
        static WineHelper()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                throw new PlatformNotSupportedException("WineHelper is only supported on Unix platforms.");

            var proc = new Process();
            proc.StartInfo.FileName = "wine64";
            proc.StartInfo.Arguments = "--version";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            try {
                proc.Start();
                proc.WaitForExit();
                if (proc.ExitCode == 0) {
                    wineExecutable = "wine64";
                    return;
                }
            }
            catch (Exception)
            {
                proc.StartInfo.FileName = "wine";
            }

            try {
                proc.Start();
                proc.WaitForExit();
                if (proc.ExitCode == 0) {
                    wineExecutable = "wine";
                    return;
                }
            }
            catch (Exception)
            {
                throw new PlatformNotSupportedException("Wine is not installed on this system.");
            }
        }
        public static int Run(Options options)
        {
            var mgfxcwine = Environment.GetEnvironmentVariable("MGFXC_WINE_PATH");

            if (string.IsNullOrEmpty(mgfxcwine))
            {
                string os = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos" : "linux";
                Console.Error.WriteLine($"Error: MGFXC0001: MGFXC effect compiler requires a valid Wine installation to be able to compile shaders. Please visit https://monogame.net/MGFX1000?tab={os} for instructions on how to set up Wine.");
                return -1;
            }

            Environment.SetEnvironmentVariable("WINEARCH", "win64");
            Environment.SetEnvironmentVariable("WINEDLLOVERRIDES", "d3dcompiler_47=n,explorer.exe=e,services.exe=f");
            Environment.SetEnvironmentVariable("WINEPREFIX", mgfxcwine);
            Environment.SetEnvironmentVariable("WINEDEBUG", "-all");
            Environment.SetEnvironmentVariable("MVK_CONFIG_LOG_LEVEL", "0"); // hide MoltenVK logs

            var assemblyLocation = typeof(Program).Assembly.Location;
            var input = ToPrefixPath(options.SourceFile);
            var output = ToPrefixPath(options.OutputFile);

            var proc = new Process();
            proc.StartInfo.FileName = wineExecutable;
            proc.StartInfo.Arguments = "dotnet ";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.AddPathArgument(assemblyLocation);
            proc.StartInfo.AddPathArgument(input);
            proc.StartInfo.AddPathArgument(output);
            proc.StartInfo.AddOptionArgument("Profile", options.Profile.Name);
            proc.StartInfo.AddOptionArgument("Defines", options.Defines);
            proc.StartInfo.AddOptionArgument("Debug", options.Debug);

            proc.Start();
            proc.WaitForExit();

            if (!File.Exists(options.OutputFile) || (new FileInfo(options.OutputFile)).Length == 0)
                return 1;

            return proc.ExitCode;
        }

        public static void AddPathArgument(this ProcessStartInfo startInfo, string value)
        {
            if (startInfo.Arguments == null)
                startInfo.Arguments = "";

            startInfo.Arguments += "\"" + value + "\" ";
        }

        public static void AddOptionArgument(this ProcessStartInfo startInfo, string option, object value)
        {
            if (startInfo.Arguments == null)
                startInfo.Arguments = "";

            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return;

            startInfo.Arguments += "/" + option + ":" + value + " ";
        }

        public static string ToPrefixPath(string localPath)
        {
            var assemblyLocation = typeof(Program).Assembly.Location;
            var proc = new Process();
            proc.StartInfo.FileName = wineExecutable;
            proc.StartInfo.Arguments = "winepath.exe -w \"" + localPath + "\"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            return proc.StandardOutput.ReadToEnd().Trim('\n');
        }
    }
}
