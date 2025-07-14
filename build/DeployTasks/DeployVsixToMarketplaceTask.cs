
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BuildScripts;

[TaskName("DeployVsixToMarketplaceTask")]
[IsDependentOn(typeof(DownloadArtifactsTask))]
public sealed class DeployVsixToMarketplaceTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
#if DEBUG
        return true;
#else
        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            var workflow = context.BuildSystem().GitHubActions.Environment.Workflow;
            if (workflow.RefType == GitHubActionsRefType.Tag &&
                !string.IsNullOrWhiteSpace(context.EnvironmentVariable("MARKETPLACE_PAT")))
            {
                return true;
            }
        }

        return false;
#endif
    }

    public override void Run(BuildContext context)
    {
        var pat = context.EnvironmentVariable("MARKETPLACE_PAT");
        var vsixPath = "vsix/MonoGame.Templates.VSExtension.vsix";

        if (!File.Exists(vsixPath))
        {
            context.Error("VSIX file not found!");
            return;
        }

        // Find VsixPublisher.exe location - adjust as needed for your environment
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        string vsixPublisherPath = string.Empty;

        context.Log.Information("Looking for VsixPublisher in VS Edition locations...");
        // Try to find it in other edition folders
        foreach (var edition in new[] { "Enterprise", "Professional", "Community" })
        {
            var vsEditionPath = System.IO.Path.Combine(
                programFiles,
                "Microsoft Visual Studio",
                "2022",
                edition,
                "VSSDK",
                "VisualStudioIntegration",
                "Tools",
                "Bin",
                "VsixPublisher.exe");

            if (File.Exists(vsEditionPath))
            {
                vsixPublisherPath = vsEditionPath;
                break;
            }
        }

        if (!File.Exists(vsixPublisherPath))
        {
            context.Error("VsixPublisher.exe not found!");
            return;
        }

        var manifestPath = "vsix/publishManifest.json";

        if (!File.Exists(manifestPath))
        {
            context.Error("publishManifest.json not found!");
            return;
        }

        // Run VsixPublisher
        var processSettings = new ProcessSettings
        {
            Arguments = $"publish -payload \"{vsixPath}\" -publishManifest \"{manifestPath}\" -personalAccessToken \"{pat}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            Silent = false
        };

        context.Information($"Publishing VSIX using VsixPublisher at: {vsixPublisherPath} with arguments: {processSettings.Arguments}");

        /*TODO uncomment this block when above it working 
        var exitCode = context.StartProcess(vsixPublisherPath, processSettings, out var stdOutput, out var stdError);

        if (exitCode == 0)
        {
            context.Information($"Successfully uploaded the VSIX to the Visual Studio Marketplace.{Environment.NewLine}Info: {stdOutput}");
        }
        else
        {
            context.Error($"Failed to upload VSIX. Exit code: {exitCode}{Environment.NewLine}Error:{stdError}");
        }*/
    }
}
