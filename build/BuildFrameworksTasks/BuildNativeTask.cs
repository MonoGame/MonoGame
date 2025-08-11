
namespace BuildScripts;

[TaskName("Build Native")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildNativeTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "Native"), context.DotNetPackSettings);
}
