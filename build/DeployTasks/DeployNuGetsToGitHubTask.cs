
namespace BuildScripts;

[TaskName("DeployNuGetsToGithub")]
[IsDependentOn(typeof(PackContentPipelineTask))]
public sealed class DeployNuGetsToGitHubTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override void Run(BuildContext context)
    {
        var repositoryOwner = context.GitHubActions().Environment.Workflow.RepositoryOwner;
        context.DotNetNuGetPush($"nugets/*.nupkg", new()
        {
            ApiKey = context.EnvironmentVariable("GITHUB_TOKEN"),
            Source = $"https://nuget.pkg.github.com/{repositoryOwner}/index.json"
        });
    }
}
