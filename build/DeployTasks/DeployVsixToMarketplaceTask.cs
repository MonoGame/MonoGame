
using System.Net.Http.Headers;
using System.Text;

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

    public override async void Run(BuildContext context)
    {
        var pat = context.EnvironmentVariable("MARKETPLACE_PAT");
        var publisher = "MonoGame";
        var extensionName = "MonoGame.Templates.VSExtension";

        var filePath = "vsix/MonoGame.Templates.VSExtension.vsix";
        if (!File.Exists(filePath))
        {
            context.Error("VSIX file not found!");
            return;
        }

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{pat}")));

        using var fileStream = File.OpenRead(filePath);
        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var response = await client.PutAsync($"https://marketplace.visualstudio.com/_apis/gallery/publishers/{publisher}/vsextensions/{extensionName}/versions", content);

        if (response.IsSuccessStatusCode)
        {
            context.Information("Successfully uploaded the VSIX to the Visual Studio Marketplace.");
        }
        else
        {
            context.Error($"Failed to upload VSIX. Response: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }
}
