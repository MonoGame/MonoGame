
namespace BuildScripts;

[TaskName("Build Native")]
[IsDependentOn(typeof(BuildMGFXCTask))]
[IsDependentOn(typeof(BuildNativeDependenciesTask))]
public sealed class BuildNativeTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var buildPremake = new BuildPremake();
        buildPremake.Run(context, "mgruntime", "native/monogame", "monogame.sln");

        context.DotNetPack(context.GetProjectPath(ProjectType.Framework, "Native"), context.DotNetPackSettings);
        context.DotNetPack("src/NuGetPackages/MonoGame.Framework/MonoGame.Framework.csproj", context.DotNetPackSettings);

        // MonoGame.Runtime.* NuGet packages are packed in the "Pack Native Runtime" task,
        // which downloads native binaries from all platform/arch build agents first.
        // This is necessary because Linux arm64 and x64 are built on separate runners.

        context.PublishBinaries("Native");
    }
}
