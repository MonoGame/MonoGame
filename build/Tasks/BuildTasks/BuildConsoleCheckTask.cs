using Cake.Common;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildConsoleCheck")]
[IsDependentOn(typeof(PrepTask))]
public sealed class BuildConsoleCheckTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.IsRunningOnWindows();
    }

    public override void Run(BuildContext context)
    {
        context.DotNetRestore(ProjectPaths.MonoGameFrameworkConsoleCheck, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameFrameworkConsoleCheck, context.DotNetBuildSettings);
    }
}
