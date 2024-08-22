
namespace BuildScripts;

[TaskName("DeployNuGetsToNuGetOrgTask")]
[IsDependentOn(typeof(DownloadArtifactsTask))]
public sealed class DeployNuGetsToNuGetOrgTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            var workflow = context.BuildSystem().GitHubActions.Environment.Workflow;
            if (workflow.RefType == GitHubActionsRefType.Tag &&
                !string.IsNullOrWhiteSpace(context.EnvironmentVariable("NUGET_API_KEY")))
            {
                return true;
            }
        }

        return false;
    }

    public override void Run(BuildContext context)
    {
        context.DotNetNuGetPush($"nugets/*.nupkg", new()
        {
            ApiKey = context.EnvironmentVariable("NUGET_API_KEY"),
            Source = $"https://api.nuget.org/v3/index.json"
        });
    }
}
