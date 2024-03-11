using Cake.Common.Tools.DotNet;
using Cake.Core;
using Cake.FileHelpers;
using Cake.Frosting;
using System.Text.RegularExpressions;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildTools")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
public sealed class BuildToolsTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetRestore(ProjectPaths.MonoGameContentBuilder, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameContentBuilder, context.DotNetBuildSettings);

        context.DotNetRestore(ProjectPaths.MonoGameEffectCompiler, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameEffectCompiler, context.DotNetBuildSettings);

        context.DotNetRestore(ProjectPaths.MonoGameContentBuilderTask, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameContentBuilderTask, context.DotNetBuildSettings);

        context.DotNetRestore(ProjectPaths.MonoGamePackagingFlatpak, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGamePackagingFlatpak, context.DotNetBuildSettings);

        string versionRegEx = @"<key>CFBundleShortVersionString<\/key>\s*<string>([^\s]*)<\/string>";
        string plistPath = "Tools/MonoGame.Content.Builder.Editor/Info.plist";
        string newVersion = $"<key>CFBundleShortVersionString</key>\n\t<string>{context.Version}</string>";
        context.ReplaceRegexInFiles(plistPath, versionRegEx, newVersion, RegexOptions.Singleline);

        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                context.DotNetRestore(ProjectPaths.MonoGameContentBuilderEditorWindows, context.DotNetRestoreSettings);
                context.DotNetPublish(ProjectPaths.MonoGameContentBuilderEditorWindows, context.DotNetPublishSettings);
                break;

            case PlatformFamily.OSX:
                context.DotNetRestore(ProjectPaths.MonoGameContentBuilderEditorMac, context.DotNetRestoreSettings);
                context.DotNetPublish(ProjectPaths.MonoGameContentBuilderEditorMac, context.DotNetPublishSettings);
                break;

            default:
                context.DotNetRestore(ProjectPaths.MonoGameContentBuilderEditorLinux, context.DotNetRestoreSettings);
                context.DotNetPublish(ProjectPaths.MonoGameContentBuilderEditorLinux, context.DotNetPublishSettings);
                break;
        }

        context.DotNetRestore(ProjectPaths.MonoGameContentBuilderEditorBootstrap, context.DotNetRestoreSettings);
        context.DotNetBuild(ProjectPaths.MonoGameContentBuilderEditorBootstrap, context.DotNetBuildSettings);
    }
}

