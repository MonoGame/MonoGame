using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using MonoGame.Framework.Build.Tasks.BuildTasks;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackNative")]
[IsDependentOn(typeof(BuildNativeTask))]
public sealed class PackNativeTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPack(ProjectPaths.MonoGameFrameworkNative, context.DotNetPackSettings);
    }
}
