using Cake.Common.Tools.MSBuild;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildUWP")]
[IsDependentOn(typeof(PrepTask))]
public sealed class BuildUWPTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.GetMSBuildWith("Microsoft.VisualStudio.Component.Windows10SDK.19041");
    }

    public override void Run(BuildContext context)
    {
        context.MSBuild(ProjectPaths.MonoGameFrameworkWindowsUniversal, context.MSPackSettings);
    }
}

