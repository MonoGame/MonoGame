
using System.Runtime.InteropServices;

namespace BuildScripts;

public sealed class BuildPremake
{
    public void Run(BuildContext context, string name, string workingDirectory, string solutionFile)
    {
        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
            {
                // Generate multi-arch solution in one go.
                Scaffold(context, name, workingDirectory, "--verbose vs2022");

                // Build for both architectures.
                BuildForArch(context, name, workingDirectory, solutionFile, "x64");
                BuildForArch(context, name, workingDirectory, solutionFile, "ARM64");
                
                break;
            }
            case PlatformFamily.Linux:
            case PlatformFamily.OSX:
            {
                // Linux/macOS build for the host architecture only
                var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
                Scaffold(context, name, workingDirectory, $"--arch={arch} gmake2");
                Make(context, name, workingDirectory);

                break;
            }
            default:
            {
                throw new NotSupportedException($"Platform {context.Environment.Platform.Family} is not supported for building the {name}.");
            }
        }
    }

    private void Scaffold(BuildContext context, string name, string workingDirectory, string premakeArguments)
    {
        int exit;
        exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = workingDirectory, Arguments = "clean" });
        if (exit != 0)
        {
            throw new Exception($"{name} Premake clean failed! {exit}");
        }

        exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = workingDirectory, Arguments = premakeArguments });
        if (exit != 0)
        {
            throw new Exception($"{name} Premake generation failed! {exit}");
        }
    }

    private void BuildForArch(BuildContext context, string name, string workingDirectory, string solutionFile, string arch)
    {
        int exit = context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = workingDirectory, Arguments = $"{solutionFile} /p:Configuration=Release /p:Platform={arch}" });
        if (exit != 0)
        {
            throw new Exception($"{name} build failed with msbuild! {exit}");
        }
    }

    private void Make(BuildContext context, string name, string workingDirectory)
    {
        int exit = context.StartProcess("make", new ProcessSettings { WorkingDirectory = workingDirectory, Arguments = "config=release" });
        if (exit != 0)
        {
            throw new Exception($"{name} build failed with make! {exit}");
        }
    }
}
