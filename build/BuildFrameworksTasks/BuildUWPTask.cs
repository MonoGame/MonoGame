using Cake.Common.Tools.VSWhere;
using Cake.Common.Tools.VSWhere.Latest;

namespace BuildScripts;

[TaskName("Build UWP")]
public sealed class BuildUWPTask : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context)
    {
        if (!context.IsRunningOnWindows())
            return false;

        var vsLatest = context.VSWhereLatest(new VSWhereLatestSettings()
        {
            Requires = "Microsoft.VisualStudio.Component.Windows10SDK.19041" 
        });

        if (vsLatest == null)
            return false;

        return context.GetFiles(vsLatest.FullPath + "/**/MSBuild.exe").Count > 0;
    }

    public override void Run(BuildContext context)
        => context.MSBuild(context.GetProjectPath(ProjectType.Framework, "WindowsUniversal"), context.MSPackSettings);
}
