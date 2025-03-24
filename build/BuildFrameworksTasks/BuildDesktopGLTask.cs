
namespace BuildScripts;

[TaskName("Build DesktopGL")]
public sealed class BuildDesktopGLTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "DesktopGL"), context.DotNetPackSettings);
}
