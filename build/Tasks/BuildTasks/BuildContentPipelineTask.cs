using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildContentPipeline")]
[IsDependentOn(typeof(PrepTask))]
public sealed class BuildContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore(ProjectPaths.MonoGameEffectCompiler, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameEffectCompiler, context.DotNetBuildSettings);

        context.DotNetRestore(ProjectPaths.MonoGameFrameworkContentPipeline, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameFrameworkContentPipeline, context.DotNetBuildSettings);
    }
}
