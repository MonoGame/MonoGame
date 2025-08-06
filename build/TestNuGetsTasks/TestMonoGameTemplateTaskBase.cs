using System.Text.RegularExpressions;
using System.IO;
using IOPath = System.IO.Path;

namespace BuildScripts;

public abstract class TestMonoGameTemplateTaskBase : FrostingTask<BuildContext>
{
    // Pre-compiled regex pattern with named groups for better performance
    private static readonly Regex PackageReferenceRegex = new(
        @"(?<partA><PackageReference\s+?Include=""(?<packageName>MonoGame\.[^""]+?)""\s+?Version="")(?<version>[^""]+?)(?<partB>"".*?>)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );
    // Static collection to track test results across all tasks
    private static readonly List<TestResult> TestResults = new();

    protected abstract string TemplateName { get; }
    protected abstract string ProjectFolderName { get; }
    protected abstract string TemplateShortName { get; }
    protected abstract PlatformFamily[] SupportedPlatforms { get; }

    private class TestResult
    {
        public required string TemplateName { get; set; }
        public TestStatus Status { get; set; }
        public required string Message { get; set; }
        public PlatformFamily Platform { get; set; }
    }

    private enum TestStatus
    {
        Success,
        Skipped,
        Failed
    }

    public override bool ShouldRun(BuildContext context)
    {
        var currentPlatform = context.Environment.Platform.Family;
        var isSupported = SupportedPlatforms.Contains(currentPlatform);

        if (!isSupported)
        {
            context.Information($"⏭️ Skipping {TemplateName} test - not supported on {currentPlatform}");
            context.Information($"   Supported platforms: {string.Join(", ", SupportedPlatforms)}");

            // Record the skip result
            TestResults.Add(new TestResult
            {
                TemplateName = TemplateName,
                Status = TestStatus.Skipped,
                Message = $"Not supported on {currentPlatform}",
                Platform = currentPlatform
            });
        }

        return isSupported;
    }

    public override void Run(BuildContext context)
    {
        var currentPlatform = context.Environment.Platform.Family;
        var nugetSourcePath = IOPath.GetFullPath(context.NuGetsDirectory);
        var testsPath = context.GetOutputPath("TemplateTests");
        var projectPath = IOPath.Combine(testsPath, ProjectFolderName);
        var nugetSourceName = "MonoGameTestSource";

        try
        {
            CleanupPreviousTestRun(context, testsPath, projectPath, nugetSourceName);

            SetupNuGetSource(context, nugetSourcePath, nugetSourceName);

            UninstallExistingTemplates(context);

            var templateVersion = GetTemplateVersionFromNuGet(context, nugetSourcePath);
            context.Information($"Detected MonoGame template version: {templateVersion}");

            InstallTemplates(context, templateVersion, nugetSourcePath);

            context.Information($"Creating template tests folder: {testsPath}");
            context.CreateDirectory(testsPath);

            CreateTestProject(context, projectPath);

            var projectDir = IOPath.Combine(projectPath, "TestProject");
            ReplaceDotnetToolsConfig(context, projectDir, templateVersion);

            UpdateProjectReferences(context, projectDir, templateVersion);

            RestoreProject(context, projectDir);

            LogCurrentFileContents(context, projectDir);

            BuildProject(context, projectDir);

            context.Information($"✅ Test completed successfully! MonoGame {TemplateName} project built without errors.");
            context.Information($"📁 Test project preserved at: {projectDir}");

            TestResults.Add(new TestResult
            {
                TemplateName = TemplateName,
                Status = TestStatus.Success,
                Message = "Built successfully",
                Platform = currentPlatform
            });
        }
        catch (Exception ex)
        {
            TestResults.Add(new TestResult
            {
                TemplateName = TemplateName,
                Status = TestStatus.Failed,
                Message = ex.Message,
                Platform = currentPlatform
            });

            throw;
        }
        finally
        {
            CleanupNuGetSource(context, nugetSourceName);
        }
    }

    private void SetupNuGetSource(BuildContext context, string nugetSourcePath, string nugetSourceName)
    {
        context.Information($"Setting up NuGet source from: {nugetSourcePath}");

        try
        {
            context.StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"nuget remove source {nugetSourceName}"
            });
        }
        catch
        {
            // Source might not exist, continue
        }

        try
        {
            var addSourceSettings = new ProcessSettings
            {
                Arguments = new ProcessArgumentBuilder()
                    .Append("nuget")
                    .Append("add")
                    .Append("source")
                    .Append(nugetSourcePath)
                    .Append("--name")
                    .Append(nugetSourceName),
                RedirectStandardOutput = true
            };

            var result = context.StartProcess("dotnet", addSourceSettings, out var output);

            if (result != 0)
            {
                var errorMessage = string.Join("\n", output);
                context.Warning($"dotnet nuget add source failed with exit code {result}");
                context.Warning($"Output: {errorMessage}");
                throw new Exception($"Failed to add NuGet source {nugetSourceName}");
            }

            context.Information($"Successfully added NuGet source: {nugetSourceName}");
        }
        catch (Exception ex)
        {
            context.Error($"Failed to add NuGet source: {ex.Message}");
            throw;
        }
    }

    private void UninstallExistingTemplates(BuildContext context)
    {
        context.Information("Removing existing MonoGame templates...");
        try
        {
            context.StartProcess("dotnet", new ProcessSettings
            {
                Arguments = "new uninstall MonoGame.Templates.CSharp"
            });
        }
        catch
        {
            context.Information("No existing MonoGame templates found to uninstall.");
        }
    }

    private void InstallTemplates(BuildContext context, string templateVersion, string nugetSourcePath)
    {
        context.Information($"Installing MonoGame templates version {templateVersion}...");
        context.StartProcess("dotnet", new ProcessSettings
        {
            Arguments = $"new install MonoGame.Templates.CSharp::{templateVersion} --nuget-source \"{nugetSourcePath}\""
        });
    }

    private void CreateTestProject(BuildContext context, string projectPath)
    {
        context.Information($"Creating new MonoGame {TemplateName} project in: {projectPath}");
        context.CreateDirectory(projectPath);
        context.StartProcess("dotnet", new ProcessSettings
        {
            Arguments = $"new {TemplateShortName} -n TestProject",
            WorkingDirectory = projectPath
        });
    }

    private void ReplaceDotnetToolsConfig(BuildContext context, string projectDir, string version)
    {
        var configDir = IOPath.Combine(projectDir, ".config");
        var dotnetToolsPath = IOPath.Combine(configDir, "dotnet-tools.json");

        context.Information($"Replacing dotnet-tools.json with platform-specific version for: {context.Environment.Platform.Family}");

        if (!Directory.Exists(configDir))
        {
            context.CreateDirectory(configDir);
        }

        var toolsJson = GetPlatformSpecificToolsJson(context, version);
        File.WriteAllText(dotnetToolsPath, toolsJson);

        context.Information("Platform-specific dotnet-tools.json created successfully.");
    }

    private string GetPlatformSpecificToolsJson(BuildContext context, string version)
    {
        var platform = context.Environment.Platform.Family;

        // Base tools that are available on all platforms
        var baseTools = $$"""
            {
            "version": 1,
            "isRoot": true,
            "tools": {
                "dotnet-mgcb": {
                "version": "{{version}}",
                "commands": [
                    "mgcb"
                ]
                },
                "dotnet-mgcb-editor": {
                "version": "{{version}}",
                "commands": [
                    "mgcb-editor"
                ]
                }
            """;

        // Add platform-specific editor tool
        string platformSpecificTool = platform switch
        {
            PlatformFamily.Windows => $$"""
                ,
                "dotnet-mgcb-editor-windows": {
                "version": "{{version}}",
                "commands": [
                    "mgcb-editor-windows"
                ]
                }
            """,
            PlatformFamily.Linux => $$"""
                ,
                "dotnet-mgcb-editor-linux": {
                "version": "{{version}}",
                "commands": [
                    "mgcb-editor-linux"
                ]
                }
            """,
            PlatformFamily.OSX => $$"""
                ,
                "dotnet-mgcb-editor-mac": {
                "version": "{{version}}",
                "commands": [
                    "mgcb-editor-mac"
                ]
                }
            """,
            _ => "" // No platform-specific tool for unknown platforms
        };

        return baseTools + platformSpecificTool + "\n  }\n}";
    }

    private void RestoreProject(BuildContext context, string projectDir)
    {
        context.Information("Restoring project dependencies...");
        context.StartProcess("dotnet", new ProcessSettings
        {
            Arguments = "restore",
            WorkingDirectory = projectDir
        });
    }

    private void BuildProject(BuildContext context, string projectDir)
    {
        context.Information("Building the test project...");

        var buildSettings = new ProcessSettings
        {
            Arguments = "build --verbosity normal --no-restore -m:1",
            WorkingDirectory = projectDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        var buildResult = context.StartProcess("dotnet", buildSettings, out var output);

        if (buildResult != 0)
        {
            context.Error("Build failed! Output:");
            foreach (var line in output)
            {
                context.Error(line);
            }
            throw new Exception($"Test project failed to build!");
        }

        InspectNuGetPackagePaths(context, output);

        context.Information("✅ Build completed successfully");
    }

    // Regex to match any path ending with MonoGame.Framework.dll
    private static readonly Regex MonoGameFrameworkPathRegex = new(
        @"([^\s]+MonoGame\.Framework\.dll)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    private void InspectNuGetPackagePaths(BuildContext context, IEnumerable<string> buildOutput)
    {
        context.Information("🔍 Inspecting NuGet package paths for MonoGame.Framework.dll from build output...");

        var monoGameFrameworkPaths = new List<string>();

        foreach (var line in buildOutput)
        {
            var matches = MonoGameFrameworkPathRegex.Matches(line);
            foreach (Match match in matches)
            {
                var path = match.Groups[1].Value.Trim('"', ' ');
                monoGameFrameworkPaths.Add($"Path: {path}");
            }
        }

        if (monoGameFrameworkPaths.Count > 0)
        {
            context.Information($"📦 Found {monoGameFrameworkPaths.Count} MonoGame.Framework.dll references in build output:");
            foreach (var path in monoGameFrameworkPaths.Distinct())
            {
                context.Information($"  🔗 {path}");
            }
        }
        else
        {
            context.Warning("⚠️ No MonoGame.Framework.dll paths found in build output");
        }
    }

    private void CleanupPreviousTestRun(BuildContext context, string testsPath, string projectPath, string nugetSourceName)
    {
        context.Information($"🧹 Cleaning up previous {TemplateName} test run...");

        if (context.DirectoryExists(projectPath))
        {
            context.Information($"Removing existing test project: {projectPath}");
            context.DeleteDirectory(projectPath, new DeleteDirectorySettings
            {
                Recursive = true,
                Force = true
            });
        }

        context.Information($"✅ Cleanup completed for {TemplateName} target");
    }

    private void CleanupNuGetSource(BuildContext context, string nugetSourceName)
    {
        context.Information("Cleaning up local NuGet source...");
        try
        {
            context.StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"nuget remove source {nugetSourceName}"
            });
        }
        catch
        {
            // Source might not exist, continue
        }
    }

    private string GetTemplateVersionFromNuGet(BuildContext context, string nugetPath)
    {
        // Ensure proper path separators and remove any double separators
        var normalizedPath = nugetPath.TrimEnd(IOPath.DirectorySeparatorChar, IOPath.AltDirectorySeparatorChar);
        var searchPattern = System.IO.Path.Combine(normalizedPath, "MonoGame.Templates.CSharp.*.nupkg");
        var templateFiles = context.GetFiles(searchPattern);

        if (!templateFiles.Any())
        {
            throw new FileNotFoundException($"No MonoGame.Templates.CSharp NuGet package found in {nugetPath}");
        }

        var latestTemplateFile = templateFiles.OrderByDescending(f => f.GetFilename().ToString()).First();
        var fileName = latestTemplateFile.GetFilenameWithoutExtension().ToString();

        const string prefix = "MonoGame.Templates.CSharp.";
        return fileName.Substring(prefix.Length);
    }

    private void UpdateProjectReferences(BuildContext context, string projectDir, string version)
    {
        context.Information($"Updating project references to version {version} in: {projectDir}");

        if (!Directory.Exists(projectDir))
        {
            throw new DirectoryNotFoundException($"Project directory not found: {projectDir}");
        }

        // Find all .csproj files recursively in the project directory
        var csprojFiles = Directory.GetFiles(projectDir, "*.csproj", SearchOption.AllDirectories);

        if (csprojFiles.Length == 0)
        {
            context.Warning($"No .csproj files found in directory: {projectDir}");
            return;
        }

        context.Information($"Found {csprojFiles.Length} .csproj file(s) to process");

        var totalSuccessfulUpdates = 0;

        foreach (var csprojPath in csprojFiles)
        {
            var relativePath = IOPath.GetRelativePath(projectDir, csprojPath);
            context.Information($"📁 Processing: {relativePath}");

            var updatesInThisFile = UpdateProjectFile(context, csprojPath, version);
            totalSuccessfulUpdates += updatesInThisFile;
        }

        if (totalSuccessfulUpdates > 0)
        {
            context.Information($"✅ Updated {totalSuccessfulUpdates} package reference(s) across {csprojFiles.Length} project file(s)");
        }
        else
        {
            context.Warning("❌ No package references were successfully updated");
        }
    }

    private int UpdateProjectFile(BuildContext context, string csprojPath, string version)
    {
        var csprojContent = File.ReadAllText(csprojPath);
        context.Information($"   📝 Updating MonoGame package(s) to version {version}...");

        var updatedContent = csprojContent;
        bool successfulUpdate = false;

        var newContent = PackageReferenceRegex.Replace(updatedContent, $"${{partA}}{version}${{partB}}");

        if (newContent != updatedContent)
        {
            updatedContent = newContent;
            successfulUpdate = true;
            context.Information($"   ✅ Successfully updated {csprojPath} to version {version}");
        }
        else
        {
            context.Warning($"   ❌ Failed to update {csprojPath} version in csproj");
        }

        if (successfulUpdate)
        {
            File.WriteAllText(csprojPath, updatedContent);
            context.Information($"   ✅ Updated MonoGame package reference(s) in this file");
        }

        return successfulUpdate ? 1 : 0;
    }

    /// <summary>
    /// Display a summary report of all test results with status icons
    /// </summary>
    public static void DisplayTestSummary(BuildContext context)
    {
        if (TestResults.Count == 0)
        {
            context.Information("No test results to display.");
            return;
        }

        context.Information("");
        context.Information("🎯 MonoGame NuGet Template Test Summary");
        context.Information("==========================================");

        var successCount = 0;
        var skippedCount = 0;
        var failedCount = 0;

        foreach (var result in TestResults.OrderBy(r => r.TemplateName))
        {
            var icon = result.Status switch
            {
                TestStatus.Success => "✅",
                TestStatus.Skipped => "⚠️",
                TestStatus.Failed => "❌",
                _ => "❓"
            };

            context.Information($"{icon} {result.TemplateName} - {result.Message}");

            switch (result.Status)
            {
                case TestStatus.Success: successCount++; break;
                case TestStatus.Skipped: skippedCount++; break;
                case TestStatus.Failed: failedCount++; break;
            }
        }

        context.Information("");
        context.Information($"📊 Results: {successCount} successful, {skippedCount} skipped, {failedCount} failed");

        if (failedCount > 0)
        {
            context.Information($"❌ {failedCount} test(s) failed - check logs above for details");
        }
        else if (successCount > 0)
        {
            context.Information($"🎉 All {successCount} eligible test(s) completed successfully!");
        }
    }

    private void LogCurrentFileContents(BuildContext context, string projectDir)
    {
        context.Information("🔍 Inspecting file contents before build...");

        var toolsJsonPath = IOPath.Combine(projectDir, ".config", "dotnet-tools.json");
        if (File.Exists(toolsJsonPath))
        {
            context.Information($"📄 Contents of {toolsJsonPath}:");
            var toolsContent = File.ReadAllText(toolsJsonPath);
            context.Information(toolsContent);
            context.Information(""); // Empty line for readability
        }

        var csprojFiles = Directory.GetFiles(projectDir, "*.csproj", SearchOption.AllDirectories);
        foreach (var csprojFile in csprojFiles)
        {
            var relativePath = IOPath.GetRelativePath(projectDir, csprojFile);
            context.Information($"📄 Contents of {relativePath}:");
            var csprojContent = File.ReadAllText(csprojFile);
            context.Information(csprojContent);
            context.Information(""); // Empty line for readability
        }
    }

    /// <summary>
    /// Clear test results (useful for multiple test runs)
    /// </summary>
    public static void ClearTestResults()
    {
        TestResults.Clear();
    }
}
