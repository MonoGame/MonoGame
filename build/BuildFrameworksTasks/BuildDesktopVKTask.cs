
namespace BuildScripts;

[TaskName("Build DesktopVK")]
public sealed class BuildDesktopVKTask : FrostingTask<BuildContext>
{
    // TEMP: Until OSX and Linux is setup to work.
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        // TODO: This should be moved to its own seperate
        // build process so that it doesn't run on all MG builds.
        {
            var buildDir = "src/monogame/external/sdl2/sdl/build";
            context.CreateDirectory(buildDir);
            context.StartProcess("cmake", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "-A x64 -D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded ../" });
            context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = buildDir, Arguments = "SDL2.sln /p:Configuration=Release /p:Platform=x64" });
        }

        // Generate the native projects.
        var exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = "src/monogame", Arguments = "clean" });
        if (exit < 0)
            throw new Exception($"Setting Premake clean failed! {exit}");
        exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = "src/monogame", Arguments = "--os=windows --verbose vs2022" });
        if (exit < 0)
            throw new Exception($"Setting Premake generation failed! {exit}");

        // Build it.
        exit = context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = "src/monogame", Arguments = "monogame.sln /p:Configuration=Release /p:Platform=x64" });
        if (exit < 0)
            throw new Exception($"Setting build failed! {exit}");
    }
}
