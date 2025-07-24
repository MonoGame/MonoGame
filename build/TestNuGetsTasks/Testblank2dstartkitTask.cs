namespace BuildScripts;

[TaskName("TestBlank2DStarterKit")]
public sealed class TestBlank2DStarterKitTask : TestMonoGameTemplateTaskBase
{
    protected override string TemplateName => "Blank 2D Starter Kit";
    protected override string ProjectFolderName => "blank2dstartkit";
    protected override string TemplateShortName => "mgblank2dstartkit";
    protected override PlatformFamily[] SupportedPlatforms => new[] { PlatformFamily.Windows, PlatformFamily.Linux, PlatformFamily.OSX };
}
