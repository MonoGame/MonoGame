
namespace BuildScripts;

[TaskName("Build Vulklan Shaders")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildShadersVulkanTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var mgfxc = context.GetProjectPath(ProjectType.Tools, "MonoGame.Effect.Compiler");
        var shadersDir = "MonoGame.Framework/Platform/Graphics/Effect/Resources";
        var workingDir = "native/monogame/vulkan/";

        foreach (var filePath in context.GetFiles($"{shadersDir}/*.fx"))
            {
                context.Information($"Building {filePath.GetFilename()}");
                context.DotNetRun(mgfxc, $"{filePath} {filePath.GetFilenameWithoutExtension()}.vk.mgfxo.h /Profile:Vulkan", workingDir);
                context.Information("");
            }
    }
}
