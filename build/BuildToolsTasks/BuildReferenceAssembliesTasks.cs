
namespace BuildScripts;

[TaskName("Build ReferenceAssemblies")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
[IsDependentOn(typeof(BuildDesktopGLTask))]
public sealed class BuildReferenceAssembliesTasks : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context) {
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Framework.Ref"), context.DotNetPackSettings);
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Framework.Content.Pipeline.Ref"), context.DotNetPackSettings);
    }
}