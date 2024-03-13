﻿
namespace BuildScripts;

[TaskName("DownloadArtifacts")]
public sealed class DownloadArtifactsTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override async Task RunAsync(BuildContext context)
    {
        context.CreateDirectory("nugets");
        foreach (var os in new[] { "windows", "mac", "linux" })
            await context.GitHubActions().Commands.DownloadArtifact($"nuget-{os}", "nugets");
    }
}
