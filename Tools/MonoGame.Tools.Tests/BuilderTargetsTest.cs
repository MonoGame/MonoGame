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
        string[] msBuildFolders = new string[]
        {
            Path.Combine ("MSBuild", "15.0", "Bin", "MSBuild.exe"),
            Path.Combine ("MSBuild", "14.0", "Bin", "MSBuild.exe"),
        };
        string FindBuildTool(string buildTool)
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && buildTool == "msbuild")
            {
                var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                foreach (var path in msBuildFolders)
                {
                    if (File.Exists(Path.Combine(programFiles, path)))
                        return Path.Combine(programFiles, path);
                }
            }
            return buildTool;
        }
        bool RunBuild(string buildTool, string projectFile, params string[] parameters)
        {
            var root = Path.GetDirectoryName(typeof(BuilderTargetsTest).Assembly.Location);
            var psi = new ProcessStartInfo(FindBuildTool(buildTool))
            {
                Arguments = projectFile + " /t:BuildContent " + string.Join(" ", parameters) + " /noconsolelogger \"/flp1:LogFile=build.log;Encoding=UTF-8;Verbosity=Diagnostic\"",
                WorkingDirectory = root,
                UseShellExecute = true,
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
#if DESKTOPGL
        [Ignore("Fails on Mac build server with xbuild for some reason.")]
#else
        [Ignore("This test need to be rewritten to properly test Tools\\MonoGame.Content.Builder instead of the old builder targets.")]
#endif
        public void BuildSimpleProject(string buildTool)
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
