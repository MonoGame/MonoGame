
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
        context.PublishToolsBinaries(context.GetProjectPath(ProjectType.ContentPipeline));
        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableMonoGameToolAssets");

        // mgcb
        context.DotNetPack(context.GetProjectPath(ProjectType.Tools, "MonoGame.Content.Builder"), context.DotNetPackSettings);

        // Repack mgcb-editor-linux with all existing architectures.
        // This ensures libmgpipeline.so is shipped for both linux-x64 and linux-arm64.
        context.DotNetPublish(context.GetProjectPath(ProjectType.MGCBEditor, "Linux"), context.DotNetPublishSettings);
        context.DotNetPack(context.GetProjectPath(ProjectType.MGCBEditorLauncher, "Linux"), context.DotNetPackSettings);

        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableNativeBuild");
    }
}
