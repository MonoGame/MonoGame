
namespace BuildScripts;

[TaskName("Build Native")]
[IsDependentOn(typeof(BuildMGFXCTask))]
public sealed class BuildNativeTask : FrostingTask<BuildContext>
{
    private string platformName = "Native";
    public override void Run(BuildContext context)
    {
        context.DotNetPack(context.GetProjectPath(ProjectType.Framework, platformName), context.DotNetPackSettings);

        context.DotNetPack("src/NuGetPackages/MonoGame.Framework/MonoGame.Framework.csproj", context.DotNetPackSettings);

        if (context.Environment.Platform.Family == PlatformFamily.Windows)
        {
            context.DotNetPack("src/NuGetPackages/MonoGame.Runtime.Windows.DX12/MonoGame.Runtime.Windows.DX12.csproj", context.DotNetPackSettings);
            context.DotNetPack("src/NuGetPackages/MonoGame.Runtime.Windows.Vulkan/MonoGame.Runtime.Windows.Vulkan.csproj", context.DotNetPackSettings);
        }
        else if (context.Environment.Platform.Family == PlatformFamily.OSX)
        {
            context.DotNetPack("src/NuGetPackages/MonoGame.Runtime.Mac.Vulkan/MonoGame.Runtime.Mac.Vulkan.csproj", context.DotNetPackSettings);
        }
        else
        {
            context.DotNetPack("src/NuGetPackages/MonoGame.Runtime.Linux.Vulkan/MonoGame.Runtime.Linux.Vulkan.csproj", context.DotNetPackSettings);
        }

        context.PublishBinaries(platformName);
    }
}
