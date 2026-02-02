
namespace BuildScripts;

[TaskName("Build Analyzer")]
public sealed class BuildAnalyzerTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        context.DotNetPack(context.GetProjectPath(ProjectType.Analyzer), context.DotNetPackSettings);
        context.PublishAnalyzerBinaries("MonoGame.Analyzers");
    }
}
