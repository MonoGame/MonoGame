using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;
using MonoGame.Framework.Build.Tasks.BuildTasks;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackAndroid")]
[IsDependentOn(typeof(BuildAndroidTask))]
public sealed class PackAndroidTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        if (context.GetMSBuildWith("Component.Xamarin"))
        {
            return true;
        }

        return context.DirectoryExists("/Library/Frameworks/Xamarin.Android.framework");
    }

    public override void Run(BuildContext context)
    {
        context.DotNetPack(ProjectPaths.MonoGameFrameworkAndroid, context.DotNetPackSettings);
    }
}
