
namespace BuildScripts;

[TaskName("Build Content Pipeline")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var builderPath = context.GetProjectPath(ProjectType.ContentPipeline);
        context.DotNetPack(builderPath, context.DotNetPackSettings);
    }
}
