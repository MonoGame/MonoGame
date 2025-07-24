namespace BuildScripts;

[TaskName("TestAndroid")]
public sealed class TestAndroidTask : TestMonoGameTemplateTaskBase
{
    protected override string TemplateName => "Android";
    protected override string ProjectFolderName => "android";
    protected override string TemplateShortName => "mgandroid";
    protected override PlatformFamily[] SupportedPlatforms => new[] { PlatformFamily.Windows, PlatformFamily.Linux, PlatformFamily.OSX };
}
