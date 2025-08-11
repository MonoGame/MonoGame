
namespace BuildScripts;

[TaskName("Build WindowsDX")]
[IsDependentOn(typeof(BuildShadersDX11Task))]
public sealed class BuildWindowsDXTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "WindowsDX"), context.DotNetPackSettings);
}
