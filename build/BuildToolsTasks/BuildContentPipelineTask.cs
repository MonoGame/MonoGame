
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
                StaticLibCheck.CheckWindows(context, context.GetOutputPath("native/mgpipeline/windows/Release/mgpipeline.dll"));
                break;
            case PlatformFamily.Linux:
                StaticLibCheck.CheckLinux(context, context.GetOutputPath("native/mgpipeline/linux/Release/mgpipeline.so"));
                break;
            case PlatformFamily.OSX:
                StaticLibCheck.CheckMacOS(context, context.GetOutputPath("native/mgpipeline/macosx/Release/mgpipeline.dylib"));
                break;
            default:
                throw new NotSupportedException($"Platform {context.Environment.Platform.Family} is not supported for static library checks.");
        }
    }
}
