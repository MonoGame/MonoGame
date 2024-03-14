
namespace BuildScripts;

[TaskName("Build mgfxc")]
public sealed class BuildMGFXCTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Effect.Compiler"), context.DotNetPackSettings);
}
