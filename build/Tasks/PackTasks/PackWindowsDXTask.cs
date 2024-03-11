using Cake.Common;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using MonoGame.Framework.Build.Tasks.BuildTasks;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackWindowsDX")]
[IsDependentOn(typeof(BuildWindowsDXTask))]
public sealed class PackWindowsDXTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.IsRunningOnWindows();
    }

    public override void Run(BuildContext context)
    {
        context.DotNetPack(ProjectPaths.MonoGameFrameworkWindowsDX, context.DotNetPackSettings);
    }
}
