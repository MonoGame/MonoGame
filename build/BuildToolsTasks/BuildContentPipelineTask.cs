
namespace BuildScripts;

[TaskName("Build Content Pipeline")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var builderPath = context.GetProjectPath(ProjectType.ContentPipeline);
        context.DotNetPackSettings.MSBuildSettings.WithProperty("DisableMonoGameToolAssets", "True");
        context.DotNetPack(builderPath, context.DotNetPackSettings);
        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableMonoGameToolAssets");
    }
}
