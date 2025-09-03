
namespace BuildScripts;

[TaskName("Build DesktopGL")]
[IsDependentOn(typeof(BuildShadersOGLTask))]
public sealed class BuildDesktopGLTask : FrostingTask<BuildContext>
{
    private string platformName = "DesktopGL";
    public override void Run(BuildContext context)
    {
        context.DotNetPack(context.GetProjectPath(ProjectType.Framework, platformName), context.DotNetPackSettings);

        context.PublishBinaries(platformName);
    }
}
