
namespace BuildScripts;

[TaskName("DeployBinaries")]
[IsDependentOn(typeof(DownloadBinariesTask))]
public sealed class DeployBinariesTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override async Task RunAsync(BuildContext context)
    {
        await context.GitHubActions().Commands.UploadArtifact(new DirectoryPath("Artifacts/binPackaging"), $"MonoGame.{context.Version}");
    }
}
