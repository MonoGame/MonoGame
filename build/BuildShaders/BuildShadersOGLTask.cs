using System.Runtime.InteropServices;

namespace BuildScripts;

[TaskName("Build OpenGL Shaders")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildShadersOGLTask : FrostingTask<BuildContext>
{
    // Linux Arm64 does not support the version of wine we need atm
     public override bool ShouldRun(BuildContext context) => !(context.IsRunningOnLinux() && RuntimeInformation.ProcessArchitecture == Architecture.Arm64);
     
    public override void Run(BuildContext context)
    {
        var mgfxc = context.GetProjectPath(ProjectType.Tools, "MonoGame.Effect.Compiler");
        var shadersDir = "MonoGame.Framework/Platform/Graphics/Effect/Resources";

        foreach (var filePath in context.GetFiles($"{shadersDir}/*.fx"))
        {
            context.Information($"Building {filePath.GetFilename()}");
            context.DotNetRun(mgfxc, $"\"{filePath}\" {filePath.GetFilenameWithoutExtension()}.ogl.mgfxo", shadersDir);
            context.Information("");
        }
    }
}
