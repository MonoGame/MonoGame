using Cake.Common.Tools.DotNet.Run;
using Cake.Git;
using MonoGame.Tool;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace BuildScripts;

public enum ProjectType
{
    Extension,
    Framework,
    Tools,
    Templates,
    Tests,
    ContentPipeline,
    DevTools,
    MGCBEditor,
    MGCBEditorLauncher
}

public class BuildContext : FrostingContext
{
    public static string VersionBase = "3.8.4";
    public static readonly Regex VersionRegex = new(@"^v\d+.\d+.\d+", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    public static readonly string DefaultRepositoryUrl = "https://github.com/MonoGame/MonoGame";

    public BuildContext(ICakeContext context) : base(context)
    {
        var repositoryUrl = context.Argument("build-repository", DefaultRepositoryUrl);
        var buildConfiguration = context.Argument("build-configuration", "Release");
        BuildOutput = context.Argument("build-output", "Artifacts");
        NuGetsDirectory = $"{BuildOutput}/NuGet/";

        Version = CalculateVersion(context);

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
        MSPackSettings.WithProperty("OutputDirectory", NuGetsDirectory);
        MSPackSettings.WithTarget("Pack");

        DotNetPublishSettings = new DotNetPublishSettings
        {
            MSBuildSettings = DotNetMSBuildSettings,
            Verbosity = DotNetVerbosity.Minimal,
            Configuration = buildConfiguration,
            SelfContained = false
        };
        // SelfContained needs to be default for MacOS
        DotNetPublishSettingsForMac = new DotNetPublishSettings
        {
            MSBuildSettings = DotNetMSBuildSettings,
            Verbosity = DotNetVerbosity.Minimal,
            Configuration = buildConfiguration
        };

        DotNetRunSettings = new DotNetRunSettings
        {
            NoBuild = true,
            NoRestore = true,
            Configuration = "Release"
        };

        Console.WriteLine($"Version: {Version}");
        Console.WriteLine($"RepositoryUrl: {repositoryUrl}");
        Console.WriteLine($"BuildConfiguration: {buildConfiguration}");

        if (context.IsRunningOnWindows())
        {
            // SET PATH SO PROCESSESS CAN FIND DXC.EXE
            var pathEnv = System.Environment.GetEnvironmentVariable("PATH");
            pathEnv += ";" + AppDomain.CurrentDomain.BaseDirectory;
            System.Environment.SetEnvironmentVariable("PATH", pathEnv);
        }
        else
        {
            // SET MGFXC_WINE_PATH for building shaders on macOS and Linux
            System.Environment.SetEnvironmentVariable("MGFXC_WINE_PATH", context.EnvironmentVariable("HOME") + "/.winemonogame");
        }

        context.CreateDirectory(BuildOutput);
    }

    public string Version { get; }

    public string BuildOutput { get; }

    public string NuGetsDirectory { get; }

    public DotNetMSBuildSettings DotNetMSBuildSettings { get; }

    public DotNetBuildSettings DotNetBuildSettings { get; }

    public DotNetPackSettings DotNetPackSettings { get; }

    public DotNetPublishSettings DotNetPublishSettings { get; }

    public DotNetPublishSettings DotNetPublishSettingsForMac { get; }

    public DotNetRunSettings DotNetRunSettings { get; }

    public MSBuildSettings MSBuildSettings { get; }

    public MSBuildSettings MSPackSettings { get; }

    public string ShellWorkingDir { get; set; } = Directory.GetCurrentDirectory();

    public string GetProjectPath(ProjectType type, string id = "") => type switch
    {
        ProjectType.Extension => $"Templates/{id}/{id}.csproj",
        ProjectType.Framework => $"MonoGame.Framework/MonoGame.Framework.{id}.csproj",
        ProjectType.Tools => $"Tools/{id}/{id}.csproj",
        ProjectType.Templates => $"external/MonoGame.Templates/CSharp/{id}.csproj",
        ProjectType.Tests => $"Tests/{id}.csproj",
        ProjectType.ContentPipeline => "MonoGame.Framework.Content.Pipeline/MonoGame.Framework.Content.Pipeline.csproj",
        ProjectType.DevTools => "src/MonoGame.Framework.DevTools/MonoGame.Framework.DevTools.csproj",
        ProjectType.MGCBEditor => $"Tools/MonoGame.Content.Builder.Editor/MonoGame.Content.Builder.Editor.{id}.csproj",
        ProjectType.MGCBEditorLauncher => $"Tools/MonoGame.Content.Builder.Editor.Launcher/MonoGame.Content.Builder.Editor.Launcher.{id}.csproj",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    public string GetOutputPath(string path)
    {
        if (System.IO.Path.IsPathRooted(path) || path.StartsWith(BuildOutput))
        {
            return path;
        }

        return System.IO.Path.Combine(BuildOutput, path);
    }

    public void Shell(string command, string args)
    {
        if (this.StartProcess(command, new ProcessSettings { WorkingDirectory = ShellWorkingDir, Arguments = args }) != 0)
        {
            throw new Exception($"Execution failed for: {command} {args}");
        }
    }

    public void DotNetRun(string project, string args, DirectoryPath? workingDir = null)
    {
        var mgfxc = System.IO.Path.Combine(Directory.GetCurrentDirectory(), project);
        DotNetRunSettings.WorkingDirectory = workingDir ?? "";
        this.DotNetRun(mgfxc, args, DotNetRunSettings);
        DotNetRunSettings.WorkingDirectory = "";
    }

    public int DxcRun(string args) => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? this.StartProcess("dxc.exe", args) : Dxc.Run(args, out _, out _);

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

    public void CheckLib(string relativePath)
    {
        var filePath = GetOutputPath(relativePath);
        this.Information($"Checking library: {filePath}");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(filePath);
        }

        switch (Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                StaticLibCheck.CheckWindows(this, filePath);
                break;
            case PlatformFamily.Linux:
                StaticLibCheck.CheckLinux(this, filePath);
                break;
            case PlatformFamily.OSX:
                StaticLibCheck.CheckMacOS(this, filePath);
                break;
            default:
                throw new NotSupportedException($"Platform {Environment.Platform.Family} is not supported for static library checks.");
        }

        this.Information("");
    }

    private static string CalculateVersion(ICakeContext context)
    {
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

            if (workflow.Repository != "MonoGame/MonoGame")
            {
                return $"{VersionBase}.{workflow.RunNumber}-{workflow.RepositoryOwner}";
            }
            else if (workflow.RefType == GitHubActionsRefType.Tag)
            {
                var baseVersion = workflow.RefName.Split('/')[^1];
                if (!VersionRegex.IsMatch(baseVersion))
                    throw new Exception($"Invalid tag: {baseVersion}");

                VersionBase = baseVersion[1..];
                return VersionBase;
            }
            else if (workflow.RefType == GitHubActionsRefType.Branch && workflow.RefName != "refs/heads/master")
            {
                return $"{VersionBase}.{workflow.RunNumber}-develop";
            }
            else
            {
                return $"{VersionBase}.{workflow.RunNumber}";
            }
        }
        else
        {
            return context.Argument("build-version", VersionBase + ".1-develop");
        }
    }
}