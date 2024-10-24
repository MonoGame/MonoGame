
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

        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(context.NuGetsDirectory.FullPath), $"nuget-{os}");
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "Tests", "Tools", "Release")), $"tests-tools-{os}");
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "Tests", "DesktopGL", "Release")), $"tests-desktopgl-{os}");
        if (context.IsRunningOnWindows())
            await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "Tests", "WindowsDX", "Release")), $"tests-windowsdx-{os}");
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(System.IO.Path.Combine(context.BuildOutput, "MonoGame.Templates.VSExtension", "net472")), $"MonoGame.Templates.VSExtension.vsix");
    }
}
