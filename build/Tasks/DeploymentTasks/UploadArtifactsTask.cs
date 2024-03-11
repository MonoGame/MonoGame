using Cake.Common.Build;
using Cake.Common.Build.GitHubActions;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System;
using System.Threading.Tasks;

namespace MonoGame.Framework.Build.Tasks.DeploymentTasks;

[TaskName("UploadArtifacts")]
public class UploadArtifactsTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.BuildSystem().IsRunningOnGitHubActions;
    }

    public override async Task RunAsync(BuildContext context)
    {
        string os = context.Environment.Platform.Family switch
        {
            PlatformFamily.Windows => "windows",
            PlatformFamily.OSX => "mac",
            PlatformFamily.Linux => "linux",
            _ => throw new InvalidOperationException("Unknown platform")
        };

        IGitHubActionsProvider github = context.GitHubActions();
        DirectoryPath nugets = new DirectoryPath(context.NuGetsDirectory);
        await github.Commands.UploadArtifact(nugets, $"nuget-{os}");
    }
}
