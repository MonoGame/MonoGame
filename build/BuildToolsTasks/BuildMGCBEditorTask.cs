using System.Text.RegularExpressions;

namespace BuildScripts;

[TaskName("Build MGCB Editor")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
public sealed class BuildMGCBEditorTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.ReplaceRegexInFiles(
            @"<key>CFBundleShortVersionString<\/key>\s*<string>([^\s]*)<\/string>",
            "Tools/MonoGame.Content.Builder.Editor/Info.plist",
            $"<key>CFBundleShortVersionString</key>\n\t<string>{context.Version}</string>",
            RegexOptions.Singleline
        );

        var platform = context.Environment.Platform.Family switch
        {
            PlatformFamily.Windows => "Windows",
            PlatformFamily.OSX => "Mac",
            _ => "Linux"
        };

        context.DotNetPublish(context.GetProjectPath(ProjectType.MGCBEditor, platform), context.DotNetPublishSettings);
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder.Editor.Launcher.Bootstrap"), context.DotNetPackSettings);
		context.DotNetPack(context.GetProjectPath(ProjectType.MGCBEditorLauncher, platform), context.DotNetPackSettings);
    }
}
