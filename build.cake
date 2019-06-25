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

MSBuildSettings mspacksettings;
DotNetCoreMSBuildSettings dnbuildsettings;
DotNetCorePackSettings dnpacksettings;

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Prep")
    .Does(() =>
{
    mspacksettings = new MSBuildSettings();
    mspacksettings.Verbosity = Verbosity.Minimal;
    mspacksettings.Configuration = configuration;
    mspacksettings = mspacksettings.WithProperty("Version", version);
    mspacksettings = mspacksettings.WithTarget("Pack");

    dnbuildsettings = new DotNetCoreMSBuildSettings();
    dnbuildsettings = dnbuildsettings.WithProperty("Version", version);

    dnpacksettings = new DotNetCorePackSettings();
    dnpacksettings.MSBuildSettings = dnbuildsettings;
    dnpacksettings.Verbosity = DotNetCoreVerbosity.Minimal;
    dnpacksettings.Configuration = configuration;
});

Task("BuildDesktopGL")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework.DesktopGL.sln");

    if (IsRunningOnWindows())
        DotNetCorePack("MonoGame.Framework/MonoGame.Framework.DesktopGL.csproj", dnpacksettings);
    else
        MSBuild("MonoGame.Framework/MonoGame.Framework.DesktopGL.csproj", mspacksettings);
});

Task("BuildWindowsDX")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework.WindowsDX.sln");

    if (IsRunningOnWindows())
        DotNetCorePack("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj", dnpacksettings);
    else
        MSBuild("MonoGame.Framework/MonoGame.Framework.WindowsDX.csproj", mspacksettings);
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("BuildDesktopGL")
    .IsDependentOn("BuildWindowsDX");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
