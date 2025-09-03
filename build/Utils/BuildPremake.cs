
namespace BuildScripts;

public sealed class BuildPremake
{
    public void Run(BuildContext context, string name, string workingDirectory, string solutionFile)
    {
        int exit;
        exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = workingDirectory, Arguments = "clean" });
        if (exit != 0)
            throw new Exception($"{name} Premake clean failed! {exit}");

        string? premakeArguments;
        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                premakeArguments = "--verbose vs2022";
                break;
            case PlatformFamily.Linux or PlatformFamily.OSX:
                premakeArguments = "gmake2";
                break;
            default:
                throw new NotSupportedException($"Platform {context.Environment.Platform.Family} is not supported for building the {name}.");
        }

        exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = workingDirectory, Arguments = premakeArguments });
        if (exit != 0)
            throw new Exception($"{name} Premake generation failed! {exit}");

        if (context.Environment.Platform.Family == PlatformFamily.Windows)
        {
            exit = context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = workingDirectory, Arguments = $"{solutionFile} /p:Configuration=Release /p:Platform=x64" });
            if (exit != 0)
                throw new Exception($"{name} build failed with msbuild! {exit}");
        }
        else
        {
            exit = context.StartProcess("make", new ProcessSettings { WorkingDirectory = workingDirectory, Arguments = "config=release" });
            if (exit != 0)
                throw new Exception($"{name} build failed with make! {exit}");
        }
    }
}
