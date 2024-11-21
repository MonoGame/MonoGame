
namespace BuildScripts;

[TaskName("Build Android")]
public sealed class BuildAndroidTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsWorkloadInstalled("android");

    public override void Run(BuildContext context)
    {
        var arguments = new DotNetMSBuildSettings();
        arguments.WithProperty("AndroidSdkDirectory", System.Environment.GetEnvironmentVariable ("ANDROID_HOME"));
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
