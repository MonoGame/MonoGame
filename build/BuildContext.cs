
namespace BuildScripts;

public enum ProjectType
{
    Framework,
    Tools,
    Templates,
    ContentPipeline,
    MGCBEditor
}

public class BuildContext : FrostingContext
{
    public static readonly Version DefaultVersionNumber = new("3.8.1.1");

    public BuildContext(ICakeContext context) : base(context)
    {
        var buildConfiguration = context.Argument("build-configuration", "Release");
        ArtifactsDirectory = context.Argument("artifacts-directory", "artifacts");
        NuGetsDirectory = $"{ArtifactsDirectory}/NuGet/";

        string repositoryUrl = "";
        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            var workflow = context.BuildSystem().GitHubActions.Environment.Workflow;
            var version = $"{DefaultVersionNumber.Major}.{DefaultVersionNumber.Minor}.{DefaultVersionNumber.Build}.{workflow.RunNumber}";

            if (workflow.Repository == "MonoGame/MonoGame" &&
               workflow.RefType == GitHubActionsRefType.Branch &&
               workflow.RefName == "refs/heads/master")
            {
                Version = $"{version}-develop";
            }
            else
            {
                Version = $"{version}-{workflow.RepositoryOwner}";
            }

            repositoryUrl = $"https://github.com/{workflow.Repository}";
        }
        else
        {
            var buildNumber = context.EnvironmentVariable("BUILD_NUMBER", DefaultVersionNumber.ToString());
            var version = context.Argument("build-version", buildNumber);
            var branchName = context.EnvironmentVariable("BRANCH_NAME", string.Empty);

            if (!branchName.Contains("master"))
            {
                Version = $"{version}-develop";
            }
            else
            {
                Version = version;
            }

            repositoryUrl = context.EnvironmentVariable("repository-url", "https://github.com/MonoGame/MonoGame");
        }

        DotNetMSBuildSettings = new DotNetMSBuildSettings();
        DotNetMSBuildSettings.WithProperty(nameof(Version), Version);
        DotNetMSBuildSettings.WithProperty(nameof(repositoryUrl), repositoryUrl);

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
            //  SET MGFXC_WINE_PATH for building shaders on macOS and Linux
            System.Environment.SetEnvironmentVariable("MGFXC_WINE_PATH", context.EnvironmentVariable("HOME") + "/.winemonogame");
        }
        
        context.CreateDirectory(ArtifactsDirectory);
    }

    public string Version { get; }

    public string ArtifactsDirectory { get; }

    public DirectoryPath NuGetsDirectory { get; }

    public DotNetMSBuildSettings DotNetMSBuildSettings { get; }

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
