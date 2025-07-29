
namespace BuildScripts;

[TaskName("Build DX11 Shaders")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildShadersDX11Task : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();
    
    public override void Run(BuildContext context)
    {
        var mgfxc = context.GetProjectPath(ProjectType.Tools, "MonoGame.Effect.Compiler");
        var shadersDir = "MonoGame.Framework/Platform/Graphics/Effect/Resources";

        foreach (var filePath in context.GetFiles($"{shadersDir}/*.fx"))
        {
            context.Information($"Building {filePath.GetFilename()}");
            context.DotNetRun(mgfxc, $"{filePath} {filePath.GetFilenameWithoutExtension()}.dx11.mgfxo /Profile:DirectX_11", shadersDir);
            context.Information("");
        }
    }
}
