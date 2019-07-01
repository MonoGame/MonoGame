#tool nuget:?package=vswhere&version=2.6.7
#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("build-target", "Default");
var version = Argument("build-version", "1.0.0.0");
var configuration = Argument("build-configuration", "Release");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

MSBuildSettings msPackSettings;
DotNetCoreMSBuildSettings dnBuildSettings;
DotNetCorePackSettings dnPackSettings;

FilePath androidToolPath;
FilePath uwpToolPath;

private MSBuildSettings GetMSBuildPackSettings()
{
    var s = new MSBuildSettings();
    s.Verbosity = Verbosity.Minimal;
    s.Configuration = configuration;
    s.WithProperty("Version", version);
    s.WithTarget("Pack");
    return s;
}

private void PackProject(string filePath)
{
    // Windows and Linux dotnet tool does not allow building of .NET
    // projects, as such we must call msbuild on these platforms.
    if (IsRunningOnWindows())
        DotNetCorePack(filePath, dnPackSettings);
    else
        MSBuild(filePath, msPackSettings);
}

private FilePath GetMSBuildWith(string requires)
{
    if (IsRunningOnWindows())
    {
        DirectoryPath vsLatest = VSWhereLatest(new VSWhereLatestSettings { Requires = requires});

        if (vsLatest != null)
        {
            var files = GetFiles(vsLatest.FullPath + "/**/MSBuild.exe");
            if (files.Any())
                return files.First();
        }
    }

    return null;
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Prep")
    .Does(() =>
{
    msPackSettings = GetMSBuildPackSettings();

    dnPackSettings = new DotNetCorePackSettings();
    dnPackSettings.MSBuildSettings = dnBuildSettings;
    dnPackSettings.Verbosity = DotNetCoreVerbosity.Minimal;
    dnPackSettings.Configuration = configuration;

    androidToolPath = GetMSBuildWith("Component.Xamarin");

    if (IsRunningOnWindows())
        uwpToolPath = GetMSBuildWith("Microsoft.VisualStudio.Component.Windows10SDK.17763"); 
});

Task("BuildDesktopGL")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework.DesktopGL.sln");
    PackProject("MonoGame.Framework/MonoGame.Framework.DesktopGL.csproj");
});

Task("BuildWindowsDX")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework.WindowsDX.sln");
    PackProject("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj");
});

Task("BuildAndroid")
    .IsDependentOn("Prep")
    .Does(() =>
{
    if (androidToolPath != null)
    {
        var packSettings = GetMSBuildPackSettings();
        packSettings.ToolPath = androidToolPath;
        DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.AndroidCore.csproj");
        MSBuild("MonoGame.Framework/MonoGame.Framework.AndroidCore.csproj", packSettings);
    }
    else
    {
        Warning("Skipping Android build: MSBuild not found or Xamarin is not installed.");
    }
});

Task("BuildUWP")
    .IsDependentOn("Prep")
    .Does(() =>
{
    if (uwpToolPath != null)
    {
        var packSettings = GetMSBuildPackSettings();
        packSettings.ToolPath = uwpToolPath;
        DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.UWP.csproj");
        MSBuild("MonoGame.Framework/MonoGame.Framework.UWP.csproj", packSettings);
    }
    else
    {
        Warning("Skipping UWP build: MSBuild not found or UWP workload (with SDK 17763) is not installed.");
    }
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("BuildDesktopGL")
    .IsDependentOn("BuildWindowsDX")
    .IsDependentOn("BuildAndroid")
    .IsDependentOn("BuildUWP");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
