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
            context.DotNetPack(context.GetProjectPath(ProjectType.ContentPipeline), context.DotNetPackSettings);
            context.CopyDirectory(new DirectoryPath(context.NuGetsDirectory.FullPath), "nugets");
        }
    }
}
