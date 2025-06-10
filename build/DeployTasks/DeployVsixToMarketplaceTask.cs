
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
    }

    public override void Run(BuildContext context)
    {
        var pat = context.EnvironmentVariable("MARKETPLACE_PAT");
        var publisher = "MonoGame";
        var extensionName = "MonoGame.Templates.VSExtension";
        var vsixPath = "vsix/MonoGame.Templates.VSExtension.vsix";

        if (!File.Exists(vsixPath))
        {
            context.Error("VSIX file not found!");
            return;
        }

        // Find VsixPublisher.exe location - adjust as needed for your environment
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        var vsixPublisherPath = System.IO.Path.Combine(
            programFilesX86,
            "Microsoft Visual Studio",
            "2022",
            "Enterprise", // May need to check for other editions (Professional, Community)
            "VSSDK",
            "VisualStudioIntegration",
            "Tools",
            "Bin",
            "VsixPublisher.exe");

        if (!File.Exists(vsixPublisherPath))
        {
            context.Log.Information("Looking for VsixPublisher in alternate locations...");
            // Try to find it in other edition folders
            foreach (var edition in new[] { "Professional", "Community" })
            {
                var alternatePath = System.IO.Path.Combine(
                    programFilesX86,
                    "Microsoft Visual Studio",
                    "2022",
                    edition,
                    "VSSDK",
                    "VisualStudioIntegration",
                    "Tools",
                    "Bin",
                    "VsixPublisher.exe");

                if (File.Exists(alternatePath))
                {
                    vsixPublisherPath = alternatePath;
                    break;
                }
            }
        }

        if (!File.Exists(vsixPublisherPath))
        {
            context.Error("VsixPublisher.exe not found!");
            return;
        }

        // Create manifest file
        var manifestPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "publish-manifest.json");
        var manifest = new
        {
            publisher = publisher,
            extensionName = extensionName,
            qnaEnabled = true,
            categories = new[] { "templates" }, // Adjust categories as needed
            identity = new { internalName = extensionName },
            overview = "overview.md", // You might need to create this file
            priceCategory = "free",
            helpMarkdown = "", // Optional help markdown
            assetFiles = new[] {
                new {
                    pathOnDisk = vsixPath,
                    targetPath = extensionName + ".vsix"
                }
            }
        };

        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));

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

        // Clean up the temporary manifest file
        if (File.Exists(manifestPath))
        {
            File.Delete(manifestPath);
        }
    }
}
