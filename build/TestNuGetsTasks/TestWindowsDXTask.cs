namespace BuildScripts;

[TaskName("TestWindowsDX")]
public sealed class TestWindowsDXTask : TestMonoGameTemplateTaskBase
{
    protected override string TemplateName => "WindowsDX";
    protected override string ProjectFolderName => "windowsdx";
    protected override string TemplateShortName => "mgwindowsdx";
    protected override PlatformFamily[] SupportedPlatforms => new[] { PlatformFamily.Windows };
}
