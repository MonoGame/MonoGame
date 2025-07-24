
using System.Text.RegularExpressions;

namespace BuildScripts;

[TaskName("TestDesktopGL")]
public sealed class TestDesktopGLTask : TestMonoGameTemplateTaskBase
{
    protected override string TemplateName => "DesktopGL";
    protected override string ProjectFolderName => "desktopgl";
    protected override string TemplateShortName => "mgdesktopgl";
    protected override PlatformFamily[] SupportedPlatforms => new[] { PlatformFamily.Windows, PlatformFamily.Linux, PlatformFamily.OSX };
}
