
namespace BuildScripts;

[TaskName("DeployNuGetsToGithub")]
[IsDependentOn(typeof(RepackForDeployTask))]
public sealed class DeployNuGetsToGitHubTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override async Task RunAsync(BuildContext context)
    {
        var actions = context.GitHubActions();
        context.Information($"Uploading nugets from {context.NuGetsDirectory}");
        context.DotNetNuGetPush($"{context.NuGetsDirectory}*.nupkg", new()
        {
            ApiKey = context.EnvironmentVariable("GITHUB_TOKEN"),
            Source = $"https://nuget.pkg.github.com/{actions.Environment.Workflow.RepositoryOwner}/index.json"
        });
        await actions.Commands.UploadArtifact(new DirectoryPath(context.NuGetsDirectory), $"nuget-{context.Version}");
    }
}
