#tool nuget:?package=vswhere&version=2.8.4
#tool nuget:?package=NUnit.ConsoleRunner&version=3.13.2
#addin nuget:?package=Cake.FileHelpers&version=5.0.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("build-target", "Default");
var version = Argument("build-version", EnvironmentVariable("BUILD_NUMBER") ?? "3.8.1.1");
var configuration = Argument("build-configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

MSBuildSettings msPackSettings, mdPackSettings, msBuildSettings;
DotNetMSBuildSettings dnBuildSettings;
DotNetPackSettings dnPackSettings;

private void PackMSBuild(string filePath)
{
    MSBuild(filePath, msPackSettings);
}

private void PackDotnet(string filePath)
{
    DotNetPack(filePath, dnPackSettings);
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
        if (EnvironmentVariable("GITHUB_REF") != "refs/heads/master")
            version += "-develop";
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

    msBuildSettings = new MSBuildSettings();
    msBuildSettings.Verbosity = Verbosity.Minimal;
    msBuildSettings.Configuration = configuration;
    msBuildSettings.WithProperty("Version", version);

    dnBuildSettings = new DotNetMSBuildSettings();
    dnBuildSettings.WithProperty("Version", version);

    dnPackSettings = new DotNetPackSettings();
    dnPackSettings.MSBuildSettings = dnBuildSettings;
    dnPackSettings.Verbosity = DotNetVerbosity.Minimal;
    dnPackSettings.Configuration = configuration;
});

Task("BuildDesktopGL")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetRestore("MonoGame.Framework/MonoGame.Framework.DesktopGL.csproj");
    PackDotnet("MonoGame.Framework/MonoGame.Framework.DesktopGL.csproj");
});

Task("TestDesktopGL")
    .IsDependentOn("BuildDesktopGL")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    CreateDirectory("Artifacts/Tests/DesktopGL/Debug");
    DotNetRun("../../../../Tests/MonoGame.Tests.DesktopGL.csproj", "", new DotNetRunSettings
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
    DotNetRestore("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj");
    PackDotnet("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj");
});

Task("TestWindowsDX")
    .IsDependentOn("BuildWindowsDX")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    CreateDirectory("Artifacts/Tests/WindowsDX/Debug");
    DotNetRun("../../../../Tests/MonoGame.Tests.WindowsDX.csproj", "", new DotNetRunSettings
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
    PackDotnet("MonoGame.Framework/MonoGame.Framework.Android.csproj");
});

Task("BuildiOS")
    .IsDependentOn("Prep")
    .WithCriteria(() =>
{
    if (IsRunningOnWindows())
        return GetMSBuildWith("Component.Xamarin");

    return DirectoryExists("/Library/Frameworks/Xamarin.iOS.framework");
}).Does(() =>
{
    PackDotnet("MonoGame.Framework/MonoGame.Framework.iOS.csproj");
});

Task("BuildUWP")
    .IsDependentOn("Prep")
    .WithCriteria(() => GetMSBuildWith("Microsoft.VisualStudio.Component.Windows10SDK.19041"))
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
    
    if (IsRunningOnMacOs())
        PackDotnet("Tools/MonoGame.Content.Builder.Editor/MonoGame.Content.Builder.Editor.Mac.csproj");

    PackDotnet("Tools/MonoGame.Content.Builder.Editor.Launcher/MonoGame.Content.Builder.Editor.Launcher.csproj");
});

Task("TestTools")
    .IsDependentOn("BuildTools")
    .Does(() =>
{
    CreateDirectory("Artifacts/Tests/Tools/" + configuration);
    DotNetRun("../../../../Tools/MonoGame.Tools.Tests/MonoGame.Tools.Tests.csproj", "", new DotNetRunSettings
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
    DotNetRestore("Templates/MonoGame.Templates.VisualStudio/MonoGame.Templates.VSExtension.csproj");
    MSBuild("Templates/MonoGame.Templates.VisualStudio/MonoGame.Templates.VSExtension.csproj", msBuildSettings);
});

Task("PackVSMacTemplates")
    .IsDependentOn("PackDotNetTemplates")
    .WithCriteria(() => IsRunningOnUnix() && DirectoryExists("/Applications") && DirectoryExists("/Library"))
    .Does(() =>
{
    // DotNetCoreRestore("Templates/VisualStudioForMac/MonoGame.IDE.VisualStudioForMac.csproj");
    // MSBuild("Templates/VisualStudioForMac/MonoGame.IDE.VisualStudioForMac.csproj", mdPackSettings);
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
