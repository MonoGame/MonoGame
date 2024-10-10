
namespace BuildScripts;

[TaskName("UploadArtifacts")]
public sealed class UploadArtifactsTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override async Task RunAsync(BuildContext context)
    {
        var os = context.Environment.Platform.Family switch
        {
            PlatformFamily.Windows => "windows",
            PlatformFamily.OSX => "macos",
            _ => "linux"
        };

        // Clean up build tools if installed
        // otherwise we get permission issues after extraction
        var path = System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release", "tools");
        if (System.IO.Directory.Exists(path)) {
            context.Log.Information ($"Deleting: {path}");
            System.IO.Directory.Delete (path, recursive: true);
        }
        if (context.IsRunningOnMacOs())
        {
            path = System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release", "osx");
            DeleteToolStore (context, path);
        }
        if (context.IsRunningOnLinux())
        {
            path = System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release", "linux");
            DeleteToolStore (context, path);
        }
        if (context.IsRunningOnWindows())
        {
            path = System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release");
            DeleteToolStore (context, path);
        }
       

        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(context.NuGetsDirectory.FullPath), $"nuget-{os}");
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release")), $"tests-tools-{os}");
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "Tests", "DesktopGL", "Release")), $"tests-desktopgl-{os}");
        if (context.IsRunningOnWindows())
            await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "Tests", "WindowsDX", "Release")), $"tests-windowsdx-{os}");
    }

    void DeleteToolStore (BuildContext context, string path)
    {
        if (System.IO.Directory.Exists (path)) {
            var store = System.IO.Path.Combine (path, ".store");
            if (System.IO.Directory.Exists (store)) {
                context.Log.Information ($"Deleting: {store}");
                System.IO.Directory.Delete (store, recursive: true);
                foreach (var file in System.IO.Directory.GetFiles (path, "mgcb-*", System.IO.SearchOption.TopDirectoryOnly)) {
                    context.Log.Information ($"Deleting: {file}");
                    System.IO.File.Delete (file);
                }
            }
        }
    }
}
