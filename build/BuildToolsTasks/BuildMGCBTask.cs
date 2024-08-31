
namespace BuildScripts;

[TaskName("Build mgcb")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
public sealed class BuildMGCBTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var builderPath = context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder");

        context.DotNetPack(builderPath, context.DotNetPackSettings);
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder.Task"), context.DotNetPackSettings);

        // ensure that the local development has the required dotnet tools.
        //  this won't actually include the tool manifest in a final build,
        //  but it will setup a local developer's project
        context.DotNetTool(builderPath, "tool install --create-manifest-if-needed mgcb-basisu");
        context.DotNetTool(builderPath, "tool install --create-manifest-if-needed mgcb-crunch");
    }
}
