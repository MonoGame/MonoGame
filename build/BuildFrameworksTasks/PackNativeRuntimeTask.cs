
namespace BuildScripts;

/// <summary>
/// Downloads native runtime binaries from all platform/architecture build agents
/// and packs the MonoGame.Runtime.* NuGet packages with complete multi-arch support.
///
/// This is separate from "Build Native" because Linux arm64 and x64 are built on
/// different GitHub Actions runners and cannot cross-compile for each other.
/// </summary>
[TaskName("Pack Native Runtime")]
public sealed class PackNativeRuntimeTask : AsyncFrostingTask<BuildContext>
{
    private static async Task DownloadArtifactAsync(BuildContext context, string artifactName, string path)
    {
        context.Information($"Downloading {artifactName} to {path}");
        context.CreateDirectory(path);
        await context.GitHubActions().Commands.DownloadArtifact(artifactName, path);
    }

    public override async Task RunAsync(BuildContext context)
    {
        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            // Download native runtime binaries from all platform/arch build agents.
            // These were uploaded by the "UploadArtifacts" task on each build runner.

            // Windows DX12 (windowsdx) - both architectures built on same runner
            await DownloadArtifactAsync(context, $"mgnative-windows-dx-x64.{context.Version}", "Artifacts/native/mgruntime/windowsdx/windows/x64/");
            await DownloadArtifactAsync(context, $"mgnative-windows-dx-arm64.{context.Version}", "Artifacts/native/mgruntime/windowsdx/windows/arm64/");

            // Windows Vulkan (desktopvk) - both architectures built on same runner
            await DownloadArtifactAsync(context, $"mgnative-windows-vk-x64.{context.Version}", "Artifacts/native/mgruntime/desktopvk/windows/x64/");
            await DownloadArtifactAsync(context, $"mgnative-windows-vk-arm64.{context.Version}", "Artifacts/native/mgruntime/desktopvk/windows/arm64/");

            // Linux Vulkan - x64 and arm64 from separate runners
            await DownloadArtifactAsync(context, $"mgnative-linux-x64.{context.Version}", "Artifacts/native/mgruntime/desktopvk/linux/x64/");
            await DownloadArtifactAsync(context, $"mgnative-linux-arm64.{context.Version}", "Artifacts/native/mgruntime/desktopvk/linux/arm64/");

            // macOS Vulkan - universal binary (x64 + arm64 in one file)
            await DownloadArtifactAsync(context, $"mgnative-macos.{context.Version}", "Artifacts/native/mgruntime/desktopvk/macosx/");
        }

        // Pack all runtime NuGet packages with whatever native binaries are available.
        // On GitHub Actions this will include all platforms/architectures.
        // For local builds, only the locally built binaries will be included.
        context.DotNetPack("src/NuGetPackages/MonoGame.Runtime.Windows.DX12/MonoGame.Runtime.Windows.DX12.csproj", context.DotNetPackSettings);
        context.DotNetPack("src/NuGetPackages/MonoGame.Runtime.Windows.Vulkan/MonoGame.Runtime.Windows.Vulkan.csproj", context.DotNetPackSettings);
        context.DotNetPack("src/NuGetPackages/MonoGame.Runtime.Mac.Vulkan/MonoGame.Runtime.Mac.Vulkan.csproj", context.DotNetPackSettings);
        context.DotNetPack("src/NuGetPackages/MonoGame.Runtime.Linux.Vulkan/MonoGame.Runtime.Linux.Vulkan.csproj", context.DotNetPackSettings);

        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            // Upload the packed runtime NuGets as a separate artifact for the deploy job
            await context.GitHubActions().Commands.UploadArtifact(
                new DirectoryPath(context.NuGetsDirectory),
                $"nuget-runtime.{context.Version}");
        }
    }
}
