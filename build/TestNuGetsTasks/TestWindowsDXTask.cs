namespace BuildScripts;

[TaskName("TestWindowsDX")]
public sealed class TestWindowsDXTask : TestMonoGameTemplateTaskBase
{
    private static readonly PlatformFamily[] _supportedPlatforms = { PlatformFamily.Windows };
    
    protected override string TemplateName => "WindowsDX";
    protected override string ProjectFolderName => "windowsdx";
    protected override string TemplateShortName => "mgwindowsdx";
    protected override PlatformFamily[] SupportedPlatforms => _supportedPlatforms;
}
