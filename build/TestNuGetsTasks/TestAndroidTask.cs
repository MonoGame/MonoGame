namespace BuildScripts;

[TaskName("TestAndroid")]
public sealed class TestAndroidTask : TestMonoGameTemplateTaskBase
{
    private static readonly PlatformFamily[] _supportedPlatforms = { PlatformFamily.Windows, PlatformFamily.Linux, PlatformFamily.OSX };
    
    protected override string TemplateName => "Android";
    protected override string ProjectFolderName => "android";
    protected override string TemplateShortName => "mgandroid";
    protected override PlatformFamily[] SupportedPlatforms => _supportedPlatforms;
}
