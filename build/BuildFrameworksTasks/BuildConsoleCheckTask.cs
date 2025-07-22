
namespace BuildScripts;

[TaskName("Build ConsoleCheck")]
public sealed class BuildConsoleCheckTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        // ConsoleCheck is getting switched to netstandard2.1 target framework next week
        // temporarely disable its compilation so that this compiles due to changes in TitleContainer.cs
        //
        // context.DotNetBuild(context.GetProjectPath(ProjectType.Framework, "ConsoleCheck"), context.DotNetBuildSettings);
    }
}
