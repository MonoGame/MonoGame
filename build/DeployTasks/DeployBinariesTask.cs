
namespace BuildScripts;

[TaskName("DeployBinaries")]
[IsDependentOn(typeof(DownloadBinariesTask))]
public sealed class DeployBinariesTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override async Task RunAsync(BuildContext context)
    {
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("binaries"), $"MonoGame.{context.Version}");
    }
}
