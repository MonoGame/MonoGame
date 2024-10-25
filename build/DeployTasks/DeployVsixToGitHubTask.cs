
namespace BuildScripts;

[TaskName("DeployVsixToGitHubTask")]
[IsDependentOn(typeof(DownloadArtifactsTask))]
public sealed class DeployVsixToGitHubTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override void Run(BuildContext context)
    {
        var filePath = "vsix/MonoGame.Templates.VSExtension.vsix";
        if (!File.Exists(filePath))
        {
            context.Error("VSIX file not found!");
            return;
        }

        var repositoryOwner = context.GitHubActions().Environment.Workflow.RepositoryOwner;
        context.DotNetNuGetPush(filePath, new()
        {
            ApiKey = context.EnvironmentVariable("GITHUB_TOKEN"),
            Source = $"https://nuget.pkg.github.com/{repositoryOwner}/index.json"
        });
    }
}
