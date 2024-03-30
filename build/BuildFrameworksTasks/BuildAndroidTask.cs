
namespace BuildScripts;

[TaskName("Build Android")]
public sealed class BuildAndroidTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsWorkloadInstalled("android");

    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "Android"), context.DotNetPackSettings);
}
