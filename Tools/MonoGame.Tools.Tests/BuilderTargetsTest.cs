using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using NUnit.Framework;
namespace MonoGame.Tests.ContentPipeline
{
    [TestFixture]
    public class BuilderTargetsTest
    {
        string FindTool (string toolName)
        {
            var dotnetRoot = Environment.GetEnvironmentVariable ("DOTNET_ROOT");
            TestContext.WriteLine ("DOTNET_ROOT=" + dotnetRoot);
            if (!string.IsNullOrEmpty (dotnetRoot))
            {
                var dotNetExe = Path.Combine (dotnetRoot, OperatingSystem.IsWindows() ? "dotnet.exe" : "dotnet");
                TestContext.WriteLine ("DOTNET_EXE=" + dotNetExe);
                if (File.Exists (dotNetExe))
                {
                    TestContext.WriteLine ("returning:" + dotNetExe);
                    return dotNetExe;
                }
            }
            TestContext.WriteLine ("returning:" + toolName);
            return toolName;
        }
        bool RunBuild(string buildTool, string projectFile, params string[] parameters)
        {
            var root = Path.GetDirectoryName(typeof(BuilderTargetsTest).Assembly.Location);
            var tool = FindTool(buildTool);
            var psi = new ProcessStartInfo(tool)
            {
                Arguments = $"build \"{projectFile}\" -t:IncludeContent {string.Join(" ", parameters)} -tl:off -bl -p:DotnetCommand=\"{tool}\"",
                WorkingDirectory = root,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            TestContext.WriteLine (psi.FileName + " " + psi.Arguments);
            using (var process = Process.Start(psi))
            {
                process.OutputDataReceived += (sender, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    TestContext.WriteLine($"Output: {e.Data}");
                }
                };

                process.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        TestContext.WriteLine($"Error: {e.Data}");
                    }
                };
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                if (!process.WaitForExit(60000)) {// wait for 60 seconds
                    process.Kill();
                    process.WaitForExit();
                }
                return process.ExitCode == 0;
            }
        }

        [Test]
        public void BuildSimpleProject()
        {
            var root = Path.GetDirectoryName(typeof(BuilderTargetsTest).Assembly.Location);
            var outputPath = Path.Combine(root, "Assets", "Projects", "Content", "bin");
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, recursive: true);

            var result = RunBuild("dotnet", Path.Combine(root, "Assets", "Projects", "BuildSimpleProject.csproj"), new string[] {
                $"-p:MGCBCommand=\"{Path.Combine(root, "mgcb.dll")}\""
            });
            Assert.AreEqual(true, result, "Content Build should have succeeded.");
            var contentFont = Path.Combine(outputPath, "DesktopGL", "Content", "ContentFont.xnb");
            Assert.IsTrue(File.Exists(contentFont), $"'{contentFont}' should exist.");
        }
    }
}
