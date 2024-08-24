// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;

namespace MonoGame.Effect.Compiler
{
    public static class WineHelper
    {
        public static int Run(Options options)
        {
            var mgfxcwine = Environment.GetEnvironmentVariable("MGFXC_WINE_PATH");

            if (string.IsNullOrEmpty(mgfxcwine))
            {
                Console.Error.WriteLine("MGFXC effect compiler requires a valid Wine installation to be able to compile shaders.");
                Console.Error.WriteLine("");
                Console.Error.WriteLine("Setup instructions:");
                Console.Error.WriteLine("- Create 64 bit wine prefix");
                Console.Error.WriteLine("- Install d3dcompiler_47 using winetricks");
                Console.Error.WriteLine("- Install .NET 8");
                Console.Error.WriteLine("- Setup MGFXC_WINE_PATH environmental variable to point to a valid wine prefix");
                Console.Error.WriteLine("");
                return -1;
            }

            Environment.SetEnvironmentVariable("WINEARCH", "win64");
            Environment.SetEnvironmentVariable("WINEDLLOVERRIDES", "d3dcompiler_47=n");
            Environment.SetEnvironmentVariable("WINEPREFIX", mgfxcwine);
            Environment.SetEnvironmentVariable("WINEDEBUG", "-all");

            var assemblyLocation = typeof(Program).Assembly.Location;
            var input = ToPrefixPath(options.SourceFile);
            var output = ToPrefixPath(options.OutputFile);

            var proc = new Process();
            proc.StartInfo.FileName = "wine64";
            proc.StartInfo.Arguments = "dotnet ";
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
            var proc = new Process();
            proc.StartInfo.FileName = "wine64";
            proc.StartInfo.Arguments = "winepath.exe -w \"" + localPath + "\"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            return proc.StandardOutput.ReadToEnd().Trim('\n');
        }
    }
}
