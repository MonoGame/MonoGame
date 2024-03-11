using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildiOS")]
[IsDependentOn(typeof(PrepTask))]
public sealed class BuildiOSTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        if (context.GetMSBuildWith("Component.Xamarin"))
        {
            return true;
        }

        return context.DirectoryExists("/Library/Frameworks/Xamarin.iOS.framework");
    }

    public override void Run(BuildContext context)
    {
        context.DotNetRestore(ProjectPaths.MonoGameFrameworkiOS, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameFrameworkiOS, context.DotNetBuildSettings);
    }
}

