#tool nuget:?package=vswhere&version=2.6.7
#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("build-target", "Default");
string version = null;
if (HasArgument("build-version"))
    version = Argument<string>("build-version");
var configuration = Argument("build-configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var majorVersion = "3.0";
if (version == null)
    version = EnvironmentVariable("BUILD_NUMBER") ?? "3.8.0.1";

MSBuildSettings msPackSettings;
DotNetCoreMSBuildSettings dnBuildSettings;
DotNetCorePackSettings dnPackSettings;

private void PackProject(string filePath)
{
    // Windows and Linux dotnet tool does not allow building of .NET
    // projects, as such we must call msbuild on these platforms.
    if (IsRunningOnWindows())
        DotNetCorePack(filePath, dnPackSettings);
    else
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
    msPackSettings = new MSBuildSettings();
    msPackSettings.Verbosity = Verbosity.Minimal;
    msPackSettings.Configuration = configuration;
    msPackSettings.WithProperty("Version", version);
    msPackSettings.WithTarget("Pack");

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

Task("BuildWindowsDX")
    .IsDependentOn("Prep")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj");
    PackProject("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj");
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
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.UWP.csproj");
    PackProject("MonoGame.Framework/MonoGame.Framework.UWP.csproj");
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
    
    DotNetCoreRestore("Tools/MonoGame.Content.Builder.Task/MonoGame.Content.Builder.Task.csproj");
    PackProject("Tools/MonoGame.Content.Builder.Task/MonoGame.Content.Builder.Task.csproj");

    DotNetCoreRestore("Tools/MonoGame.Packaging.Flatpak/MonoGame.Packaging.Flatpak.csproj");
    PackProject("Tools/MonoGame.Packaging.Flatpak/MonoGame.Packaging.Flatpak.csproj");
});

Task("PackDotNetTemplates")
    .Does(() =>
{
    DotNetCoreRestore("Templates/MonoGame.Templates.CSharp/MonoGame.Templates.CSharp.csproj");
    PackProject("Templates/MonoGame.Templates.CSharp/MonoGame.Templates.CSharp.csproj");
});

Task("PackVSTemplates")
    .IsDependentOn("PackDotNetTemplates")
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

Task("PackWindows")
    .WithCriteria(() => IsRunningOnWindows())
    .IsDependentOn("BuildAll")
    .IsDependentOn("PackVSTemplates")
    .Does(() =>
{
});

Task("PackLinux")
    .IsDependentOn("BuildAll")
    .IsDependentOn("PackDotNetTemplates")
    .Does(() =>
{
});

Task("PackMac")
    .IsDependentOn("BuildAll")
    .IsDependentOn("PackDotNetTemplates")
    .Does(() =>
{
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
    .IsDependentOn("PackWindows")
    .IsDependentOn("PackLinux")
    .IsDependentOn("PackMac");

Task("Default")
    .IsDependentOn("Pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
