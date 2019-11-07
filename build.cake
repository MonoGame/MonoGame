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
    version = EnvironmentVariable("BUILD_NUMBER") ?? "1.0.0.0";

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
    DotNetCoreRestore("MonoGame.Framework.DesktopGL.sln");
    PackProject("MonoGame.Framework/MonoGame.Framework.DesktopGL.csproj");
});

Task("BuildWindowsDX")
    .IsDependentOn("Prep")
    .WithCriteria(() => IsRunningOnWindows())
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
    if (DirectoryExists("/usr/lib/xamarin.android"))
        return true;

    return DirectoryExists("/Developer/MonoAndroid");
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

Task("BuildiOS")
    .IsDependentOn("Prep")
    .WithCriteria(() =>
{
    return DirectoryExists("/Developer/MonoTouch");
}).Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.iOSCore.csproj");
    MSBuild("MonoGame.Framework/MonoGame.Framework.iOSCore.csproj", msPackSettings);
});

Task("BuildUWP")
    .IsDependentOn("Prep")
    .WithCriteria(() => GetMSBuildWith("Microsoft.VisualStudio.Component.Windows10SDK.17763"))
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework/MonoGame.Framework.WindowsUniversal.csproj");
    MSBuild("MonoGame.Framework/MonoGame.Framework.WindowsUniversal.csproj", msPackSettings);
});

Task("BuildContentPipeline")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("MonoGame.Framework.Content.Pipeline/MonoGame.Framework.Content.Pipeline.csproj");
    MSBuild("MonoGame.Framework.Content.Pipeline/MonoGame.Framework.Content.Pipeline.csproj", msPackSettings);
});

Task("BuildTools")
    .IsDependentOn("Prep")
    .Does(() =>
{
    DotNetCoreRestore("Tools/2MGFX/2MGFX.csproj");
    PackProject("Tools/2MGFX/2MGFX.csproj");

    DotNetCoreRestore("Tools/MGCB/MGCB.csproj");
    PackProject("Tools/MGCB/MGCB.csproj");

    DotNetCoreRestore("Tools/MonoGame.Content.Builder/MonoGame.Content.Builder.csproj");
    PackProject("Tools/MonoGame.Content.Builder/MonoGame.Content.Builder.csproj");
});

Task("PackVSTemplates")
    .Does(() =>
{
    var vsdirs = GetDirectories("./ProjectTemplates/VisualStudio20*");
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
    // pack core templates as a nuget
    DotNetCoreRestore("ProjectTemplates/DotNetTemplate/MonoGame.Templates.CSharp/MonoGame.Templates.CSharp.csproj");
    PackProject("ProjectTemplates/DotNetTemplate/MonoGame.Templates.CSharp/MonoGame.Templates.CSharp.csproj");
});

Task("PackWindows")
    .WithCriteria(() => IsRunningOnWindows())
    .IsDependentOn("BuildAll")
    .IsDependentOn("PackVSTemplates")
    .Does(() =>
{
    // The old build script passes defines through an nsh file, NSIS needs it to exist or it will crash
    // TODO remove this
    if (!FileExists("./Installers/Windows/header.nsh"))
        System.IO.File.Create("./Installers/Windows/header.nsh").Dispose();

    var settings = new MakeNSISSettings();
    settings.ToolPath = "C:/Program Files (x86)/NSIS/makensis.exe";
    settings.WorkingDirectory = "./Installers/Windows";
    settings.Defines = new Dictionary<string, string>()
    {
        { "FrameworkPath", Context.Environment.WorkingDirectory.CombineWithFilePath("Installers/").FullPath },
        { "VERSION", majorVersion},
        { "INSTALLERVERSION", version },
    };

    MakeNSIS("./Installers/Windows/MonoGame.nsi", settings);
});

Task("PackLinux")
    .IsDependentOn("BuildAll")
    .Does(() =>
{
});

Task("PackMac")
    .IsDependentOn("BuildAll")
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

Task("PackInstallers")
    .IsDependentOn("PackWindows")
    .IsDependentOn("PackLinux")
    .IsDependentOn("PackMac");

Task("Default")
    .IsDependentOn("PackInstallers");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
