
namespace BuildScripts;

[TaskName("Build Emscripten")]
[IsDependentOn(typeof(BuildNativeDependenciesTask))]
public sealed class BuildEmscriptenTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => !context.IsRunningOnWindows();
    public override void Run(BuildContext context)
    {
        var buildPremake = new BuildPremake();
        buildPremake.Run(context, "Emscripten", "native/monogame", "monogame.sln", "emscripten");

    }
}
