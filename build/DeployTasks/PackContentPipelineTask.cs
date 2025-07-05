using System;
using System.IO;
using Cake.Core.IO;
using FrostingContext = Cake.Frosting.FrostingContext;

namespace BuildScripts
{
    [TaskName("PackContentPipeline")]
    [IsDependentOn(typeof(DownloadArtifactsTask))]
    public sealed class PackContentPipelineTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            // Pack the MonoGame.Content.Pipeline project including native libs
            var builderPath = context.GetProjectPath(ProjectType.ContentPipeline);
            context.DotNetPackSettings.MSBuildSettings.WithProperty("DisableMonoGameToolAssets", "True");
            context.DotNetPack(builderPath, context.DotNetPackSettings);
            context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableMonoGameToolAssets");
            context.CopyDirectory(new DirectoryPath(context.NuGetsDirectory.FullPath), "nugets");
        }
    }
}
