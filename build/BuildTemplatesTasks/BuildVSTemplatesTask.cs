using System.Text.RegularExpressions;

namespace BuildScripts;

[TaskName("Build Visual Studio templates")]
[IsDependentOn(typeof(BuildDotNetTemplatesTask))]
public sealed class BuildVSTemplatesTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        string shortVersion = context.Version.Split('-')[0];

        string versionRegEx = "<Identity Version=\"([^\"]*)\"";
        string filePath = "Templates/MonoGame.Templates.VSExtension/source.extension.vsixmanifest";
        string newVersion = $"<Identity Version=\"{shortVersion}\"";
        context.ReplaceRegexInFiles(filePath, versionRegEx, newVersion, RegexOptions.Singleline);

        versionRegEx = "[0-9](\\.[0-9])*";
        filePath = "Templates/MonoGame.Templates.VSExtension/Templates.pkgdef";
        context.ReplaceRegexInFiles(filePath, versionRegEx, shortVersion, RegexOptions.Singleline);

        var vsTemplatesProject = context.GetProjectPath(ProjectType.Extension, "MonoGame.Templates.VSExtension");
        context.DotNetRestore(vsTemplatesProject);
        context.MSBuild(vsTemplatesProject, context.MSBuildSettings);
    }
}
