
namespace BuildScripts;

[TaskName("DeployVsixToGitHubTask")]
[IsDependentOn(typeof(DownloadArtifactsTask))]
public sealed class DeployVsixToGitHubTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override void Run(BuildContext context)
    {
        var repositoryOwner = context.GitHubActions().Environment.Workflow.RepositoryOwner;
        context.DotNetNuGetPush($"vsix/*.vsix", new()
        {
            ApiKey = context.EnvironmentVariable("GITHUB_TOKEN"),
            Source = $"https://nuget.pkg.github.com/{repositoryOwner}/index.json"
        });
    }
}
