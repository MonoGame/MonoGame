
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

        // Runtime NuGets are packed in a separate job after all native builds complete
        await DownloadArtifactAsync(context, $"nuget-runtime.{context.Version}", context.NuGetsDirectory);

        // Windows mgpipeline produces both x64 and arm64
        await DownloadArtifactAsync(context, $"mgpipeline-windows-x64.{context.Version}", "native/mgpipeline/windows/x64/Release/");
        await DownloadArtifactAsync(context, $"mgpipeline-windows-arm64.{context.Version}", "native/mgpipeline/windows/arm64/Release/");

        // macOS mgpipeline produces universal binary for both x64 and arm64
        await DownloadArtifactAsync(context, $"mgpipeline-macos.{context.Version}", "native/mgpipeline/macosx/Release/");

        // Linux mgpipeline produces both x64 and arm64 but on different hosts
        await DownloadArtifactAsync(context, $"mgpipeline-linux-x64.{context.Version}", "native/mgpipeline/linux/x64/Release/");
        await DownloadArtifactAsync(context, $"mgpipeline-linux-arm64.{context.Version}", "native/mgpipeline/linux/arm64/Release/");

        await DownloadArtifactAsync(context, $"MonoGame.Templates.VSExtension.{context.Version}.vsix", "vsix");
    }
}
