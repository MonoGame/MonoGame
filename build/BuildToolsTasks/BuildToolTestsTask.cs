
namespace BuildScripts;

[TaskName("Build Tool Tests")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
public sealed class BuildToolTestsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetBuild(context.GetProjectPath(ProjectType.Tools, "MonoGame.Tools.Tests"), context.DotNetBuildSettings);
    }
}