
namespace BuildScripts;

[TaskName("Build Debug Adapters")]
[IsDependentOn(typeof(BuildDesktopGLTask))]
public sealed class BuildDevToolsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.DevTools), context.DotNetPackSettings);
}
