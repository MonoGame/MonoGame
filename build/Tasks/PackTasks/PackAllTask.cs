using Cake.Frosting;
using MonoGame.Framework.Build.Tasks.BuildTasks;

namespace MonoGame.Framework.Build.Tasks.PackTasks;

[TaskName("PackAll")]
[IsDependentOn(typeof(BuildAllTask))]
[IsDependentOn(typeof(PackNativeTask))]
[IsDependentOn(typeof(PackDesktopGLTask))]
[IsDependentOn(typeof(PackWindowsDXTask))]
[IsDependentOn(typeof(PackAndroidTask))]
[IsDependentOn(typeof(PackiOSTask))]
[IsDependentOn(typeof(PackContentPipelineTask))]
[IsDependentOn(typeof(PackToolsTask))]
[IsDependentOn(typeof(PackDotNetTemplatesTask))]
[IsDependentOn(typeof(PackVSTemplatesTask))]
public sealed class PackAllTask : FrostingTask<BuildContext> { }
