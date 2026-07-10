// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.D3DCompiler;

namespace MonoGame.Effect.Compiler
{
    static class WineHelper
    {
        static string _wineExecutable = "wine";
        static string _winePathExecutable = "winepath";

        static WineHelper()
        {
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                throw new PlatformNotSupportedException("WineHelper is only supported on Unix platforms.");
            }

            if (!DetectWine() || !SetupWine() || !DetectWinePath())
            {
                var os = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "macos" : "linux";
                var errMessage = $"Error: MGFXC0001: MGFXC effect compiler requires a valid Wine installation to be able to compile shaders. Please visit https://docs.monogame.net/errors/mgfx0001?tab={os} for more details.";
                Console.Error.WriteLine(errMessage);
                throw new Exception(errMessage);
            }
        }

        static string Which(params string[] exes)
        {
            var proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.RedirectStandardOutput = true;

            foreach (var exe in exes)
            {
                proc.StartInfo.FileName = "which";
                proc.StartInfo.Arguments = exe;
                proc.Start();
                proc.WaitForExit();
                if (proc.ExitCode == 0)
                {
                    return exe;
                }
            }
            return string.Empty;
        }

        static bool DetectWinePath()
        {
            _winePathExecutable =Which("winepath");
            return !string.IsNullOrEmpty(_winePathExecutable);
        }

        static bool DetectWine()
        {
            string[] wineCommands = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ?
                ["wine64", "wine"] :
                ["wine", "wine64"];
            _wineExecutable = Which(wineCommands);
            return !string.IsNullOrEmpty(_wineExecutable);
        }

        static bool SetupWine()
        {
            var mgfxcwine = Environment.GetEnvironmentVariable("MGFXC_WINE_PATH");
            if (string.IsNullOrEmpty(mgfxcwine))
            {
                return false;
            }

            Environment.SetEnvironmentVariable("WINEARCH", "win64");
            Environment.SetEnvironmentVariable("WINEDLLOVERRIDES", "d3dcompiler_47=n,explorer.exe=e,services.exe=f");
            Environment.SetEnvironmentVariable("WINEPREFIX", mgfxcwine);
            Environment.SetEnvironmentVariable("WINEDEBUG", "-all");
            Environment.SetEnvironmentVariable("MVK_CONFIG_LOG_LEVEL", "0"); // hide MoltenVK logs
            return true;
        }

        static int RunInWine(string cmd, out string output)
        {
            Console.WriteLine($"Running in Wine: {_wineExecutable} {cmd}");
            var proc = new Process();
            proc.StartInfo.FileName = _wineExecutable;
            proc.StartInfo.Arguments = cmd;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            proc.Start();
            proc.WaitForExit();

            output = proc.StandardOutput.ReadToEnd() + proc.StandardError.ReadToEnd();

            return proc.ExitCode;
        }

        static string GetWinePath(string path)
        {
            var proc = new Process();
            proc.StartInfo.FileName = _winePathExecutable;
            proc.StartInfo.Arguments = $"-w \"{path}\"";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;

            proc.Start();
            proc.WaitForExit();

            return '"' + proc.StandardOutput.ReadToEnd().Replace(@"\", @"\\").Trim('\n') + '"';
        }

        public static CompilationResult RunFxc2(string fileContents, string shaderFunction, string shaderProfile, ShaderFlags shaderFlags, string displayPath)
        {
            var srcPath = Path.GetTempFileName();
            var dstPath = Path.GetTempFileName();
            CompilationResult ret = null;

            try
            {
                File.WriteAllText(srcPath, fileContents);

                var cmd = $"dotnet c:\\fxccs.dll {GetWinePath(srcPath)} {shaderFunction} {shaderProfile} {(int)shaderFlags} {displayPath} {GetWinePath(dstPath)}";
                var result = RunInWine(cmd, out string output);
                if (result == 0)
                {
                    ret = new CompilationResult(new ShaderBytecode(File.ReadAllBytes(dstPath)), Result.Ok, "");
                }
                else
                {
                    ret = new CompilationResult(null, Result.Fail, $"\nWine returned exit code {result}.\nOutput:\n{output}");
                }
            }
            catch (Exception e)
            {
                ret = new CompilationResult(null, Result.Fail, $"\n{e.Message}");
            }

            File.Delete(srcPath);
            File.Delete(dstPath);

            if (ret.ResultCode != Result.Ok)
            {
                throw new Exception($"Failed to compile shader!\n{ret.Message}");
            }

            return ret;
        }
    }
}
