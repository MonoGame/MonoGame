
namespace BuildScripts;

[TaskName("DownloadBinaries")]
public sealed class DownloadBinariesTask : AsyncFrostingTask<BuildContext>
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
        foreach (PlatformFamily platform in Enum.GetValues(typeof(PlatformFamily)))
        {
            string platformStr = platform switch
            {
                PlatformFamily.Windows => "windows",
                PlatformFamily.OSX => "macos",
                _ => "linux"
            };
            await DownloadArtifactAsync(context, $"mgcontentbuilder-{platformStr}.{context.Version}", "binaries/MonoGame.Framework.Content.Pipeline/");
            await DownloadArtifactAsync(context, $"mgeffectcompiler-{platformStr}.{context.Version}", "binaries/MonoGame.Framework.Content.Pipeline/");
            await DownloadArtifactAsync(context, $"mgcontentpipeline-{platformStr}.{context.Version}", "binaries/MonoGame.Framework.Content.Pipeline/");
            await DownloadArtifactAsync(context, $"mgpipeline-{platformStr}.{context.Version}", "binaries/MonoGame.Framework.Content.Pipeline/");
            await DownloadArtifactAsync(context, $"mgframework-{platformStr}.{context.Version}", "binaries/MonoGame.Framework/");
        }

        // Manually download native Windows binaries, once Linux/Mac are available, they will move the the loop above.
        await DownloadArtifactAsync(context, $"mgnative-windows.{context.Version}", "binaries/MonoGame.Framework/");

        // Clean up duplicate "publish" folder from NuGet packaging
        DeleteDirectory(context, context.GetOutputPath("binaries/MonoGame.Framework.Content.Pipeline/publish"));
        var mgfPath = context.GetOutputPath("binaries/MonoGame.Framework/");
        // loop through the mgf path and locate folders named "release" and move their contents to their parent folder using only Cake Frosting Context methods
        // Find all "release" folders under the mgfPath using Cake's globbing and move their contents up one folder
        foreach (var releaseDir in context.GetDirectories($"{mgfPath}/**/release"))
        {
            var parentFullPath = System.IO.Path.GetDirectoryName(releaseDir.FullPath);
            if (string.IsNullOrEmpty(parentFullPath))
                continue;

            context.MoveFiles($"{releaseDir.FullPath}/*.*", parentFullPath);
            DeleteDirectory(context, releaseDir.FullPath);
        }
    }

    private void DeleteDirectory(BuildContext context, string fullPath)
    {
        context.DeleteDirectory(fullPath, new DeleteDirectorySettings { Recursive = true, Force = true });
    }
}
