using Cake.Common.Build;
using Cake.Common.Build.GitHubActions;
using Cake.Common.IO;
using Cake.Core.IO;
using Cake.Frosting;
using System.Threading.Tasks;

namespace MonoGame.Framework.Build.Tasks.DeploymentTasks;

[TaskName("DownloadArtifacts")]
public class DownloadArtifactsTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.BuildSystem().IsRunningOnGitHubActions;
    }

    public override async Task RunAsync(BuildContext context)
    {
        IGitHubActionsProvider github = context.GitHubActions();

        DirectoryPath nugetsDir = new DirectoryPath("nugets");

        string[] platforms = new string[] { "windows", "mac", "linux" };

        foreach (string os in platforms)
        {
            string artifactDir = $"nuget-{os}";
            context.CreateDirectory(artifactDir);
            await github.Commands.DownloadArtifact(artifactDir, artifactDir);
            context.CopyDirectory(artifactDir, nugetsDir);
        }
    }
}
