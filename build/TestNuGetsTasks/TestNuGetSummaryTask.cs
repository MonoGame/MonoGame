namespace BuildScripts;

[TaskName("TestNuGetSummary")]
public sealed class TestNuGetSummaryTask : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        TestMonoGameTemplateTaskBase.DisplayTestSummary(context);
    }
}
