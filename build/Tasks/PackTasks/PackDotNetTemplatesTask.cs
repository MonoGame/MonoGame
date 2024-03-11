using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackDotNetTemplates")]
[IsDependentOn(typeof(PrepTask))]
public sealed class PackDotNetTemplatesTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPack(ProjectPaths.MonoGameTemplatesCSharp, context.DotNetPackSettings);
    }
}
