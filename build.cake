#tool nuget:?package=vswhere&version=2.6.7
#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin nuget:?package=Cake.FileHelpers&version=3.2.1

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

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Prep")
    .Does(() =>
{
    // Set MGFXC_WINE_PATH for building shaders on macOS and Linux
    System.Environment.SetEnvironmentVariable("MGFXC_WINE_PATH", EnvironmentVariable("HOME") + "/.winemonogame");

    // We tag the version with the build branch to make it
    // easier to spot special builds in NuGet feeds.
    var branch = EnvironmentVariable("GIT_BRANCH") ?? string.Empty;
    if (branch == "develop")
	version += "-develop";

    Console.WriteLine("Build Version: {0}", version);

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
    
    PackDotnet("Tools/MonoGame.Content.Builder.Editor/MonoGame.Content.Builder.Editor.csproj");
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
