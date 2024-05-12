﻿
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
            PlatformFamily.OSX => "mac",
            _ => "linux"
        };

        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath(context.NuGetsDirectory.FullPath), $"nuget-{os}");
    }
}
