using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using MonoGame.Framework.Build.Tasks.BuildTasks;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackiOS")]
[IsDependentOn(typeof(BuildiOSTask))]
public sealed class PackiOSTask : FrostingTask<BuildContext>
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
        context.DotNetPack(ProjectPaths.MonoGameFrameworkiOS, context.DotNetPackSettings);
    }
}
