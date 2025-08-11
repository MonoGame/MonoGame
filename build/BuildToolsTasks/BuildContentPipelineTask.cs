
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

        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                context.CheckLib("native/mgpipeline/windows/Release/mgpipeline.dll");
                break;
            case PlatformFamily.Linux:
                context.CheckLib("native/mgpipeline/linux/Release/libmgpipeline.so");
                break;
            case PlatformFamily.OSX:
                context.CheckLib("native/mgpipeline/macosx/Release/libmgpipeline.dylib");
                break;
            default:
                throw new NotSupportedException($"Platform {context.Environment.Platform.Family} is not supported for static library checks.");
        }
    }
}
