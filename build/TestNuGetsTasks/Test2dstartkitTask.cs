namespace BuildScripts;

[TaskName("TestFull2DStarterKit")]
public sealed class TestFull2DStarterKitTask : TestMonoGameTemplateTaskBase
{
    private static readonly PlatformFamily[] _supportedPlatforms = { PlatformFamily.Windows, PlatformFamily.Linux, PlatformFamily.OSX };
    
    protected override string TemplateName => "Full 2D Starter Kit";
    protected override string ProjectFolderName => "mgtwodstartkit";
    protected override string TemplateShortName => "mg2dstartkit";
    protected override PlatformFamily[] SupportedPlatforms => _supportedPlatforms;
}
