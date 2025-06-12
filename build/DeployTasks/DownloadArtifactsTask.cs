
namespace BuildScripts;

[TaskName("DownloadArtifacts")]
public sealed class DownloadArtifactsTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override async Task RunAsync(BuildContext context)
    {
        var version = BuildContext.CalculateVersion(context);

        context.CreateDirectory("nugets");
        foreach (var os in new[] { "windows", "macos", "linux" })
            await context.GitHubActions().Commands.DownloadArtifact($"nuget-{os}.{version}", "nugets");

        context.CreateDirectory("vsix");
        await context.GitHubActions().Commands.DownloadArtifact($"MonoGame.Templates.VSExtension.{version}.vsix", "vsix");
    }
}
