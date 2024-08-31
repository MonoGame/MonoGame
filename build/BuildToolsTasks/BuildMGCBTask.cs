
namespace BuildScripts;

[TaskName("Build mgcb")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
public sealed class BuildMGCBTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder"), context.DotNetPackSettings);
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder.Task"), context.DotNetPackSettings);
    }
}
