namespace BuildScripts;

[TaskName("Build Browser WebGL2")]
[IsDependentOn(typeof(BuildNativeTask))]
public sealed class BuildBrowserWebGL2Task : FrostingTask<BuildContext>
{
    private const string BrowserWasmDirectory = "native/monogame/browser-wasm";
    private const string BrowserWasmReleasePreset = "browser-wasm-release";

    public override void Run(BuildContext context)
    {
        ConfigureNativeRuntime(context);
        BuildNativeRuntime(context);
        context.DotNetPack("src/NuGetPackages/MonoGame.Runtime.Browser.WebGL2/MonoGame.Runtime.Browser.WebGL2.csproj", context.DotNetPackSettings);
    }

    private static void ConfigureNativeRuntime(BuildContext context)
    {
        RunCMake(context, new ProcessArgumentBuilder()
            .Append("--preset")
            .Append(BrowserWasmReleasePreset), "Browser WebGL2 native runtime configuration failed.");
    }

    private static void BuildNativeRuntime(BuildContext context)
    {
        RunCMake(context, new ProcessArgumentBuilder()
            .Append("--build")
            .Append("--preset")
            .Append(BrowserWasmReleasePreset), "Browser WebGL2 native runtime build failed.");
    }

    private static void RunCMake(BuildContext context, ProcessArgumentBuilder arguments, string errorMessage)
    {
        ProcessSettings settings = new()
        {
            Arguments = arguments,
            WorkingDirectory = context.MakeAbsolute(new DirectoryPath(BrowserWasmDirectory))
        };

        if (context.StartProcess("cmake", settings) != 0)
        {
            throw new InvalidOperationException(errorMessage);
        }
    }
}
