using System.Text.RegularExpressions;

namespace BuildScripts;

[TaskName("Build MGCB Editor")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
public sealed class BuildMGCBEditorTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            context.ReplaceRegexInFiles(
                "Tools/MonoGame.Content.Builder.Editor/Info.plist",
                @"<key>CFBundleShortVersionString<\/key>\s*<string>([^\s]*)<\/string>",
                $"<key>CFBundleShortVersionString</key>\n\t<string>{context.Version}</string>",
                RegexOptions.Singleline
            );
        }

        var platform = context.Environment.Platform.Family switch
        {
            PlatformFamily.Windows => "Windows",
            PlatformFamily.OSX => "Mac",
            _ => "Linux"
        };

        if (context.Environment.Platform.Family != PlatformFamily.OSX)
            context.DotNetPublish(context.GetProjectPath(ProjectType.MGCBEditor, platform), context.DotNetPublishSettings);
        else
            context.DotNetPublish(context.GetProjectPath(ProjectType.MGCBEditor, platform), context.DotNetPublishSettingsForMac);
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder.Editor.Launcher.Bootstrap"), context.DotNetPackSettings);
		context.DotNetPack(context.GetProjectPath(ProjectType.MGCBEditorLauncher, platform), context.DotNetPackSettings);
    }
}
