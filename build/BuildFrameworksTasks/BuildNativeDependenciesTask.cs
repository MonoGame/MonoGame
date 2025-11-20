namespace BuildScripts;

[TaskName("Build Native Dependencies")]
public sealed class BuildNativeDependenciesTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        BuildSDL2(context);
        BuildFAudio(context);
        if (context.IsRunningOnWindows())
            return;
        BuildSDL2ForEmscripten(context);
        BuildFAudioForEmscripten(context);
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

    void BuildSDL2ForEmscripten(BuildContext context)
    {
        var sdlSourceDir = "native/monogame/external/sdl2/sdl";
        var sdlBuildDir = System.IO.Path.Combine(sdlSourceDir, "build_emscripten");
        
        RecreateDirectory(context, sdlBuildDir);

        var configureSettings = new ProcessSettings { WorkingDirectory = sdlBuildDir };
        SetupEmscriptenEnvironment(context, configureSettings);
        var configureArgs = new ProcessArgumentBuilder();
        // Add the relative path to the source directory.
        configureArgs.Append("cmake");
        configureArgs.Append("../");
        configureArgs.Append("-DSDL_STATIC=ON -DSDL_TEST=OFF");
        configureArgs.Append($"-D CMAKE_BUILD_TYPE=Release");

        configureSettings.Arguments = configureArgs;

        var emcmake = context.IsRunningOnWindows() ? "emcmake.bat" : "emcmake";

        if (context.StartProcess(emcmake, configureSettings) != 0)
        {
            throw new Exception("SDL2 Emscripten CMake configuration failed!");
        }

        var buildSettings = new ProcessSettings { WorkingDirectory = sdlBuildDir };
        SetupEmscriptenEnvironment(context, buildSettings);
            
        var buildArgs = new ProcessArgumentBuilder();
        buildArgs.Append("make");

        buildSettings.Arguments = buildArgs;

        var emmake = context.IsRunningOnWindows() ? "emmake.bat" : "emmake";

        if (context.StartProcess(emmake, buildSettings) != 0)
        {
            throw new Exception("SDL2 Emscripten build failed!");
        }

        var sourcePath = context.GetOutputPath($"Artifacts/monogame.native/emscripten/wasm/{context.BuildConfiguration}");

        if (!context.DirectoryExists(sourcePath))
        {
            context.CreateDirectory(sourcePath);
        }

        context.CopyFile(
            System.IO.Path.Combine(sdlBuildDir, "libSDL2.a"),
            System.IO.Path.Combine(sourcePath, "libSDL2.a"));
    }

    void BuildFAudioForEmscripten(BuildContext context)
    {
        var faudioSourceDir = "native/monogame/external/faudio";
        var faudioBuildDir = System.IO.Path.Combine(faudioSourceDir, "build_emscripten");

        RecreateDirectory(context, faudioBuildDir);

        var sdlIncludeDir = System.IO.Path.Combine("native/monogame/external/sdl2/sdl", "include");
        var sdlBuildDir = System.IO.Path.Combine("native/monogame/external/sdl2/sdl", "build_emscripten");
        var sdlLibPath = System.IO.Path.Combine(sdlBuildDir, "libSDL2.a");

        var configureSettings = new ProcessSettings { WorkingDirectory = faudioBuildDir };
        SetupEmscriptenEnvironment(context, configureSettings);
        var configureArgs = new ProcessArgumentBuilder();
        // Add the relative path to the source directory.
        configureArgs.Append("cmake");
        configureArgs.Append("../");
        configureArgs.Append("-DBUILD_SHARED_LIBS=OFF");
        configureArgs.Append($"-DSDL2_INCLUDE_DIRS={context.MakeAbsolute(new DirectoryPath(sdlIncludeDir))}");
        configureArgs.Append($"-DSDL2_LIBRARIES={context.MakeAbsolute(new FilePath(sdlLibPath))}");
        configureArgs.Append($"-DCMAKE_C_STANDARD_INCLUDE_DIRECTORIES={context.MakeAbsolute(new DirectoryPath(sdlIncludeDir))}");
        configureArgs.Append($"-DCMAKE_CXX_STANDARD_INCLUDE_DIRECTORIES={context.MakeAbsolute(new DirectoryPath(sdlIncludeDir))}");
        configureArgs.Append("-DBUILD_SDL3=OFF");
        configureArgs.Append($"-D CMAKE_BUILD_TYPE=Release");

        configureSettings.Arguments = configureArgs;

        var emcmake = context.IsRunningOnWindows() ? "emcmake.bat" : "emcmake";

        if (context.StartProcess(emcmake, configureSettings) != 0)
        {
            throw new Exception("FAudio Emscripten CMake configuration failed!");
        }

        var buildSettings = new ProcessSettings { WorkingDirectory = faudioBuildDir };
        SetupEmscriptenEnvironment(context, buildSettings);
        var buildArgs = new ProcessArgumentBuilder();
        buildArgs.Append("make");

        buildSettings.Arguments = buildArgs;

        var emmake = context.IsRunningOnWindows() ? "emmake.bat" : "emmake";
        
        if (context.StartProcess(emmake, buildSettings) != 0)
        {
            throw new Exception("FAudio Emscripten build failed!");
        }

        var sourcePath = context.GetOutputPath($"Artifacts/monogame.native/emscripten/wasm/{context.BuildConfiguration}");

        if (!context.DirectoryExists(sourcePath))
        {
            context.CreateDirectory(sourcePath);
        }

        context.CopyFile(
            System.IO.Path.Combine(faudioBuildDir, "libFAudio.a"),
            System.IO.Path.Combine(sourcePath, "libFAudio.a"));
    }

    private void SetupEmscriptenEnvironment(BuildContext context, ProcessSettings settings)
    {
        var emSdkDir = System.Environment.GetEnvironmentVariable("EMSDK") ?? string.Empty;
        var emscriptenDir = System.IO.Path.Combine(emSdkDir, "upstream", "emscripten");
        var nodeDir =  System.Environment.GetEnvironmentVariable("EMSDK_NODE") ?? string.Empty;
        var pythonDir = System.Environment.GetEnvironmentVariable("EMSDK_PYTHON") ?? string.Empty;
        var llvmBin =   System.IO.Path.Combine(emSdkDir, "upstream", "bin");
        settings.EnvironmentVariables = new Dictionary<string, string>()
        {
            { "EMSDK", emSdkDir },
            { "EMSDK_NODE", nodeDir },
            { "EMSDK_PYTHON", pythonDir },
            { "EMSCRIPTEN", emscriptenDir },
            { "PATH", $"{emSdkDir};{emscriptenDir};{llvmBin};{nodeDir};{pythonDir};{System.Environment.GetEnvironmentVariable("PATH")}" }
        };
        if (!context.IsRunningOnWindows())
        {
            settings.EnvironmentVariables["PATH"] = $"{emSdkDir}:{emscriptenDir}:{llvmBin}:{nodeDir}:{pythonDir}:{System.Environment.GetEnvironmentVariable("PATH")}";
        }

        context.Information("Emscripten Environment Variables:");
        foreach (var kvp in settings.EnvironmentVariables)
        {
            context.Information($"{kvp.Key}={kvp.Value}");
        }
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
