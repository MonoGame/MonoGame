
namespace BuildScripts;

[TaskName("Build Content Pipeline")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var buildPremake = new BuildPremake();
        buildPremake.Run(context, "Content Pipeline", "native/pipeline", "pipeline.sln");

        StaticLibCheck staticLibCheck = new StaticLibCheck();
        var platform = context.Environment.Platform.Family switch
        {
            PlatformFamily.Windows => "windows",
            PlatformFamily.OSX => "macosx",
            PlatformFamily.Linux => "linux",
            _ => throw new Exception("Unsupported platform family: " + context.Environment.Platform.Family)
        };
        staticLibCheck.Check(context, $"Artifacts/native/mgpipeline/{platform}/Release");

        var builderPath = context.GetProjectPath(ProjectType.ContentPipeline);
        context.DotNetPackSettings.MSBuildSettings.WithProperty("DisableMonoGameToolAssets", "True");
        context.DotNetPack(builderPath, context.DotNetPackSettings);
        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableMonoGameToolAssets");
    }
}
