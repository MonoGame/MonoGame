using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using MonoGame.Framework.Build.Tasks.BuildTasks;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackDesktopGL")]
[IsDependentOn(typeof(BuildDesktopGLTask))]
public sealed class PackDesktopGLTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPack(ProjectPaths.MonoGameFrameworkDesktopGL, context.DotNetPackSettings);
    }
}
