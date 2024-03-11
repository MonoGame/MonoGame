using Cake.Common;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.MSBuild;
using Cake.FileHelpers;
using Cake.Frosting;
using System.Text.RegularExpressions;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackVSTemplates")]
[IsDependentOn(typeof(PackDotNetTemplatesTask))]
public sealed class PackVSTemplatesTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        return context.IsRunningOnWindows();
    }

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

        context.DotNetRestore(ProjectPaths.MonoGameTemplatesVSExtension);
        context.MSBuild(ProjectPaths.MonoGameTemplatesVSExtension, context.MSBuildSettings);
    }
}
