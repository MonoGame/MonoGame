using System;
using System.Runtime.InteropServices;

namespace BuildScripts;

/// <summary>
/// Cake Context Extensions to provide support for building libraries or native
/// tools in a custom monogame docker image based on steams sniper image
/// for maximum linux compatability
/// </summary>
public static class ProcessDockerExtensions
{
    const string DOCKERIMAGE_X64 = "ghcr.io/monogame/steamrt-sniper-premake:v5.0.0-beta8-amd64";
    const string DOCKERIMAGE_ARM64 = "ghcr.io/monogame/steamrt-sniper-premake:v5.0.0-beta8-arm64";
    private static bool HasDocker(BuildContext context)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Environment.GetEnvironmentVariable ("CI") == "true")
        {
            var args = new ProcessArgumentBuilder();
            args.Append("docker");
            var settings = new ProcessSettings { Arguments = args };
            if (context.StartProcess("which", settings) == 0)
            {
                return true;
            }
        }
        return false;
    }

    public static int StartProcessWithDocker(this BuildContext context,  string command, string workingDirectory, ProcessArgumentBuilder args, string volumeMount = "")
    {
        var hasDocker = HasDocker(context);
        if (hasDocker)
        {
            var workdir = "/src";
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                workdir += $"/{workingDirectory}";
            }
            args.Prepend(command);
            args.Prepend(RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? DOCKERIMAGE_ARM64 : DOCKERIMAGE_X64);
            args.Prepend(workdir);
            args.Prepend("-w");
            args.Prepend($"{System.IO.Path.GetFullPath(".")}:/src");
            args.Prepend("-v");
            if (!string.IsNullOrEmpty(volumeMount))
            {
                args.Prepend("type=bind,source=\"$PWD/.tools\",target=/tools");
                args.Prepend("--mount");
            }
            args.Prepend("run");
            command = "docker";
        }
        var settings = new ProcessSettings { Arguments = args, WorkingDirectory = workingDirectory };
        settings.NoWorkingDirectory = !hasDocker;
        return context.StartProcess(command, settings);
    }

    public static int StartProcessWithDocker(this BuildContext context,  string command, ProcessSettings settings)
    {
        var hasDocker = HasDocker(context);
        return context.StartProcess(command, settings);
    }

    public static string MakeAbsolute(this BuildContext context)
    {
        return string.Empty;
    }
}