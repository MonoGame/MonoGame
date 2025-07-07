
namespace BuildScripts;

[TaskName("Build Content Pipeline")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        int exit;
        exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = "native/pipeline", Arguments = "clean" });
        if (exit != 0)
            throw new Exception($"Native Pipeline Premake clean failed! {exit}");

        var premakeArguments = string.Empty;
        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                premakeArguments = "--verbose vs2022";
                break;
            case PlatformFamily.Linux or PlatformFamily.OSX:
                premakeArguments = "gmake2";
                break;
            default:
                throw new NotSupportedException($"Platform {context.Environment.Platform.Family} is not supported for building the content pipeline.");
        }

        exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = "native/pipeline", Arguments = premakeArguments });
        if (exit != 0)
            throw new Exception($"Native Pipeline Premake generation failed! {exit}");

        if (context.Environment.Platform.Family == PlatformFamily.Windows)
        {
            exit = context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = "native/pipeline", Arguments = "pipeline.sln /p:Configuration=Release /p:Platform=x64" });
            if (exit != 0)
                throw new Exception($"Native Pipeline build failed with msbuild! {exit}");
        }
        else
        {
            exit = context.StartProcess("make", new ProcessSettings { WorkingDirectory = "native/pipeline", Arguments = "config=release" });
            if (exit != 0)
                throw new Exception($"Native Pipeline build failed with make! {exit}");
        }

        StaticLibCheck staticLibCheck = new StaticLibCheck();
        staticLibCheck.Check(context, "native/pipeline");

        var builderPath = context.GetProjectPath(ProjectType.ContentPipeline);
        context.DotNetPackSettings.MSBuildSettings.WithProperty("DisableMonoGameToolAssets", "True");
        context.DotNetPack(builderPath, context.DotNetPackSettings);
        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableMonoGameToolAssets");
    }
}
