
namespace BuildScripts;

[TaskName("Build DesktopVK")]
public sealed class BuildDesktopVKTask : FrostingTask<BuildContext>
{
    // TEMP: Until OSX and Linux is setup to work.
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        int exit = context.XMake(@"src\monogame\", "f -m debug -v -y");
        if (exit < 0)
            throw new Exception($"Setting debug config failed! {exit}");

        exit = context.XMake(@"src\monogame\", "-r");
        if (exit < 0)
            throw new Exception($"Rebuild debug failed! {exit}");

        exit = context.XMake(@"src\monogame\", "f -m release -v -y");
        if (exit < 0)
            throw new Exception($"Setting release config failed! {exit}");

        exit = context.XMake(@"src\monogame\", "-r");
        if (exit < 0)
            throw new Exception($"Rebuild release failed! {exit}");
    }
}
