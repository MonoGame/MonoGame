
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

        if (context.Environment.Platform.Family == PlatformFamily.Windows)
        {
            exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = "native/pipeline", Arguments = "--verbose vs2022" });
            if (exit != 0)
                throw new Exception($"Native Pipeline Premake generation failed! {exit}");

            exit = context.StartProcess("msbuild", new ProcessSettings { WorkingDirectory = "native/pipeline", Arguments = "pipeline.sln /p:Configuration=Release /p:Platform=x64" });
            if (exit != 0)
                throw new Exception($"Native Pipeline build failed! {exit}");
        }
        else
        {
            exit = context.StartProcess("premake5", new ProcessSettings { WorkingDirectory = "native/pipeline", Arguments = "gmake2" });
            if (exit != 0)
                throw new Exception($"Native Pipeline Premake generation failed! {exit}");

            exit = context.StartProcess("make", new ProcessSettings { WorkingDirectory = "native/pipeline", Arguments = "config=release" });
        }

        var builderPath = context.GetProjectPath(ProjectType.ContentPipeline);
        context.DotNetPackSettings.MSBuildSettings.WithProperty("DisableMonoGameToolAssets", "True");
        context.DotNetPack(builderPath, context.DotNetPackSettings);
        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableMonoGameToolAssets");
    }
}
