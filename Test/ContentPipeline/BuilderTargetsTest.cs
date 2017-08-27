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
        bool RunBuild (string buildTool, string projectFile, params string[] parameters)
        {
            var root = Path.GetDirectoryName(typeof(BuilderTargetsTest).Assembly.Location);
            var psi = new ProcessStartInfo(buildTool)
            {
                Arguments = projectFile + " /t:BuildContent " + string.Join(" ", parameters),
                WorkingDirectory = root,
            };
            using (var process = Process.Start(psi))
            {
                process.WaitForExit();
                return process.ExitCode == 0;
            }
        }

        static object[] BuilderTargetsBuildTools = new object[] {
            "msbuild",
            "xbuild",
        };

        [Test]
        [TestCaseSource("BuilderTargetsBuildTools")]
        public void BuildSimpleProject (string buildTool)
        {
            if (buildTool == "xbuild" && Environment.OSVersion.Platform == PlatformID.Win32NT)
                Assert.Ignore("Skipping xbuild tests on windows");

            var root = Path.GetDirectoryName(typeof(BuilderTargetsTest).Assembly.Location);
            var outputPath = Path.Combine(root, "Assets", "Projects", "Content", "bin");
            if (Directory.Exists(outputPath))
                Directory.Delete(outputPath, recursive: true);

            var result = RunBuild(buildTool, Path.Combine("Assets", "Projects", "BuildSimpleProject.csproj"), new string[] {
                "/p:MonoGameContentBuilderExe=" + Path.Combine(root, "MGCB.exe")
            });
            Assert.AreEqual(true, result, "Content Build should have succeeded.");
            var contentFont = Path.Combine(outputPath, "DesktopGL", "Content", "ContentFont.xnb");
            Assert.IsTrue(File.Exists(contentFont), "'" + contentFont + "' should exist.");
 
        }
    }
}
