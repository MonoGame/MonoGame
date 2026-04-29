
namespace BuildScripts;

[TaskName("DownloadBinaries")]
public sealed class DownloadBinariesTask : AsyncFrostingTask<BuildContext>
{
    private string binariesPackagingFolder = "binPackaging/";
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
        // Download managed framework/binaries/pipeline artifacts
        // Windows: only x64 runner (managed code is arch-independent, single upload)
        // Linux: both x64 and arm64 runners produce managed artifacts
        // macOS: universal binary, no arch suffix
        string[] managedVariants = ["windows-x64", "linux-x64", "linux-arm64"];
        foreach (var variant in managedVariants)
        {
            await DownloadArtifactAsync(context, $"mgframework-{variant}.{context.Version}", $"Artifacts/MonoGame.Framework/");
            await DownloadArtifactAsync(context, $"mgbinaries-{variant}.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");
            await DownloadArtifactAsync(context, $"mgpipeline-{variant}.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/MonoGame.Framework.Content.Pipeline/");
        }
        // macOS (no arch suffix)
        await DownloadArtifactAsync(context, $"mgframework-macos.{context.Version}", $"Artifacts/MonoGame.Framework/");
        await DownloadArtifactAsync(context, $"mgbinaries-macos.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");
        await DownloadArtifactAsync(context, $"mgpipeline-macos.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/MonoGame.Framework.Content.Pipeline/");

        // Download native runtime binaries for all platform/arch combinations
        await DownloadArtifactAsync(context, $"mgnative-windows-dx-x64.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");
        await DownloadArtifactAsync(context, $"mgnative-windows-dx-arm64.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");
        await DownloadArtifactAsync(context, $"mgnative-windows-vk-x64.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");
        await DownloadArtifactAsync(context, $"mgnative-windows-vk-arm64.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");
        await DownloadArtifactAsync(context, $"mgnative-linux-x64.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");
        await DownloadArtifactAsync(context, $"mgnative-linux-arm64.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");
        await DownloadArtifactAsync(context, $"mgnative-macos.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");

        context.MoveDirectory(context.GetOutputPath($"{binariesPackagingFolder}/MonoGame.Framework/MonoGame.Framework.Content.Pipeline/"), context.GetOutputPath($"{binariesPackagingFolder}MonoGame.Framework.Content.Pipeline/"));

        // Post tasks due to issues with Android / iOS "publish" steps
        var sourcePath = context.GetOutputPath("Artifacts/MonoGame.Framework/");
        var processingPath = context.GetOutputPath($"{binariesPackagingFolder}MonoGame.Framework/");
        string[] targets = ["Android", "iOS"];
        foreach (var platform in targets)
        {
            context.Information($"Post Processing platform: {platform}");
            context.CreateDirectory($"{processingPath}{platform}");
            context.CopyFiles($"{sourcePath}{platform}/Release/*.*", $"{processingPath}{platform}");
            context.CreateDirectory($"{processingPath}{platform}/runtimes");
            context.CopyDirectory($"{processingPath}DesktopGL/runtimes", $"{processingPath}{platform}/runtimes");
        }
    }
}
