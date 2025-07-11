
namespace BuildScripts;

[TaskName("Build Content Pipeline")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.ShellWorkingDir = "native/pipeline";
        context.Shell("premake5", "clean");
        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                context.Shell("premake5", "--verbose vs2022");
                context.Shell("msbuild", "pipeline.sln /p:Configuration=Release /p:Platform=x64");
                break;
            case PlatformFamily.OSX:
            case PlatformFamily.Linux:
                context.Shell("premake5", "gmake2");
                context.Shell("make", "config=release");
                break;
            default:
                throw new NotSupportedException($"Platform {context.Environment.Platform.Family} is not supported for building the content pipeline.");
        }
        StaticLibCheck.Check(context, "native/pipeline");

        var builderPath = context.GetProjectPath(ProjectType.ContentPipeline);
        context.DotNetPackSettings.MSBuildSettings.WithProperty("DisableMonoGameToolAssets", "True");
        context.DotNetPack(builderPath, context.DotNetPackSettings);
        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableMonoGameToolAssets");
    }
}
