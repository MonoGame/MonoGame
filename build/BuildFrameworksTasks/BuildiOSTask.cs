
namespace BuildScripts;

[TaskName("Build iOS")]
[IsDependentOn(typeof(BuildShadersOGLTask))]
public sealed class BuildiOSTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsWorkloadInstalled("ios");

    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "iOS"), context.DotNetPackSettings);
}

