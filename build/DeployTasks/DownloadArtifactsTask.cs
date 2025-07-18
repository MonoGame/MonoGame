
namespace BuildScripts;

[TaskName("DownloadArtifacts")]
public sealed class DownloadArtifactsTask : AsyncFrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    private static async Task DownloadArtifactAsync(BuildContext context, string artifactName, string path)
    {
        var fullPath = context.GetOutputPath(path);
        context.Information($"Downloading {artifactName} to {fullPath}");
        context.CreateDirectory(fullPath);
        await context.GitHubActions().Commands.DownloadArtifact(artifactName, fullPath);
    }

    public override async Task RunAsync(BuildContext context)
    {
        await DownloadArtifactAsync(context, $"nuget-windows.{context.Version}", context.NuGetsDirectory);
        await DownloadArtifactAsync(context, $"nuget-macos.{context.Version}", context.NuGetsDirectory);
        await DownloadArtifactAsync(context, $"nuget-linux.{context.Version}", context.NuGetsDirectory);

        await DownloadArtifactAsync(context, $"mgpipeline-windows.{context.Version}", "native/mgpipeline/windows/Release/");
        await DownloadArtifactAsync(context, $"mgpipeline-macos.{context.Version}", "native/mgpipeline/macosx/Release/");
        await DownloadArtifactAsync(context, $"mgpipeline-linux.{context.Version}", "native/mgpipeline/linux/Release/");

        await DownloadArtifactAsync(context, $"MonoGame.Templates.VSExtension.{context.Version}.vsix", "vsix");
    }
}
