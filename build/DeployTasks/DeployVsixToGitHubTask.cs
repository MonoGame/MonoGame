
using System.Net.Http.Headers;
using System.Text.Json;

namespace BuildScripts;

[TaskName("DeployVsixToGitHubTask")]
[IsDependentOn(typeof(DownloadArtifactsTask))]
public sealed class DeployVsixToGitHubTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.BuildSystem().IsRunningOnGitHubActions;

    public override void Run(BuildContext context)
    {
        var filePath = "vsix/MonoGame.Templates.VSExtension.vsix";
        if (!File.Exists(filePath))
        {
            context.Error("VSIX file not found!");
            return;
        }

        var owner = context.GitHubActions().Environment.Workflow.RepositoryOwner;
        var repo = context.GitHubActions().Environment.Workflow.Repository;
        var tagName = "v3.8.2"; // Need to pick latest tag dynamically from the tag that triggered the workflow

        if (tagName == null)
        {
            context.Error("No tags found in the repository!");
            return;
        }

        var token = context.EnvironmentVariable("GITHUB_TOKEN");
        var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/releases/tags/{tagName}";

        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("GitHubUploadClient");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);

        var releaseResponse = client.GetAsync(apiUrl).Result;
        if (!releaseResponse.IsSuccessStatusCode)
        {
            context.Error($"Failed to find release: {releaseResponse.ReasonPhrase}");
            return;
        }

        var release = JsonDocument.Parse(releaseResponse.Content.ReadAsStringAsync().Result).RootElement;
        var uploadUrl = release.GetProperty("upload_url").GetString()?.Replace("{?name,label}", $"?name={System.IO.Path.GetFileName(filePath)}");

        if (uploadUrl == null)
        {
            context.Error("Failed to find upload URL.");
            return;
        }

        using var stream = File.OpenRead(filePath);
        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        var uploadResponse = client.PostAsync(uploadUrl, content).Result;
        if (uploadResponse.IsSuccessStatusCode)
        {
            context.Information("Successfully uploaded VSIX to GitHub Release.");
        }
        else
        {
            context.Error($"Failed to upload VSIX: {uploadResponse.ReasonPhrase}");
        }
    }

}
