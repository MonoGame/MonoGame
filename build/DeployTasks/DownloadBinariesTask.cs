
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
        foreach (PlatformFamily platform in Enum.GetValues(typeof(PlatformFamily)))
        {
            string platformStr = platform switch
            {
                PlatformFamily.Windows => "windows",
                PlatformFamily.OSX => "macos",
                _ => "linux"
            };
            await DownloadArtifactAsync(context, $"mgcontentbuilder-{platformStr}.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework.Content.Pipeline/");
            await DownloadArtifactAsync(context, $"mgeffectcompiler-{platformStr}.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework.Content.Pipeline/");
            await DownloadArtifactAsync(context, $"mgcontentpipeline-{platformStr}.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework.Content.Pipeline/");
            await DownloadArtifactAsync(context, $"mgpipeline-{platformStr}.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework.Content.Pipeline/");
            await DownloadArtifactAsync(context, $"mgbinaries-{platformStr}.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");
            await DownloadArtifactAsync(context, $"mgframework-{platformStr}.{context.Version}", $"Artifacts/MonoGame.Framework/");
        }

        // Manually download native Windows binaries, once Linux/Mac are available, they will move the the loop above.
        await DownloadArtifactAsync(context, $"mgnative-windows.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");

        // Clean up duplicate "publish" folder from NuGet cp packaging
        context.DeleteDirectory(context.GetOutputPath($"{binariesPackagingFolder}MonoGame.Framework.Content.Pipeline/publish"));

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
