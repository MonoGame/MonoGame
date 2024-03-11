using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using MonoGame.Framework.Build.Tasks.BuildTasks;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackContentPipeline")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
public sealed class PackContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPack(ProjectPaths.MonoGameEffectCompiler, context.DotNetPackSettings);
        context.DotNetPack(ProjectPaths.MonoGameFrameworkContentPipeline, context.DotNetPackSettings);
    }
}
