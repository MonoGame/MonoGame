using Cake.Git;
using Microsoft.VisualBasic;
using System.Text.RegularExpressions;

namespace BuildScripts;

public enum ProjectType
{
    Framework,
    Tools,
    Templates,
    ContentPipeline,
    MGCBEditor,
    MGCBEditorLauncher
}

public class BuildContext : FrostingContext
{
    public static string VersionBase = "1.0.0";
    public static readonly Regex VersionRegex = new(@"^v\d+.\d+.\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static readonly string DefaultRepositoryUrl = "https://github.com/MonoGame/MonoGame";

    public BuildContext(ICakeContext context) : base(context)
    {
        var repositoryUrl = context.Argument("build-repository", DefaultRepositoryUrl);
        var buildConfiguration = context.Argument("build-configuration", "Release");
        BuildOutput = context.Argument("build-output", "artifacts");
        NuGetsDirectory = $"{BuildOutput}/NuGet/";

        var tags = GitAliases.GitTags(context, ".");
        foreach (var tag in tags)
        {
            if (VersionRegex.IsMatch(tag.FriendlyName))
            {
                VersionBase = tag.FriendlyName[1..];
            }
        }

        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            var workflow = context.BuildSystem().GitHubActions.Environment.Workflow;
            repositoryUrl = $"https://github.com/{workflow.Repository}";

            if (workflow.Repository != "MonoGame/MonoGame")
            {
                Version = $"{VersionBase}.{workflow.RunNumber}-{workflow.RepositoryOwner}";
            }
            else if (workflow.RefType == GitHubActionsRefType.Tag)
            {
                var baseVersion = workflow.RefName;
                if (!Regex.IsMatch(baseVersion, @"v\d+.\d+.\d+"))
                    throw new Exception($"Invalid tag: {baseVersion}");
                
                VersionBase = baseVersion[1..];
                Version = $"{VersionBase}.{workflow.RunNumber}";
            }
            else if (workflow.RefType == GitHubActionsRefType.Branch && workflow.RefName != "refs/heads/master")
            {
                Version = $"{VersionBase}.{workflow.RunNumber}-develop";
            }
            else
            {
                Version = $"{VersionBase}.{workflow.RunNumber}";
            }
        }
        else
        {
            Version = context.Argument("build-version", VersionBase + ".1-develop");
        }

        DotNetMSBuildSettings = new DotNetMSBuildSettings();
        DotNetMSBuildSettings.WithProperty("Version", Version);
        DotNetMSBuildSettings.WithProperty("RepositoryUrl", repositoryUrl);

        DotNetBuildSettings = new DotNetBuildSettings
        {
            MSBuildSettings = DotNetMSBuildSettings,
            Verbosity = DotNetVerbosity.Minimal,
            Configuration = buildConfiguration
        };

        DotNetPackSettings = new DotNetPackSettings
        {
            MSBuildSettings = DotNetMSBuildSettings,
            Verbosity = DotNetVerbosity.Minimal,
            OutputDirectory = NuGetsDirectory,
            Configuration = buildConfiguration
        };

        MSBuildSettings = new MSBuildSettings
        {
            Verbosity = Verbosity.Minimal,
            Configuration = buildConfiguration
        };
        MSBuildSettings.WithProperty(nameof(Version), Version);
        MSBuildSettings.WithProperty(nameof(repositoryUrl), repositoryUrl);

        MSPackSettings = new MSBuildSettings
        {
            Verbosity = Verbosity.Minimal,
            Configuration = buildConfiguration,
            Restore = true
        };
        MSPackSettings.WithProperty(nameof(Version), Version);
        MSPackSettings.WithProperty(nameof(repositoryUrl), repositoryUrl);
        MSPackSettings.WithProperty("OutputDirectory", NuGetsDirectory.FullPath);
        MSPackSettings.WithTarget("Pack");

        DotNetPublishSettings = new DotNetPublishSettings
        {
            MSBuildSettings = DotNetMSBuildSettings,
            Verbosity = DotNetVerbosity.Minimal,
            Configuration = buildConfiguration,
            SelfContained = false
        };

        Console.WriteLine($"Version: {Version}");
        Console.WriteLine($"RepositoryUrl: {repositoryUrl}");
        Console.WriteLine($"BuildConfiguration: {buildConfiguration}");

        if (!context.IsRunningOnWindows())
        {
            // SET MGFXC_WINE_PATH for building shaders on macOS and Linux
            System.Environment.SetEnvironmentVariable("MGFXC_WINE_PATH", context.EnvironmentVariable("HOME") + "/.winemonogame");
        }
        
        context.CreateDirectory(BuildOutput);
    }

    public string Version { get; }

    public string BuildOutput { get; }

    public DirectoryPath NuGetsDirectory { get; }

    public DotNetMSBuildSettings DotNetMSBuildSettings { get; }

    public DotNetBuildSettings DotNetBuildSettings { get; }

    public DotNetPackSettings DotNetPackSettings { get; }

    public DotNetPublishSettings DotNetPublishSettings { get; }

    public MSBuildSettings MSBuildSettings { get; }

    public MSBuildSettings MSPackSettings { get; }

    public string GetProjectPath(ProjectType type, string id = "") => type switch
    {
        ProjectType.Framework => $"MonoGame.Framework/MonoGame.Framework.{id}.csproj",
        ProjectType.Tools => $"Tools/{id}/{id}.csproj",
        ProjectType.Templates => $"Templates/{id}/{id}.csproj",
        ProjectType.ContentPipeline => "MonoGame.Framework.Content.Pipeline/MonoGame.Framework.Content.Pipeline.csproj",
        ProjectType.MGCBEditor => $"Tools/MonoGame.Content.Builder.Editor/MonoGame.Content.Builder.Editor.{id}.csproj",
		ProjectType.MGCBEditorLauncher => $"Tools/MonoGame.Content.Builder.Editor.Launcher/MonoGame.Content.Builder.Editor.Launcher.{id}.csproj",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    public bool IsWorkloadInstalled(string workload)
    {
        this.StartProcess(
            "dotnet",
            new ProcessSettings()
            {
                Arguments = $"workload list",
                RedirectStandardOutput = true
            },
            out IEnumerable<string> processOutput
        );

        return processOutput.Any(match => match.StartsWith($"{workload} "));
    }
}
