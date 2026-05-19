
namespace BuildScripts;

[TaskName("DownloadTestArtifacts")]
public sealed class DownloadTestArtifactsTask : AsyncFrostingTask<BuildContext>
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
        context.CreateDirectory("tests-tools");
        await context.GitHubActions().Commands.DownloadArtifact($"tests-tools-{os}", "tests-tools");
        await context.GitHubActions().Commands.DownloadArtifact($"tests-desktopgl-{os}", "tests-desktopgl");
        if (context.IsRunningOnWindows())
            await context.GitHubActions().Commands.DownloadArtifact($"tests-windowsdx-{os}", "tests-windowsdx");
    }
}
