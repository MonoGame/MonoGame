
namespace BuildScripts;

[TaskName("Build Native")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildNativeTask : FrostingTask<BuildContext>
{
    private string platformName = "Native";
    public override void Run(BuildContext context)
    {
        context.DotNetPack(context.GetProjectPath(ProjectType.Framework, platformName), context.DotNetPackSettings);

        context.PublishBinaries(platformName);
    }
}
