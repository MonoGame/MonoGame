
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
    public static readonly string DefaultRepositoryUrl = "https://github.com/MonoGame/MonoGame";
    public static readonly string DefaultBaseVersion = "3.8.1";

    public BuildContext(ICakeContext context) : base(context)
    {
        var repositoryUrl = context.Argument("build-repository", DefaultRepositoryUrl);
        var buildConfiguration = context.Argument("build-configuration", "Release");
        BuildOutput = context.Argument("build-output", "artifacts");
        Version = context.Argument("build-version", DefaultBaseVersion + ".1-develop");
        NuGetsDirectory = $"{BuildOutput}/NuGet/";

        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            var workflow = context.BuildSystem().GitHubActions.Environment.Workflow;
            repositoryUrl = $"https://github.com/{workflow.Repository}";

            if (workflow.Repository != "MonoGame/MonoGame")
            {
                Version = $"{DefaultBaseVersion}.{workflow.RunNumber}-{workflow.RepositoryOwner}";
            }
            else if (workflow.RefType == GitHubActionsRefType.Branch && workflow.RefName != "refs/heads/master")
            {
                Version = $"{DefaultBaseVersion}.{workflow.RunNumber}-develop";
            }
            else
            {
                Version = $"{DefaultBaseVersion}.{workflow.RunNumber}";
            }
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
