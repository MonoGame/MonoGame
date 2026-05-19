
namespace BuildScripts;

[TaskName("TestDesktopGL")]
public sealed class TestDesktopGLTask : TestMonoGameTemplateTaskBase
{
    private static readonly PlatformFamily[] _supportedPlatforms = { PlatformFamily.Windows, PlatformFamily.Linux, PlatformFamily.OSX };
    
    protected override string TemplateName => "DesktopGL";
    protected override string ProjectFolderName => "desktopgl";
    protected override string TemplateShortName => "mgdesktopgl";
    protected override PlatformFamily[] SupportedPlatforms => _supportedPlatforms;
}
