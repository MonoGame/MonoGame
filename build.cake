#tool nuget:?package=vswhere&version=2.6.7
#tool nuget:?package=NUnit.ConsoleRunner&version=3.12.0
#addin nuget:?package=Cake.FileHelpers&version=3.3.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("build-target", "Default");
var version = Argument("build-version", EnvironmentVariable("BUILD_NUMBER") ?? "3.8.0.1");
var configuration = Argument("build-configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

MSBuildSettings msPackSettings, mdPackSettings;
DotNetCoreMSBuildSettings dnBuildSettings;
DotNetCorePackSettings dnPackSettings;

private void PackMSBuild(string filePath)
{
    MSBuild(filePath, msPackSettings);
}

private void PackDotnet(string filePath)
{
    DotNetCorePack(filePath, dnPackSettings);
}

private bool GetMSBuildWith(string requires)
{
    if (IsRunningOnWindows())
    {
        DirectoryPath vsLatest = VSWhereLatest(new VSWhereLatestSettings { Requires = requires });

        if (vsLatest != null)
        {
            var files = GetFiles(vsLatest.FullPath + "/**/MSBuild.exe");
            if (files.Any())
            {
                msPackSettings.ToolPath = files.First();
                return true;
            }
        }
    }

    return false;
}

private void ParseVersion()
{
    if (!string.IsNullOrEmpty(EnvironmentVariable("GITHUB_ACTIONS")))
    {
        version = "3.8.1." + EnvironmentVariable("GITHUB_RUN_NUMBER");

        var repositoryUrl = EnvironmentVariable("GITHUB_REPOSITORY");
        var branch = EnvironmentVariable("GITHUB_REF");

        if (!string.IsNullOrEmpty(repositoryUrl) && 
            repositoryUrl != "MonoGame/MonoGame") // If we are building a PR
        {
            var split = repositoryUrl.Split('/');
            version = version + "-" + split[0];
        }
        else if (repositoryUrl == "MonoGame/MonoGame" &&
            !string.IsNullOrEmpty(branch) ||
            branch != " refs/heads/master") // If we are building our repository
        {
            var branchName = branch.Split('/')[2];
            version = version + "-" + branchName;
        }
    }
    else
    {
        var branch = EnvironmentVariable("BRANCH_NAME") ?? string.Empty;    
        if (!branch.Contains("master"))
            version += "-develop";
    }

    Console.WriteLine("Version: " + version);
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Prep")
    .Does(() =>
{
    // Set MGFXC_WINE_PATH for building shaders on macOS and Linux
    System.Environment.SetEnvironmentVariable("MGFXC_WINE_PATH", EnvironmentVariable("HOME") + "/.winemonogame");

    ParseVersion();

    msPackSettings = new MSBuildSettings();
    msPackSettings.Verbosity = Verbosity.Minimal;
    msPackSettings.Configuration = configuration;
    msPackSettings.Restore = true;
    msPackSettings.WithProperty("Version", version);
    msPackSettings.WithTarget("Pack");

    mdPackSettings = new MSBuildSettings();
    mdPackSettings.Verbosity = Verbosity.Minimal;
    mdPackSettings.Configuration = configuration;
    mdPackSettings.WithProperty("Version", version);
    mdPackSettings.WithTarget("PackageAddin");

    dnBuildSettings = new DotNetCoreMSBuildSettings();
    dnBuildSettings.WithProperty("Version", version);

    dnPackSettings = new DotNetCorePackSettings();
    dnPackSettings.MSBuildSettings = dnBuildSettings;
    dnPackSettings.Verbosity = DotNetCoreVerbosity.Minimal;
    dnPackSettings.Configuration = configuration;
});

Task("BuildDesktopGL")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.DesktopGL.csproj");
    PackDotnet("MonoGame.Framework/MonoGame.Framework.DesktopGL.csproj");
});

Task("TestDesktopGL")
    .IsDependentOn("BuildDesktopGL")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    CreateDirectory("Artifacts/Tests/DesktopGL/Debug");
    DotNetCoreRun("../../../../Tests/MonoGame.Tests.DesktopGL.csproj", "", new DotNetCoreRunSettings
    {
        WorkingDirectory = "Artifacts/Tests/DesktopGL/Debug",
	    ArgumentCustomization = args => args.Append("--teamcity")
    });
});

Task("BuildWindowsDX")
    .IsDependentOn("Prep")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj");
    PackDotnet("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj");
});

Task("TestWindowsDX")
    .IsDependentOn("BuildWindowsDX")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    CreateDirectory("Artifacts/Tests/WindowsDX/Debug");
    DotNetCoreRun("../../../../Tests/MonoGame.Tests.WindowsDX.csproj", "", new DotNetCoreRunSettings
    {
        WorkingDirectory = "Artifacts/Tests/WindowsDX/Debug",
	    ArgumentCustomization = args => args.Append("--teamcity")
    });
});

Task("BuildAndroid")
    .IsDependentOn("Prep")
    .WithCriteria(() =>
{
    if (IsRunningOnWindows())
        return GetMSBuildWith("Component.Xamarin");

    return DirectoryExists("/Library/Frameworks/Xamarin.Android.framework");
}).Does(() =>
{
    PackMSBuild("MonoGame.Framework/MonoGame.Framework.Android.csproj");
});

Task("BuildiOS")
    .IsDependentOn("Prep")
    .WithCriteria(() =>
{
    return DirectoryExists("/Library/Frameworks/Xamarin.iOS.framework");
}).Does(() =>
{
    PackMSBuild("MonoGame.Framework/MonoGame.Framework.iOS.csproj");
});

Task("BuildUWP")
    .IsDependentOn("Prep")
    .WithCriteria(() => GetMSBuildWith("Microsoft.VisualStudio.Component.Windows10SDK.17763"))
    .Does(() =>
{
    PackMSBuild("MonoGame.Framework/MonoGame.Framework.WindowsUniversal.csproj");
});

Task("BuildContentPipeline")
    .IsDependentOn("Prep")
    .Does(() =>
{
    PackDotnet("Tools/MonoGame.Effect.Compiler/MonoGame.Effect.Compiler.csproj");

    PackDotnet("MonoGame.Framework.Content.Pipeline/MonoGame.Framework.Content.Pipeline.csproj");
});

Task("BuildTools")
    .IsDependentOn("BuildContentPipeline")
    .Does(() =>
{
    PackDotnet("Tools/MonoGame.Content.Builder/MonoGame.Content.Builder.csproj");

    PackDotnet("Tools/MonoGame.Effect.Compiler/MonoGame.Effect.Compiler.csproj");

    PackDotnet("Tools/MonoGame.Content.Builder.Task/MonoGame.Content.Builder.Task.csproj");

    PackDotnet("Tools/MonoGame.Packaging.Flatpak/MonoGame.Packaging.Flatpak.csproj");
    
    var versionReg = @"<key>CFBundleShortVersionString<\/key>\s*<string>([^\s]*)<\/string>";
    var plistPath = "Tools/MonoGame.Content.Builder.Editor/Info.plist";
    var newVersion = "<key>CFBundleShortVersionString</key>\n\t<string>" + version + "</string>";
    ReplaceRegexInFiles(plistPath, versionReg, newVersion, System.Text.RegularExpressions.RegexOptions.Singleline);
    
    if (IsRunningOnWindows())
        PackDotnet("Tools/MonoGame.Content.Builder.Editor/MonoGame.Content.Builder.Editor.Windows.csproj");
    
    PackDotnet("Tools/MonoGame.Content.Builder.Editor/MonoGame.Content.Builder.Editor.Linux.csproj");
    
    // if (IsRunningOnMacOs()) TODO: Update CAKE
    if (IsRunningOnUnix() && DirectoryExists("/Applications"))
        PackDotnet("Tools/MonoGame.Content.Builder.Editor/MonoGame.Content.Builder.Editor.Mac.csproj");

    PackDotnet("Tools/MonoGame.Content.Builder.Editor.Launcher/MonoGame.Content.Builder.Editor.Launcher.csproj");
});

Task("TestTools")
    .IsDependentOn("BuildTools")
    .Does(() =>
{
    CreateDirectory("Artifacts/Tests/Tools/" + configuration);
    DotNetCoreRun("../../../../Tools/MonoGame.Tools.Tests/MonoGame.Tools.Tests.csproj", "", new DotNetCoreRunSettings
    {
        WorkingDirectory = "Artifacts/Tests/Tools/" + configuration,
	    ArgumentCustomization = args => args.Append("--teamcity"),
        Configuration = configuration
    });
});

Task("PackDotNetTemplates")
    .IsDependentOn("Prep")
    .Does(() =>
{
    PackDotnet("Templates/MonoGame.Templates.CSharp/MonoGame.Templates.CSharp.csproj");
});

Task("PackVSTemplates")
    .IsDependentOn("PackDotNetTemplates")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    var dotnet = Context.Tools.Resolve("dotnet.exe");
    if (StartProcess(dotnet, "tool restore") != 0)
        throw new Exception("dotnet tool restore failed.");

    var result = StartProcess(
        dotnet,
        "vstemplate " +
       $"-s Artifacts/MonoGame.Templates.CSharp/Release/MonoGame.Templates.CSharp.{version}.nupkg " +
       $"--vsix Artifacts/MonoGame.Templates.CSharp/MonoGame.Templates.CSharp.{version}.vsix " +
        "@Templates/VisualStudio/settings.rsp");

    if (result != 0)
        throw new Exception("dotnet-vstemplate failed to create VSIX.");
});

Task("PackVSMacTemplates")
    .IsDependentOn("PackDotNetTemplates")
    .WithCriteria(() => IsRunningOnUnix() && DirectoryExists("/Applications") && DirectoryExists("/Library"))
    .Does(() =>
{
    DotNetCoreRestore("Templates/VisualStudioForMac/MonoGame.IDE.VisualStudioForMac.csproj");
    MSBuild("Templates/VisualStudioForMac/MonoGame.IDE.VisualStudioForMac.csproj", mdPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("SanityCheck")
    .IsDependentOn("Prep");

Task("BuildAll")
    .IsDependentOn("BuildDesktopGL")
    .IsDependentOn("BuildWindowsDX")
    .IsDependentOn("BuildAndroid")
    .IsDependentOn("BuildiOS")
    .IsDependentOn("BuildUWP")
    .IsDependentOn("BuildContentPipeline")
    .IsDependentOn("BuildTools");

Task("Pack")
    .IsDependentOn("BuildAll")
    .IsDependentOn("PackDotNetTemplates")
    .IsDependentOn("PackVSMacTemplates")
    .IsDependentOn("PackVSTemplates");

Task("Test")
    .IsDependentOn("TestWindowsDX")
    .IsDependentOn("TestDesktopGL")
    .IsDependentOn("TestTools");

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
