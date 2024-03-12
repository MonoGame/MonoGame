
namespace BuildScripts;

[TaskName("Build Content Pipeline")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.ContentPipeline), context.DotNetPackSettings);
}
