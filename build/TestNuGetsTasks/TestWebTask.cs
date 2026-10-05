using IOPath = System.IO.Path;

namespace BuildScripts;

[TaskName("TestWeb")]
public sealed class TestWebTask : TestMonoGameTemplateTaskBase
{
    private static readonly PlatformFamily[] SupportedPlatformFamilies = { PlatformFamily.Windows, PlatformFamily.Linux, PlatformFamily.OSX };
    private static readonly string[] RequiredPublishedFiles =
    {
        "index.html",
        "_monogame/monogame-web-host.js",
        "_monogame/browser-accelerometer.js",
        "_monogame/browser-audio.js",
        "_monogame/browser-content.js",
        "_monogame/browser-host-common.js",
        "_monogame/browser-window.js"
    };
    private static readonly string[] UnexpectedPublishedFiles = { "main.js", "host.css" };
    private static readonly string[] RequiredDefaultShellFragments =
    {
        "id=\"monogame-loading\"",
        "id=\"monogame-error\"",
        "onStatus:",
        "onFirstFrame:",
        "onError:"
    };

    protected override string TemplateName => "WebGL2";
    protected override string ProjectFolderName => "webgl2";
    protected override string TemplateShortName => "mgweb";
    protected override PlatformFamily[] SupportedPlatforms => SupportedPlatformFamilies;
    protected override string SuccessMessage => "Published successfully";

    protected override string GetToolsJson(BuildContext context, string version)
    {
        return $$"""
            {
              "version": 1,
              "isRoot": true,
              "tools": {
                "dotnet-mgcb": {
                  "version": "{{version}}",
                  "commands": [
                    "mgcb"
                  ]
                }
              }
            }
            """;
    }

    protected override void ValidateProject(BuildContext context, string projectDir)
    {
        string projectDirectory = context.MakeAbsolute(new DirectoryPath(projectDir)).FullPath;
        string publishDirectory = IOPath.Combine(projectDirectory, "publish");
        ProcessArgumentBuilder arguments = new ProcessArgumentBuilder()
            .Append("publish")
            .Append("--no-restore")
            .Append("--configuration")
            .Append("Release")
            .Append("--output")
            .AppendQuoted(publishDirectory);
        ProcessSettings settings = new()
        {
            Arguments = arguments,
            WorkingDirectory = projectDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        int result = context.StartProcess("dotnet", settings, out IEnumerable<string> output);
        if (result != 0)
        {
            foreach (string line in output)
            {
                context.Error(line);
            }

            throw new InvalidOperationException("WebGL2 template project failed to publish.");
        }

        VerifyPublishedHost(IOPath.Combine(publishDirectory, "wwwroot"));
    }

    private static void VerifyPublishedHost(string publishDirectory)
    {
        foreach (string relativePath in RequiredPublishedFiles)
        {
            string path = IOPath.Combine(publishDirectory, relativePath);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"The published WebGL2 template is missing '{relativePath}'.", path);
            }
        }

        foreach (string relativePath in UnexpectedPublishedFiles)
        {
            string path = IOPath.Combine(publishDirectory, relativePath);
            if (File.Exists(path))
            {
                throw new InvalidOperationException($"The published WebGL2 template unexpectedly contains '{relativePath}'.");
            }
        }

        string indexHtmlPath = IOPath.Combine(publishDirectory, "index.html");
        string indexHtml = File.ReadAllText(indexHtmlPath);
        if (!indexHtml.Contains("const { start } = await import(\"./_monogame/monogame-web-host.js\");", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("The published WebGL2 template does not use the explicit browser host bootstrap.");
        }

        foreach (string requiredFragment in RequiredDefaultShellFragments)
        {
            if (!indexHtml.Contains(requiredFragment, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"The published WebGL2 template is missing the default startup shell fragment '{requiredFragment}'.");
            }
        }
    }
}
