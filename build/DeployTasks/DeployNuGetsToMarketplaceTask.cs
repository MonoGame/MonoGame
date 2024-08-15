
namespace BuildScripts;

[TaskName("DeployNuGetsToMarketplace")]
[IsDependentOn(typeof(DownloadArtifactsTask))]
public sealed class DeployNuGetsToMarketplace : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override void Run(BuildContext context)
    {
        var repositoryOwner = context.GitHubActions().Environment.Workflow.RepositoryOwner;
        context.DotNetNuGetPush($"nugets/*.nupkg", new()
        {
            ApiKey = context.EnvironmentVariable("GITHUB_TOKEN"),
            Source = $"https://api.nuget.org/v3/index.json"
        });
    }
}