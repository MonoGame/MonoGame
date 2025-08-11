
namespace BuildScripts;

[TaskName("Build OpenGL Shaders")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildShadersOGLTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var mgfxc = context.GetProjectPath(ProjectType.Tools, "MonoGame.Effect.Compiler");
        var shadersDir = "MonoGame.Framework/Platform/Graphics/Effect/Resources";

        foreach (var filePath in context.GetFiles($"{shadersDir}/*.fx"))
        {
            context.Information($"Building {filePath.GetFilename()}");
            context.DotNetRun(mgfxc, $"{filePath} {filePath.GetFilenameWithoutExtension()}.ogl.mgfxo", shadersDir);
            context.Information("");
        }
    }
}
