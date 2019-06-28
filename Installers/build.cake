//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("build-target", "Default");
var version = Argument("build-version", "1.0.0.0");


//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var majorVersion = "3.0";
var buildNumber = EnvironmentVariable("BUILD_NUMBER");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("PackWindows")
    .WithCriteria(() => IsRunningOnWindows())
    .Does(() =>
{
    // The old build script passes defines through an nsh file, NSIS needs it to exist or it will crash
    // TODO remove this
    if (!FileExists("./Windows/header.nsh"))
        System.IO.File.Create("./Windows/header.nsh").Dispose();

    var settings = new MakeNSISSettings();
    settings.ToolPath = "C:/Program Files (x86)/NSIS/makensis.exe";
    settings.WorkingDirectory = "./Windows";
    settings.Defines = new Dictionary<string, string>()
    {
        { "FrameworkPath", Context.Environment.WorkingDirectory.FullPath },
        { "VERSION", majorVersion},
        { "INSTALLERVERSION", buildNumber },
    };

    MakeNSIS("./Windows/MonoGame.nsi", settings);
});

Task("PackLinux")
    .Does(() =>
{
});

Task("PackMac")
    .Does(() =>
{
});


//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("PackWindows")
    .IsDependentOn("PackLinux")
    .IsDependentOn("PackMac");


//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

