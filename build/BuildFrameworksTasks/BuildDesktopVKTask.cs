
namespace BuildScripts;

[TaskName("Build DesktopVK")]
[IsDependentOn(typeof(BuildNativeDependenciesTask))]
public sealed class BuildDesktopVKTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var buildPremake = new BuildPremake();
        buildPremake.Run(context, "DesktopVK", "native/monogame", "monogame.sln");

    }
}
