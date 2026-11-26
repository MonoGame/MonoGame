
namespace BuildScripts;

[TaskName("Build dotnet templates")]
public sealed class BuildDotNetTemplatesTask : FrostingTask<BuildContext>
{
    private const string TemplatesSourceDirectory = "external/MonoGame.Templates/CSharp";
    private const string TemplatesProjectName = "MonoGame.Templates.CSharp.csproj";

    public override void Run(BuildContext context)
    {
        string stagingDirectory = context.GetOutputPath("TemplatePackageSource");
        if (context.DirectoryExists(stagingDirectory))
        {
            context.DeleteDirectory(stagingDirectory);
        }

        context.CopyDirectory(TemplatesSourceDirectory, stagingDirectory);
        context.DotNetPack(System.IO.Path.Combine(stagingDirectory, TemplatesProjectName), context.DotNetPackSettings);
    }
}
