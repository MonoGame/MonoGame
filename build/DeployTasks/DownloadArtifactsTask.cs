
namespace BuildScripts;

[TaskName("DownloadArtifacts")]
public sealed class DownloadArtifactsTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override async Task RunAsync(BuildContext context)
    {
        var version = BuildContext.CalculateVersion(context);
        var buildConf = context.Argument("build-configuration", "Release");
        var mgDir = System.IO.Path.Combine(context.BuildOutput, "mgpipeline", buildConf);

        context.CreateDirectory("nugets");
        context.CreateDirectory(mgDir);
        foreach (var os in new[] { "windows", "macos", "linux" })
        {
            await context.GitHubActions().Commands.DownloadArtifact($"nuget-{os}.{version}", "nugets");
            await context.GitHubActions().Commands.DownloadArtifact($"mgpipeline-{os}.{version}", mgDir);
        }

        context.CreateDirectory("vsix");
        await context.GitHubActions().Commands.DownloadArtifact($"MonoGame.Templates.VSExtension.{version}.vsix", "vsix");
    }
}
