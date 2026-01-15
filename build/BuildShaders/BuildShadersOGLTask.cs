
namespace BuildScripts;

[TaskName("Build OpenGL Shaders")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildShadersOGLTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        var mgfxc = context.GetProjectPath(ProjectType.Tools, "MonoGame.Effect.Compiler");
        var shadersDir = "MonoGame.Framework/Platform/Graphics/Effect/Resources";

        var envVars = System.Environment.GetEnvironmentVariables();
        context.Information($"Environment");

        foreach (System.Collections.DictionaryEntry entry in envVars)
        {
            context.Information($"{entry.Key}={entry.Value}");
        }

        foreach (var filePath in context.GetFiles($"{shadersDir}/*.fx"))
        {
            context.Information($"Building {filePath.GetFilename()}");
            context.DotNetRun(mgfxc, $"\"{filePath}\" {filePath.GetFilenameWithoutExtension()}.ogl.mgfxo", shadersDir);
            context.Information("");
        }
    }
}
