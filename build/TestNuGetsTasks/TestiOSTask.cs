namespace BuildScripts;

[TaskName("TestiOS")]
public sealed class TestiOSTask : TestMonoGameTemplateTaskBase
{
    protected override string TemplateName => "iOS";
    protected override string ProjectFolderName => "ios";
    protected override string TemplateShortName => "mgios";
    protected override PlatformFamily[] SupportedPlatforms => new[] { PlatformFamily.OSX };
}
