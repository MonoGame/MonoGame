namespace BuildScripts;

[TaskName("TestWeb")]
public sealed class TestWebTask : TestMonoGameTemplateTaskBase
{
    private static readonly PlatformFamily[] SupportedPlatformFamilies = { PlatformFamily.Windows, PlatformFamily.Linux, PlatformFamily.OSX };

    protected override string TemplateName => "WebGL2";
    protected override string ProjectFolderName => "webgl2";
    protected override string TemplateShortName => "mgweb";
    protected override PlatformFamily[] SupportedPlatforms => SupportedPlatformFamilies;
}
