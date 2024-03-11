using Cake.Frosting;

namespace MonoGame.Framework.Build.Tasks.BuildTasks;

[TaskName("BuildAll")]
[IsDependentOn(typeof(BuildConsoleCheckTask))]
[IsDependentOn(typeof(BuildNativeTask))]
[IsDependentOn(typeof(BuildDesktopGLTask))]
[IsDependentOn(typeof(BuildWindowsDXTask))]
[IsDependentOn(typeof(BuildAndroidTask))]
[IsDependentOn(typeof(BuildiOSTask))]
[IsDependentOn(typeof(BuildUWPTask))]
[IsDependentOn(typeof(BuildContentPipelineTask))]
[IsDependentOn(typeof(BuildToolsTask))]
public sealed class BuildAllTask : FrostingTask<BuildContext> { }
