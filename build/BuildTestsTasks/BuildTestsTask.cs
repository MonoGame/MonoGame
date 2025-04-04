
namespace BuildScripts;

[TaskName("Build Tests")]
[IsDependentOn(typeof(BuildFrameworksTask))]
[IsDependentOn(typeof(BuildContentPipelineTask))]
public sealed class BuildTestsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild(context.GetProjectPath(ProjectType.Tests, "MonoGame.Tests.DesktopGL"), context.DotNetBuildSettings);
        if (context.IsRunningOnWindows())
            context.DotNetBuild(context.GetProjectPath(ProjectType.Tests, "MonoGame.Tests.WindowsDX"), context.DotNetBuildSettings);
    }
}