
namespace BuildScripts;

[TaskName("Build Frameworks")]
[IsDependentOn(typeof(BuildConsoleCheckTask))]
[IsDependentOn(typeof(BuildNativeTask))]
[IsDependentOn(typeof(BuildDesktopGLTask))]
[IsDependentOn(typeof(BuildWindowsDXTask))]
[IsDependentOn(typeof(BuildAndroidTask))]
[IsDependentOn(typeof(BuildiOSTask))]
[IsDependentOn(typeof(BuildContentPipelineTask))]
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

[TaskName("Build All")]
[IsDependentOn(typeof(BuildFrameworksTask))]
[IsDependentOn(typeof(BuildToolsTask))]
[IsDependentOn(typeof(BuildTemplatesTask))]
public sealed class BuildAllTask : FrostingTask<BuildContext> { }

[TaskName("Deploy")]
[IsDependentOn(typeof(DeployNuGetsToGitHubTask))]
[IsDependentOn(typeof(DeployNuGetsToNuGetOrgTask))]
public sealed class DeployTask : FrostingTask<BuildContext> { }

[TaskName("Default")]
[IsDependentOn(typeof(BuildAllTask))]
public sealed class DefaultTask : FrostingTask<BuildContext> { }
