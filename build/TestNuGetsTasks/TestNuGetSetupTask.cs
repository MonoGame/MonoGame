namespace BuildScripts;

[TaskName("TestNuGetSetup")]
public sealed class TestNuGetSetupTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        TestMonoGameTemplateTaskBase.ClearTestResults();
        context.Information("🔄 Initialized MonoGame NuGet template testing...");
    }
}
