using MonoGame.Tool;

namespace BuildScripts;

[TaskName("Build DX12 Shaders")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildShadersDX12Task : FrostingTask<BuildContext>
{
    public override bool ShouldRun(BuildContext context) => context.IsRunningOnWindows();

    public override void Run(BuildContext context)
    {
        var mgfxc = context.GetProjectPath(ProjectType.Tools, "MonoGame.Effect.Compiler");
        var shadersDir = "MonoGame.Framework/Platform/Graphics/Effect/Resources";
        var workingDir = "native/monogame/directx12/";

        foreach (var filePath in context.GetFiles($"{shadersDir}/*.fx"))
        {
            context.Information($"Building {filePath.GetFilename()}");
            context.DotNetRun(mgfxc, $"{filePath} {filePath.GetFilenameWithoutExtension()}.dx12.mgfxo.h /Profile:DirectX_12", workingDir);
            context.Information("");
        }

        if (context.DxcRun("-T cs_6_0 -O3 -Vn GenerateMips_main -Fh native/monogame/directx12/GenerateMips_Desktop.h native/monogame/directx12/GenerateMips.hlsl") != 0)
        {
            throw new Exception("An error occured while running dxc");
        }
    }
}
