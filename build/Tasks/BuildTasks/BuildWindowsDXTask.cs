using Cake.Common;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildWindowsDX")]
[IsDependentOn(typeof(PrepTask))]
public sealed class BuildWindowsDXTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.IsRunningOnWindows();
    }

    public override void Run(BuildContext context)
    {
        context.DotNetRestore(ProjectPaths.MonoGameFrameworkWindowsDX, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameFrameworkWindowsDX, context.DotNetBuildSettings);
    }
}
