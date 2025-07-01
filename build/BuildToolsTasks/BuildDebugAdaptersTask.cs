
namespace BuildScripts;

[TaskName("Build Debug Adapters")]
[IsDependentOn(typeof(BuildDesktopGLTask))]
public sealed class BuildDebugAdaptersTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.DebugAdapters), context.DotNetPackSettings);
}
