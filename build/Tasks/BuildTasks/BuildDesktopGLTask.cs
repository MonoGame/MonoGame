using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildDesktopGL")]
[IsDependentOn(typeof(PrepTask))]
public sealed class BuildDesktopGLTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore(ProjectPaths.MonoGameFrameworkDesktopGL, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameFrameworkDesktopGL, context.DotNetBuildSettings);
    }
}
