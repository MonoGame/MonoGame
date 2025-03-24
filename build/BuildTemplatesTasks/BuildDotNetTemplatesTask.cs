
namespace BuildScripts;

[TaskName("Build dotnet templates")]
public sealed class BuildDotNetTemplatesTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
        => context.DotNetPack(context.GetProjectPath(ProjectType.Templates, "MonoGame.Templates.CSharp"), context.DotNetPackSettings);
}
