using System.Runtime.InteropServices;

namespace BuildScripts;

[TaskName("Build Content Pipeline")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildContentPipelineTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var builderPath = context.GetProjectPath(ProjectType.ContentPipeline);
        context.DotNetPackSettings.MSBuildSettings.WithProperty("DisableMonoGameToolAssets", "True");
        context.DotNetPack(builderPath, context.DotNetPackSettings);
        context.DotNetPackSettings.MSBuildSettings.Properties.Remove("DisableMonoGameToolAssets");

        switch (context.Environment.Platform.Family)
        {
            case PlatformFamily.Windows:
                // Both architectures are built on Windows, so lets check both.
                context.CheckLib("native/mgpipeline/windows/x64/Release/mgpipeline.dll");
                context.CheckLib("native/mgpipeline/windows/arm64/Release/mgpipeline.dll");
                break;
            case PlatformFamily.Linux:
                var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
                context.CheckLib($"native/mgpipeline/linux/{arch}/Release/libmgpipeline.so");
                break;
            case PlatformFamily.OSX:
                context.CheckLib("native/mgpipeline/macosx/Release/libmgpipeline.dylib");
                break;
            default:
                throw new NotSupportedException($"Platform {context.Environment.Platform.Family} is not supported for static library checks.");
        }
        context.PublishToolsBinaries(builderPath);
    }
}
