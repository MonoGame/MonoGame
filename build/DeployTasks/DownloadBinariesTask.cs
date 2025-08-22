
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
        }

        // Manually download native Windows binaries, once Linux/Mac are available, they will move the the loop above.
        await DownloadArtifactAsync(context, $"mgnative-windows.{context.Version}", $"{binariesPackagingFolder}MonoGame.Framework/");

        // Clean up duplicate "publish" folder from NuGet cp packaging
        DeleteDirectory(context, context.GetOutputPath($"{binariesPackagingFolder}MonoGame.Framework.Content.Pipeline/publish"));


        // Post tasks due to issues with Android / iOS "publish" steps
        var inputPath = context.GetOutputPath($"Artifacts/MonoGame.Framework/");
        var outputPath = context.GetOutputPath($"{binariesPackagingFolder}MonoGame.Framework/");
        // Copy MonoGame.Framework/Android to Binaries/MonoGame.Framework
        context.CreateDirectory($"{outputPath}Android");
        context.CopyDirectory($"{inputPath}Android", $"{outputPath}Android");
        context.CreateDirectory($"{outputPath}Android/runtimes");
        context.CopyDirectory($"{binariesPackagingFolder}MonoGame.Framework/runtimes", $"{outputPath}Android/runtimes");


        // Copy MonoGame.Framework/IOS to Binaries/MonoGame.Framework
        context.CreateDirectory($"{outputPath}iOS");
        context.CopyDirectory($"{inputPath}iOS", $"{outputPath}iOS");
        context.CreateDirectory($"{outputPath}iOS/runtimes");
        context.CopyDirectory($"{binariesPackagingFolder}MonoGame.Framework/runtimes", $"{outputPath}iOS/runtimes");
    }

    private void DeleteDirectory(BuildContext context, DirectoryPath fullPath)
    {
        context.DeleteDirectory(fullPath, new DeleteDirectorySettings { Recursive = true, Force = true });
    }
}
