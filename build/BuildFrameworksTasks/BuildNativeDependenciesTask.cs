
namespace BuildScripts;

[TaskName("Build Native Dependencies")]
public sealed class BuildNativeDependenciesTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var sdlSourceDir = "native/monogame/external/sdl2/sdl";
        var sdlBuildDir = System.IO.Path.Combine(sdlSourceDir, "build");

        if (context.DirectoryExists(sdlBuildDir))
        {
            context.DeleteDirectory(sdlBuildDir, new DeleteDirectorySettings { Recursive = true });
        }
        context.CreateDirectory(sdlBuildDir);

        var configureSettings = new ProcessSettings { WorkingDirectory = sdlBuildDir };
        var configureArgs = new ProcessArgumentBuilder();
        // Add the relative path to the source directory.
        configureArgs.Append("../");
        configureArgs.Append("-DSDL_STATIC=ON");

        // Append platform-specific CMake arguments.
        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                configureArgs.Append("-A x64");
                configureArgs.Append("-D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded");
                break;
            case PlatformFamily.Linux:
                configureArgs.Append("-D CMAKE_POSITION_INDEPENDENT_CODE=ON");
                break;
            case PlatformFamily.OSX:
                configureArgs.Append("-D CMAKE_OSX_ARCHITECTURES=\"x86_64;arm64\"");
                configureArgs.Append("-D CMAKE_OSX_DEPLOYMENT_TARGET=10.15");
                break;
        }

        configureSettings.Arguments = configureArgs;
        
        if (context.StartProcess("cmake", configureSettings) != 0)
        {
            throw new Exception("SDL2 CMake configuration failed!");
        }

        var buildSettings = new ProcessSettings { WorkingDirectory = sdlBuildDir };
        var buildArgs = new ProcessArgumentBuilder();
        buildArgs.Append("--build .");
        buildArgs.Append("--config Release");
        buildArgs.Append("--parallel");

        buildSettings.Arguments = buildArgs;
        
        if (context.StartProcess("cmake", buildSettings) != 0)
        {
            throw new Exception("SDL2 build failed!");
        }
    }
}
