
namespace BuildScripts;

[TaskName("Build ReferenceAssemblies")]
[IsDependentOn(typeof(BuildContentPipelineTask))]
[IsDependentOn(typeof(BuildDesktopGLTask))]
public sealed class BuildReferenceAssembliesTasks : FrostingTask<BuildContext>
{
     public override bool ShouldRun(BuildContext context) => context.IsToolInstalled("jetbrains.refasmer.clitool");
    public override void Run(BuildContext context) {
        // generate a reference assembly for DesktopGL and Pipelines
        context.StartProcess (
            "dotnet",
            new ProcessSettings()
            {
                Arguments = $"refasmer -v -O artifacts/MonoGame.Framework.Ref -c artifacts/MonoGame.Framework/DesktopGL/{context.DotNetPackSettings.Configuration}/MonoGame.Framework.dll",
                RedirectStandardOutput = true
            },
            out IEnumerable<string> processOutput
        );
        context.StartProcess (
            "dotnet",
            new ProcessSettings()
            {
                Arguments = $"refasmer -v -O artifacts/MonoGame.Framework.Content.Pipeline.Ref -c artifacts/MonoGame.Framework.Content.Pipeline/{context.DotNetPackSettings.Configuration}/MonoGame.Framework.Content.Pipeline.dll",
                RedirectStandardOutput = true
            },
            out processOutput
        );
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Framework.Ref"), context.DotNetPackSettings);
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Framework.Content.Pipeline.Ref"), context.DotNetPackSettings);
    }
}