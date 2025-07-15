
namespace BuildScripts;

[TaskName("RepackForDeploy")]
[IsDependentOn(typeof(DownloadArtifactsTask))]
public sealed class RepackForDeployTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        // repack any project that needs native libraries.
        context.DotNetPackSettings.MSBuildSettings.WithProperty("DisableNativeBuild", "True");

        //  MonoGame.Framework.Content.Pipeline
        context.DotNetPackSettings.MSBuildSettings.WithProperty("DisableMonoGameToolAssets", "True");
        context.DotNetPack(context.GetProjectPath(ProjectType.ContentPipeline), context.DotNetPackSettings);
        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableMonoGameToolAssets");

        // mgcb
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder"), context.DotNetPackSettings);

        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableNativeBuild");
        context.CopyDirectory(new DirectoryPath(context.NuGetsDirectory.FullPath), "nugets");
    }
}
