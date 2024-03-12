
namespace BuildScripts;

[TaskName("DownloadArtifacts")]
public sealed class DownloadArtifactsTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override async Task RunAsync(BuildContext context)
    {
        foreach (var os in new[] { "windows", "mac", "linux" })
        {
            var artifactDir = $"nuget-{os}";
            context.CreateDirectory(artifactDir);
            await context.GitHubActions().Commands.DownloadArtifact(artifactDir, "nugets");
            context.CopyDirectory(artifactDir, "nugets");
        }
    }
}
