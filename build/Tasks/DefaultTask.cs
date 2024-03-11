using Cake.Frosting;
using MonoGame.Framework.Build.Tasks.PackTasks;

namespace MonoGame.Framework.Build.Tasks;

[TaskName("Default")]
[IsDependentOn(typeof(PackAllTask))]
public sealed class DefaultTask : FrostingTask<BuildContext> { }
