
namespace BuildScripts;

[TaskName("Build Content Pipeline")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var builderPath = context.GetProjectPath(ProjectType.ContentPipeline);
        context.DotNetPack(builderPath, context.DotNetPackSettings);

        // ensure that the local development has the required dotnet tools.
        //  this won't actually include the tool manifest in a final build,
        //  but it will setup a local developer's project
        context.DotNetTool(builderPath, "tool install --create-manifest-if-needed mgcb-basisu");
        context.DotNetTool(builderPath, "tool install --create-manifest-if-needed mgcb-crunch");
    }
}
