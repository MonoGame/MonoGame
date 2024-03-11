using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Build.GitHubActions;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Core.IO;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.DeploymentTasks;

[TaskName("DeployNuGetsToGithub")]
[IsDependentOn(typeof(DownloadArtifactsTask))]
public sealed class DeployNuGetsToGitHubTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.BuildSystem().IsRunningOnGitHubActions;
    }

    public override void Run(BuildContext context)
    {
        IGitHubActionsProvider github = context.GitHubActions();
        foreach (var nuget in context.GetFiles($"nugets/*.nupkg"))
        {
            context.DotNetNuGetPush(nuget, new()
            {
                ApiKey = context.EnvironmentVariable("GITHUB_TOKEN"),
                Source = $"https://nuget.pkg.github.com/{github.Environment.Workflow.RepositoryOwner}/index.json"
            });
        }
    }
}
