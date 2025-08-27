
namespace BuildScripts;

[TaskName("Build WindowsDX")]
[IsDependentOn(typeof(BuildShadersDX11Task))]
public sealed class BuildWindowsDXTask : FrostingTask<BuildContext>
{
    private string platformName = "WindowsDX";
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        context.DotNetPack(context.GetProjectPath(ProjectType.Framework, platformName), context.DotNetPackSettings);

        context.PublishBinaries(platformName);
    }
}
