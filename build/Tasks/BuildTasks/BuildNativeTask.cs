using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildNative")]
[IsDependentOn(typeof(PrepTask))]
public sealed class BuildNativeTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore(ProjectPaths.MonoGameFrameworkNative, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameFrameworkNative, context.DotNetBuildSettings);
    }
}
