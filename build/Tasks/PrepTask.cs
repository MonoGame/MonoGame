using Cake.Common;
using Cake.Common.IO;
using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks;

[TaskName("Prep")]
public sealed class PrepTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (!context.IsRunningOnWindows())
        {
            //  SET MGFXC_WINE_PATH for building shaders on macOS and Linux
            System.Environment.SetEnvironmentVariable("MGFXC_WINE_PATH", context.EnvironmentVariable("HOME") + "/.winemonogame");
        }

        context.CleanDirectory(context.ArtifactsDirectory);
        context.CreateDirectory(context.ArtifactsDirectory);
    }
}
