namespace BuildScripts;

[TaskName("Build Native Dependencies")]
public sealed class BuildNativeDependenciesTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        BuildSDL2(context);
        BuildFAudio(context);
    }

    private void BuildSDL2(BuildContext context)
    {
        var sdlSourceDir = "native/monogame/external/sdl2/sdl";
        var sdlBuildDir = System.IO.Path.Combine(sdlSourceDir, "build");

        RecreateDirectory(context, sdlBuildDir);

        var configureArgs = new ProcessArgumentBuilder()
            .Append("-S").AppendQuoted(context.MakeAbsolute(new DirectoryPath(sdlSourceDir)).FullPath)
            .Append("-B").AppendQuoted(context.MakeAbsolute(new DirectoryPath(sdlBuildDir)).FullPath)
            .Append("-DSDL_STATIC=ON")
            .Append("-DSDL_TEST=OFF");

        AppendPlatformCMakeArgs(configureArgs, context, isSDL: true);

        RunCMake(context, configureArgs, "SDL2 CMake configuration failed!");

        RunCMakeBuild(context, sdlBuildDir, "Release", "SDL2 build failed!");
    }

    private void BuildFAudio(BuildContext context)
    {
        var faudioSourceDir = "native/monogame/external/faudio";
        var faudioBuildDir = System.IO.Path.Combine(faudioSourceDir, "build");

        RecreateDirectory(context, faudioBuildDir);

        var sdlIncludeDir = System.IO.Path.Combine("native/monogame/external/sdl2/sdl", "include");

        var configureArgs = new ProcessArgumentBuilder()
            .Append("-S").AppendQuoted(context.MakeAbsolute(new DirectoryPath(faudioSourceDir)).FullPath)
            .Append("-B").AppendQuoted(context.MakeAbsolute(new DirectoryPath(faudioBuildDir)).FullPath)
            .Append("-DBUILD_SHARED_LIBS=OFF")
            .Append($"-DCMAKE_C_STANDARD_INCLUDE_DIRECTORIES={context.MakeAbsolute(new DirectoryPath(sdlIncludeDir))}")
            .Append($"-DCMAKE_CXX_STANDARD_INCLUDE_DIRECTORIES={context.MakeAbsolute(new DirectoryPath(sdlIncludeDir))}")
            .Append("-DBUILD_SDL3=OFF");

        AppendPlatformCMakeArgs(configureArgs, context, isSDL: false);

        RunCMake(context, configureArgs, "FAudio CMake configuration failed!");

        RunCMakeBuild(context, faudioBuildDir, "Release", "FAudio build failed!");
    }

    private void AppendPlatformCMakeArgs(ProcessArgumentBuilder args, BuildContext context, bool isSDL)
    {
        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                args.Append("-A").Append("x64");
                if (isSDL)
                {
                    args.Append("-DSDL_FORCE_STATIC_VCRT=ON");
                }
                else
                {
                    args.Append("-DCMAKE_C_FLAGS_DEBUG=\"/MTd /Zi /Ob0 /Od /RTC1\"");
                    args.Append("-DCMAKE_C_FLAGS_RELEASE=\"/MT /O2 /Ob2 /DNDEBUG\"");
                }
                break;

            case PlatformFamily.Linux:
                args.Append("-DCMAKE_POSITION_INDEPENDENT_CODE=ON");
                break;

            case PlatformFamily.OSX:
                args.Append("-DCMAKE_OSX_ARCHITECTURES=x86_64;arm64");
                args.Append("-DCMAKE_OSX_DEPLOYMENT_TARGET=10.15");
                break;
        }
    }

    private void RunCMake(BuildContext context, ProcessArgumentBuilder args, string errorMessage)
    {
        var settings = new ProcessSettings { Arguments = args };
        if (context.StartProcess("cmake", settings) != 0)
        {
            throw new Exception(errorMessage);
        }
    }

    private void RunCMakeBuild(BuildContext context, string buildDir, string config, string errorMessage)
    {
        var buildArgs = new ProcessArgumentBuilder()
            .Append("--build")
            .AppendQuoted(context.MakeAbsolute(new DirectoryPath(buildDir)).FullPath)
            .Append("--config").Append(config)
            .Append("--parallel");

        RunCMake(context, buildArgs, errorMessage);
    }

    private void RecreateDirectory(BuildContext context, string dir)
    {
        if (context.DirectoryExists(dir))
        {
            context.DeleteDirectory(dir, new DeleteDirectorySettings { Recursive = true });
        }
        context.CreateDirectory(dir);
    }
}
