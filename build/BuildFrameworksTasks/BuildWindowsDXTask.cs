
namespace BuildScripts;

[TaskName("Build WindowsDX")]
public sealed class BuildWindowsDXTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "WindowsDX"), context.DotNetPackSettings);
}
