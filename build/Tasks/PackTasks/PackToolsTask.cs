using Cake.Common.Tools.DotNet;
using Cake.Core;
using Cake.Frosting;
using MonoGame.Framework.Build.Tasks.BuildTasks;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackTools")]
[IsDependentOn(typeof(BuildToolsTask))]
public sealed class PackToolsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPack(ProjectPaths.MonoGameContentBuilder, context.DotNetPackSettings);
        context.DotNetPack(ProjectPaths.MonoGameEffectCompiler, context.DotNetPackSettings);
        context.DotNetPack(ProjectPaths.MonoGameContentBuilderTask, context.DotNetPackSettings);
        context.DotNetPack(ProjectPaths.MonoGamePackagingFlatpak, context.DotNetPackSettings);

        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                context.DotNetPack(ProjectPaths.MonoGameContentBuilderEditorLauncherWindows, context.DotNetPackSettings);
                break;

            case PlatformFamily.OSX:
                context.DotNetPack(ProjectPaths.MonoGameContentBuilderEditorLauncherMac, context.DotNetPackSettings);
                break;

            default:
                context.DotNetPack(ProjectPaths.MonoGameContentBuilderEditorLauncherLinux, context.DotNetPackSettings);
                break;
        }

        context.DotNetPack(ProjectPaths.MonoGameContentBuilderEditorBootstrap, context.DotNetPackSettings);
    }
}
