#tool nuget:?package=vswhere&version=2.6.7
#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

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

private void PackProject(string filePath)
{
    MSBuild(filePath, msPackSettings);
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

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Prep")
    .Does(() =>
{
    // We tag the version with the build branch to make it
    // easier to spot special builds in NuGet feeds.
    var branch = EnvironmentVariable("GIT_BRANCH") ?? string.Empty;
    if (branch == "develop")
	version += "-develop";

    Console.WriteLine("Build Version: {0}", version);

    msPackSettings = new MSBuildSettings();
    msPackSettings.Verbosity = Verbosity.Minimal;
    msPackSettings.Configuration = configuration;
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
    PackProject("MonoGame.Framework/MonoGame.Framework.DesktopGL.csproj");
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
    PackProject("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj");
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
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.Android.csproj");
    PackProject("MonoGame.Framework/MonoGame.Framework.Android.csproj");
});

Task("BuildiOS")
    .IsDependentOn("Prep")
    .WithCriteria(() =>
{
    return DirectoryExists("/Library/Frameworks/Xamarin.iOS.framework");
}).Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.iOS.csproj");
    PackProject("MonoGame.Framework/MonoGame.Framework.iOS.csproj");
});

Task("BuildUWP")
    .IsDependentOn("Prep")
    .WithCriteria(() => GetMSBuildWith("Microsoft.VisualStudio.Component.Windows10SDK.17763"))
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.WindowsUniversal.csproj");
    PackProject("MonoGame.Framework/MonoGame.Framework.WindowsUniversal.csproj");
});

Task("BuildContentPipeline")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework.Content.Pipeline/MonoGame.Framework.Content.Pipeline.csproj");
    PackProject("MonoGame.Framework.Content.Pipeline/MonoGame.Framework.Content.Pipeline.csproj");
});

Task("BuildTools")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("Tools/MonoGame.Content.Builder/MonoGame.Content.Builder.csproj");
    PackProject("Tools/MonoGame.Content.Builder/MonoGame.Content.Builder.csproj");
    
    DotNetCoreRestore("Tools/MonoGame.Effect.Compiler/MonoGame.Effect.Compiler.csproj");
    PackProject("Tools/MonoGame.Effect.Compiler/MonoGame.Effect.Compiler.csproj");
    
    DotNetCoreRestore("Tools/MonoGame.Content.Builder.Editor/MonoGame.Content.Builder.Editor.csproj");
    PackProject("Tools/MonoGame.Content.Builder.Editor/MonoGame.Content.Builder.Editor.csproj");

    DotNetCoreRestore("Tools/MonoGame.Content.Builder.Task/MonoGame.Content.Builder.Task.csproj");
    PackProject("Tools/MonoGame.Content.Builder.Task/MonoGame.Content.Builder.Task.csproj");

    DotNetCoreRestore("Tools/MonoGame.Packaging.Flatpak/MonoGame.Packaging.Flatpak.csproj");
    PackProject("Tools/MonoGame.Packaging.Flatpak/MonoGame.Packaging.Flatpak.csproj");
});

Task("TestTools")
    .IsDependentOn("BuildTools")
    .Does(() =>
{
    CreateDirectory("Artifacts/Tests/Tools/Debug");
    DotNetCoreRun("../../../../Tools/MonoGame.Tools.Tests/MonoGame.Tools.Tests.csproj", "", new DotNetCoreRunSettings
    {
        WorkingDirectory = "Artifacts/Tests/Tools/Debug",
	    ArgumentCustomization = args => args.Append("--teamcity")
    });
});

Task("PackDotNetTemplates")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("Templates/MonoGame.Templates.CSharp/MonoGame.Templates.CSharp.csproj");
    PackProject("Templates/MonoGame.Templates.CSharp/MonoGame.Templates.CSharp.csproj");
});

Task("PackVSTemplates")
    .IsDependentOn("PackDotNetTemplates")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    var vsdirs = GetDirectories("./Templates/VisualStudio20*");
    foreach (var vsdir in vsdirs)
    {
        DeleteFiles(vsdir.CombineWithFilePath("*.zip").FullPath);
        var projdirs = GetDirectories(vsdir.CombineWithFilePath("*").FullPath);
        foreach (var projdir in projdirs)
        {
            var outputPath = vsdir.CombineWithFilePath(projdir.GetDirectoryName() + ".zip");
                Zip(projdir, outputPath);
        }
    }
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
    .IsDependentOn("PackVSTemplates")
    .IsDependentOn("PackVSMacTemplates");

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
