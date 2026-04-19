
namespace BuildScripts;

[TaskName("UploadArtifacts")]
public sealed class UploadArtifactsTask : AsyncFrostingTask<BuildContext>
{
    private static readonly int DefaultTimeoutInSeconds = 100;
    private readonly Func<string, HttpClient> _httpClientFactoryFunction;

    public UploadArtifactsTask(Func<string, HttpClient> httpClientFactoryFunction)
    {
        _httpClientFactoryFunction = httpClientFactoryFunction;
    }

    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override async Task RunAsync(BuildContext context)
    {
        LogHttpClientTimeout(context);

        var os = context.Environment.Platform.Family switch
        {
            PlatformFamily.Windows => "windows",
            PlatformFamily.OSX => "macos",
            _ => "linux"
        };

        // Clean up build tools if installed
        // otherwise we get permission issues after extraction
        // because the zip removes all the permissions.
        // Plus in windows hidden files (like the .store directory)
        // are ignored. This causes `dotnet tool` to error.
        var path = System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release", "dotnet-tools");
        if (System.IO.Directory.Exists(path))
        {
            context.Log.Information($"Deleting: {path}");
            System.IO.Directory.Delete(path, recursive: true);
        }
        if (context.IsRunningOnMacOs())
        {
            path = System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release", "osx");
            DeleteToolStore(context, path);
        }
        if (context.IsRunningOnLinux())
        {
            path = System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release", "linux");
            DeleteToolStore(context, path);
        }
        if (context.IsRunningOnWindows())
        {
            path = System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release");
            DeleteToolStore(context, path);
        }

        // Upload mgpipeline native libraries
        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("Artifacts/native/mgpipeline/windows/Release/"), $"mgpipeline-{os}.{context.Version}");
                await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("Artifacts/native/mgruntime/windowsdx/windows/"), $"mgnative-{os}.{context.Version}");
                break;
            case PlatformFamily.Linux:
                await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("Artifacts/native/mgpipeline/linux/Release/"), $"mgpipeline-{os}.{context.Version}");
                //await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("Artifacts/monogame.native/linux/"), $"mgnative-{os}.{context.Version}");
                break;
            case PlatformFamily.OSX:
                await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("Artifacts/native/mgpipeline/macosx/Release/"), $"mgpipeline-{os}.{context.Version}");
                //await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("Artifacts/monogame.native/macosx/"), $"mgnative-{os}.{context.Version}");
                break;
            default:
                throw new NotSupportedException($"Platform {context.Environment.Platform.Family} is not supported for static library checks.");
        }

        // Upload Binaries
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("Artifacts/MonoGame.Framework/"), $"mgframework-{os}.{context.Version}");
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("Artifacts/Binaries/"), $"mgbinaries-{os}.{context.Version}");

        // Upload NuGet packages
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(context.NuGetsDirectory), $"nuget-{os}.{context.Version}");
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "Tests", "DesktopGL", "Release")), $"tests-desktopgl-{os}");
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "Tests", "DesktopVK", "Release")), $"tests-desktopvk-{os}");
        if (context.IsRunningOnWindows())
        {
            await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "Tests", "WindowsDX", "Release")), $"tests-windowsdx-{os}");

            // Assuming that the .vsix file has already been created and is located at this exact path.
            var vsixFilePath = System.IO.Path.Combine(context.BuildOutput, "MonoGame.Templates.VSExtension", "net472", "MonoGame.Templates.VSExtension.vsix");
            await context.GitHubActions().Commands.UploadArtifact(new FilePath(vsixFilePath), $"MonoGame.Templates.VSExtension.{context.Version}.vsix");
        }
    }

    void DeleteToolStore(BuildContext context, string path)
    {
        if (System.IO.Directory.Exists(path))
        {
            var store = System.IO.Path.Combine(path, ".store");
            if (System.IO.Directory.Exists(store))
            {
                context.Log.Information($"Deleting: {store}");
                System.IO.Directory.Delete(store, recursive: true);
                foreach (var file in System.IO.Directory.GetFiles(path, "mgcb-*", System.IO.SearchOption.TopDirectoryOnly))
                {
                    context.Log.Information($"Deleting: {file}");
                    System.IO.File.Delete(file);
                }
                foreach (var file in System.IO.Directory.GetFiles(path, "tools_version.txt", System.IO.SearchOption.TopDirectoryOnly))
                {
                    context.Log.Information($"Deleting: {file}");
                    System.IO.File.Delete(file);
                }

            }
        }
    }

    private void LogHttpClientTimeout(BuildContext context)
    {
        var testClient = _httpClientFactoryFunction("HttpClient.Timeout");

        var timeoutInSeconds = testClient.Timeout.TotalSeconds;

        if (timeoutInSeconds > DefaultTimeoutInSeconds)
        {
            context.Log.Information($"HttpClient.Timeout is set at: {testClient.Timeout.TotalSeconds} seconds.");
        }
        else if (Math.Abs(timeoutInSeconds - DefaultTimeoutInSeconds) < 0.01)
        {
            context.Log.Warning($"HttpClient.Timeout is set at: {testClient.Timeout.TotalSeconds} seconds.");
        }
        else
        {
            context.Log.Error($"HttpClient.Timeout is set at: {testClient.Timeout.TotalSeconds} seconds.");
        }
    }
}
