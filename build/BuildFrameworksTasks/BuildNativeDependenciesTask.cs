
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
        configureArgs.Append("-DSDL_STATIC=ON -DSDL_TEST=OFF");

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


        var faudioSourceDir = "native/monogame/external/faudio";
        var faudioBuildDir = System.IO.Path.Combine(faudioSourceDir, "build");
        if (context.DirectoryExists(faudioBuildDir))
        {
            context.DeleteDirectory(faudioBuildDir, new DeleteDirectorySettings { Recursive = true });
        }


        DirectoryPath sdlIncludeDir = System.IO.Path.Combine(sdlSourceDir, "include");

        context.CreateDirectory(faudioBuildDir);
        var faudioConfigureSettings = new ProcessSettings { WorkingDirectory = faudioBuildDir };
        var faudioConfigureArgs = new ProcessArgumentBuilder();
        faudioConfigureArgs.Append("../");
        faudioConfigureArgs.Append("-DBUILD_SHARED_LIBS=OFF");
        faudioConfigureArgs.Append($"-DCMAKE_C_FLAGS=\"/I{context.MakeAbsolute(sdlIncludeDir)}\"");
        faudioConfigureArgs.Append($"-DCMAKE_CXX_FLAGS=\"/I{context.MakeAbsolute(sdlIncludeDir)}\"");
        faudioConfigureArgs.Append("-DBUILD_SDL3=OFF");

        // Append platform-specific CMake arguments.
        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                faudioConfigureArgs.Append("-A x64");
                faudioConfigureArgs.Append("-D CMAKE_MSVC_RUNTIME_LIBRARY=MultiThreaded");
                break;
            case PlatformFamily.Linux:
                faudioConfigureArgs.Append("-D CMAKE_POSITION_INDEPENDENT_CODE=ON");
                break;
            case PlatformFamily.OSX:
                faudioConfigureArgs.Append("-D CMAKE_OSX_ARCHITECTURES=\"x86_64;arm64\"");
                faudioConfigureArgs.Append("-D CMAKE_OSX_DEPLOYMENT_TARGET=10.15");
                break;
        }
        faudioConfigureSettings.Arguments = faudioConfigureArgs;

        if (context.StartProcess("cmake", faudioConfigureSettings) != 0)
        {
            throw new Exception("FAudio CMake configuration failed!");
        }
        var faudioBuildSettings = new ProcessSettings { WorkingDirectory = faudioBuildDir };
        var faudioBuildArgs = new ProcessArgumentBuilder();
        faudioBuildArgs.Append("--build .");
        faudioBuildArgs.Append("--config Release");
        faudioBuildArgs.Append("--parallel");
        faudioBuildSettings.Arguments = faudioBuildArgs;

        if (context.StartProcess("cmake", faudioBuildSettings) != 0)
        {
            throw new Exception("FAudio build failed!");
        }
    }
}
