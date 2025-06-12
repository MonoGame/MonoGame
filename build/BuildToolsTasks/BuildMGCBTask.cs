using System.Text.RegularExpressions;

namespace BuildScripts;

[TaskName("Build mgcb")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
public sealed class BuildMGCBTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            context.ReplaceRegexInFiles(
                "Tools/MonoGame.Content.Builder.Task/MonoGame.Content.Builder.Task.props",
                @"<MonoGameVersion Condition=""'\$\(MonoGameVersion\)' == ''"">([^\s]*)<\/MonoGameVersion>",
                $"<MonoGameVersion Condition=\"'$(MonoGameVersion)' == ''\">{context.Version}</MonoGameVersion>",
                RegexOptions.Singleline
            );
        }
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder"), context.DotNetPackSettings);
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder.Task"), context.DotNetPackSettings);
    }
}
