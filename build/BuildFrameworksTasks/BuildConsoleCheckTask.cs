
namespace BuildScripts;

[TaskName("Build ConsoleCheck")]
public sealed class BuildConsoleCheckTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
        => context.DotNetBuild(context.GetProjectPath(ProjectType.Framework, "ConsoleCheck"), context.DotNetBuildSettings);
}
