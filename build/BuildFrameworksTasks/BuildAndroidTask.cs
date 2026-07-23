
namespace BuildScripts;

[TaskName("Build Android")]
[IsDependentOn(typeof(BuildShadersOGLTask))]
public sealed class BuildAndroidTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsWorkloadInstalled("android");

    public override void Run(BuildContext context)
    {
        var arguments = new DotNetMSBuildSettings();
        var androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME");
        if (!string.IsNullOrWhiteSpace(androidHome))
        {
            arguments.WithProperty("AndroidSdkDirectory", androidHome);
        }
        arguments.WithProperty("AcceptAndroidSDKLicenses", "true");
        arguments.WithTarget("InstallAndroidDependencies");
        var installSettings = new DotNetBuildSettings
        {
            MSBuildSettings = arguments,
            Verbosity = DotNetVerbosity.Minimal,
            Configuration = context.DotNetPackSettings.Configuration,
        };

        context.DotNetBuild(context.GetProjectPath(ProjectType.Framework, "Android"), installSettings);
        context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "Android"), context.DotNetPackSettings);
    }
}
