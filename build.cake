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

    dnPackSettings = new DotNetCorePackSettings();
    dnPackSettings.MSBuildSettings = dnBuildSettings;
    dnPackSettings.Verbosity = DotNetCoreVerbosity.Minimal;
    dnPackSettings.Configuration = configuration;
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
    .WithCriteria(() =>
{
    if (IsRunningOnWindows())
        return GetMSBuildWith("Component.Xamarin");

    // Xamarin Android on Linux needs to be installed in this specific dir
    // We don't have Mac support... yet!
    return DirectoryExists("/usr/lib/xamarin.android");
}).Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.AndroidCore.csproj");

    var buildSettings = msPackSettings;
    if (DirectoryExists("/usr/lib/xamarin.android"))
    {
        // Some weird bug when packing xamarin andoid assembly from Linux
        // Easy workaround at least :)
        buildSettings = buildSettings.WithProperty("DesignTimeBuild", "true");
    }

    MSBuild("MonoGame.Framework/MonoGame.Framework.AndroidCore.csproj", buildSettings);
});

Task("BuildUWP")
    .IsDependentOn("Prep")
    .WithCriteria(() => GetMSBuildWith("Microsoft.VisualStudio.Component.Windows10SDK.17763"))
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.UWP.csproj");
    MSBuild("MonoGame.Framework/MonoGame.Framework.UWP.csproj", msPackSettings);
});

Task("BuildContentPipeline")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework.Content.Pipeline/MonoGame.Framework.Content.Pipeline.csproj");
    MSBuild("MonoGame.Framework.Content.Pipeline/MonoGame.Framework.Content.Pipeline.csproj", msPackSettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("BuildDesktopGL")
    .IsDependentOn("BuildWindowsDX")
    .IsDependentOn("BuildAndroid")
    .IsDependentOn("BuildUWP")
    .IsDependentOn("BuildContentPipeline");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
