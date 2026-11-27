
namespace BuildScripts;

[TaskName("Build Native")]
[IsDependentOn(typeof(BuildMGFXCTask))]
[IsDependentOn(typeof(BuildShadersTask))]
[IsDependentOn(typeof(BuildNativeDependenciesTask))]
public sealed class BuildNativeTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // On the GitHub Linux CI, docker is used to run the native build first, which would create
        // this directory as `root` user, which then blocks the browser build from creating
        // the output directory it needs. To get around this, we create the directory as the
        // process user first here
        context.CreateDirectory("Artifacts/native/mgruntime");

        var buildPremake = new BuildPremake();
        buildPremake.Run(context, "mgruntime", "native/monogame", "monogame.sln");
        buildPremake.Run(context, "mgpipeline", "native/pipeline", "pipeline.sln");

        context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "Native"), context.DotNetPackSettings);
        context.DotNetPack("src/NuGetPackages/MonoGame.Framework/MonoGame.Framework.csproj", context.DotNetPackSettings);

        // MonoGame.Runtime.* NuGet packages are packed in the "Pack Native Runtime" task,
        // which downloads native binaries from all platform/arch build agents first.
        // This is necessary because Linux arm64 and x64 are built on separate runners.

        context.PublishBinaries("Native");
    }
}
