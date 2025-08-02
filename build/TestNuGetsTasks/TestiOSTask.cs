namespace BuildScripts;

[TaskName("TestiOS")]
public sealed class TestiOSTask : TestMonoGameTemplateTaskBase
{
    private static readonly PlatformFamily[] _supportedPlatforms = { PlatformFamily.OSX };
    
    protected override string TemplateName => "iOS";
    protected override string ProjectFolderName => "ios";
    protected override string TemplateShortName => "mgios";
    protected override PlatformFamily[] SupportedPlatforms => _supportedPlatforms;
}
