
namespace BuildScripts;

[TaskName("DeployVsixToMarketplaceTask")]
[IsDependentOn(typeof(DownloadArtifactsTask))]
public sealed class DeployVsixToMarketplaceTask : FrostingTask<BuildContext>
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
        // Confirm for testing purposes the fil is there.
        context.Information("Files in 'vsix' directory:");
        foreach (var file in context.GetFiles("vsix/*"))
        {
            context.Information(file.FullPath);
        }

        context.DotNetNuGetPush($"vsix/*.vsix", new()
        {
            ApiKey = context.EnvironmentVariable("NUGET_API_KEY"),
            Source = $"https://api.nuget.org/v3/index.json"
        });
    }
}
