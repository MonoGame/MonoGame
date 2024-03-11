using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildAndroid")]
[IsDependentOn(typeof(PrepTask))]
public sealed class BuildAndroidTask : FrostingTask<BuildContext>
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
        context.DotNetRestore(ProjectPaths.MonoGameFrameworkAndroid, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameFrameworkAndroid, context.DotNetBuildSettings);
    }
}
