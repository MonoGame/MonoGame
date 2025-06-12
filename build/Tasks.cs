
namespace BuildScripts;

[TaskName("Build Frameworks")]
[IsDependentOn(typeof(BuildNativeTask))]
[IsDependentOn(typeof(BuildDesktopVKTask))]
[IsDependentOn(typeof(BuildDesktopGLTask))]
[IsDependentOn(typeof(BuildWindowsDXTask))]
[IsDependentOn(typeof(BuildAndroidTask))]
[IsDependentOn(typeof(BuildiOSTask))]
[IsDependentOn(typeof(BuildContentPipelineTask))]
[IsDependentOn(typeof(BuildConsoleCheckTask))]
public sealed class BuildFrameworksTask : FrostingTask<BuildContext> { }

[TaskName("Build Tools")]
[IsDependentOn(typeof(BuildMGFXCTask))]
[IsDependentOn(typeof(BuildContentPipelineTask))]
[IsDependentOn(typeof(BuildMGCBTask))]
[IsDependentOn(typeof(BuildMGCBEditorTask))]
public sealed class BuildToolsTask : FrostingTask<BuildContext> { }

[TaskName("Build Templates")]
[IsDependentOn(typeof(BuildDotNetTemplatesTask))]
[IsDependentOn(typeof(BuildVSTemplatesTask))]
public sealed class BuildTemplatesTask : FrostingTask<BuildContext> { }

[TaskName("Build All Tests")]
[IsDependentOn(typeof(BuildTestsTask))]
[IsDependentOn(typeof(BuildToolTestsTask))]
public sealed class BuildAllTestsTask : FrostingTask<BuildContext> { }


[TaskName("Build All")]
[IsDependentOn(typeof(BuildFrameworksTask))]
[IsDependentOn(typeof(BuildToolsTask))]
[IsDependentOn(typeof(BuildTemplatesTask))]
[IsDependentOn(typeof(BuildAllTestsTask))]
public sealed class BuildAllTask : FrostingTask<BuildContext> { }

[TaskName("Deploy")]
[IsDependentOn(typeof(DeployNuGetsToGitHubTask))]
[IsDependentOn(typeof(DeployNuGetsToNuGetOrgTask))]
[IsDependentOn(typeof(DeployVsixToMarketplaceTask))]
public sealed class DeployTask : FrostingTask<BuildContext> { }

[TaskName("Test")]
[IsDependentOn(typeof(DownloadTestArtifactsTask))]
public sealed class TestTask : FrostingTask<BuildContext> {}

[TaskName("Default")]
[IsDependentOn(typeof(BuildAllTask))]
public sealed class DefaultTask : FrostingTask<BuildContext> { }
