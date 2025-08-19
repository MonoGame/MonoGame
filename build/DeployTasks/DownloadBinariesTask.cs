
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
            await DownloadArtifactAsync(context, $"mgcontentbuilder-{platformStr}.{context.Version}", "binaries/MonoGame.Content.Builder/");
            await DownloadArtifactAsync(context, $"mgeffectcompiler-{platformStr}.{context.Version}", "binaries/MonoGame.Effect.Compiler/");
            await DownloadArtifactAsync(context, $"mgframework-{platformStr}.{context.Version}", "binaries/MonoGame.Framework/");
            await DownloadArtifactAsync(context, $"mgcontentpipeline-{platformStr}.{context.Version}", "binaries/MonoGame.Framework.Content.Pipeline/");
        }

        await DownloadArtifactAsync(context, $"mgpipeline-windows.{context.Version}", "binaries/mgpipeline/windows/Release/");
        await DownloadArtifactAsync(context, $"mgpipeline-macos.{context.Version}", "binaries/mgpipeline/macosx/Release/");
        await DownloadArtifactAsync(context, $"mgpipeline-linux.{context.Version}", "binaries/mgpipeline/linux/Release/");

        await DownloadArtifactAsync(context, $"mgnative-windows.{context.Version}", "binaries/mgnative/windows/Release/");
        //await DownloadArtifactAsync(context, $"mgnative-macos.{context.Version}", "binaries/mgnative/macosx/Release/");
        //await DownloadArtifactAsync(context, $"mgnative-linux.{context.Version}", "binaries/mgnative/linux/Release/");
    }
}
