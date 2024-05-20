
namespace BuildScripts;

[TaskName("Build DesktopVK")]
public sealed class BuildDesktopVKTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "DesktopVK"), context.DotNetPackSettings);
}
