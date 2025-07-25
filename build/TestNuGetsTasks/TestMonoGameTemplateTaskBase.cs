using System.Text.RegularExpressions;
using System.IO;
using IOPath = System.IO.Path;

namespace BuildScripts;

public abstract class TestMonoGameTemplateTaskBase : FrostingTask<BuildContext>
{
    // Pre-compiled regex patterns for better performance
    private static readonly Regex PackageReferenceRegex = new(@"<PackageReference\s+Include=""(MonoGame\.[^""]*)""\s+Version=""([^""]*)""\s*/?(?:\s*/>|>)", RegexOptions.Compiled);
    
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
            // Step 0: Clean up any previous test artifacts
            CleanupPreviousTestRun(context, testsPath, projectPath, nugetSourceName);

            // Step 1: Setup NuGet source pointing to Artifacts/NuGet folder
            SetupNuGetSource(context, nugetSourcePath, nugetSourceName);

            // Step 2: Remove any existing MonoGame dotnet templates
            UninstallExistingTemplates(context);

            // Step 3: Work out the version from the Templates NuGet package
            var templateVersion = GetTemplateVersionFromNuGet(context, nugetSourcePath);
            context.Information($"Detected MonoGame template version: {templateVersion}");

            // Step 4: Install the templates using the detected version
            InstallTemplates(context, templateVersion, nugetSourcePath);

            // Step 5: Create the template tests folder
            context.Information($"Creating template tests folder: {testsPath}");
            context.CreateDirectory(testsPath);

            // Step 6: Create a new MonoGame project
            CreateTestProject(context, projectPath);

            // Step 6.5: Replace dotnet-tools.json with platform-specific version
            var projectDir = IOPath.Combine(projectPath, "TestProject");
            ReplaceDotnetToolsConfig(context, projectDir, templateVersion);

            // Step 7: Update the project references to use the version being tested
            UpdateProjectReferences(context, projectDir, templateVersion);

            // Step 8: Restore packages for the project
            RestoreProject(context, projectDir);

            // Step 8.5: Log file contents before build for debugging
            LogCurrentFileContents(context, projectDir);

            // Step 9: Run dotnet build to verify the project builds
            BuildProject(context, projectDir);

            context.Information($"✅ Test completed successfully! MonoGame {TemplateName} project built without errors.");
            context.Information($"📁 Test project preserved at: {projectDir}");
            
            // Record the success result
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
            // Record the failure result
            TestResults.Add(new TestResult
            {
                TemplateName = TemplateName,
                Status = TestStatus.Failed,
                Message = ex.Message,
                Platform = currentPlatform
            });
            
            throw; // Re-throw to maintain existing error handling behavior
        }
        finally
        {
            // Cleanup: Remove the local NuGet source
            CleanupNuGetSource(context, nugetSourceName);
        }
    }

    private void SetupNuGetSource(BuildContext context, string nugetSourcePath, string nugetSourceName)
    {
        context.Information($"Setting up NuGet source from: {nugetSourcePath}");
        
        // Remove source if it exists (ignore errors)
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

        // Try using CAKE's cross-platform process execution with proper argument handling
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
            // Template might not be installed, continue
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

        // Always replace with platform-specific version to avoid cross-platform tool issues
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

        // Capture build output to only show it on failure
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
            // Build failed - show the output for debugging
            context.Error("Build failed! Output:");
            foreach (var line in output)
            {
                context.Error(line);
            }
            throw new Exception($"Test project failed to build!");
        }
        
        // Build succeeded - just show a success message without the verbose output
        context.Information("✅ Build completed successfully");
    }

    private void CleanupPreviousTestRun(BuildContext context, string testsPath, string projectPath, string nugetSourceName)
    {
        context.Information($"🧹 Cleaning up previous {TemplateName} test run...");
        
        // Remove any existing test project directory for this specific target
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
        
        // Remove "MonoGame.Templates.CSharp." prefix to get the version
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
        // Read and analyze the csproj file directly using regex
        var csprojContent = File.ReadAllText(csprojPath);
        var monoGamePackages = new List<string>();
        
        // Find all MonoGame PackageReference elements using cached regex
        var matches = PackageReferenceRegex.Matches(csprojContent);
        
        foreach (Match match in matches)
        {
            var packageName = match.Groups[1].Value;
            var currentVersion = match.Groups[2].Value;
            monoGamePackages.Add(packageName);
            context.Information($"📦 Found MonoGame package: {packageName} (current version: {currentVersion})");
        }

        if (monoGamePackages.Count == 0)
        {
            context.Information("   No MonoGame packages found in this file");
            return 0;
        }
        
        // Update packages directly via csproj editing (more reliable than dotnet add package for prerelease versions)
        context.Information($"   📝 Updating {monoGamePackages.Count} package(s) to version {version}...");
        
        var updatedContent = csprojContent;
        var successfulUpdates = 0;
        
        foreach (var packageName in monoGamePackages)
        {
            // Pre-escape the package name once for reuse
            var escapedPackageName = Regex.Escape(packageName);
            
            // Handle both self-closing and open tag formats properly
            var selfClosingPattern = $@"<PackageReference\s+Include=""{escapedPackageName}""\s+Version=""[^""]*""\s*/>";
            var openTagPattern = $@"(<PackageReference\s+Include=""{escapedPackageName}""\s+Version="")[^""]*("">)";
            
            // First try self-closing format
            var selfClosingReplacement = $@"<PackageReference Include=""{packageName}"" Version=""{version}"" />";
            var newContent = Regex.Replace(updatedContent, selfClosingPattern, selfClosingReplacement);
            
            // If no change, try open tag format (just update the version, preserve the rest)
            if (newContent == updatedContent)
            {
                var openTagReplacement = $@"${{1}}{version}${{2}}";
                newContent = Regex.Replace(updatedContent, openTagPattern, openTagReplacement);
            }
            
            if (newContent != updatedContent)
            {
                updatedContent = newContent;
                successfulUpdates++;
                context.Information($"   ✅ Successfully updated {packageName} to version {version}");
            }
            else
            {
                context.Warning($"   ❌ Failed to update {packageName} version in csproj");
            }
        }
        
        if (successfulUpdates > 0)
        {
            // Write the updated content back to the file
            File.WriteAllText(csprojPath, updatedContent);
            context.Information($"   ✅ Updated {successfulUpdates} package reference(s) in this file");
        }
        
        return successfulUpdates;
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
        
        // Log dotnet-tools.json content
        var toolsJsonPath = IOPath.Combine(projectDir, ".config", "dotnet-tools.json");
        if (File.Exists(toolsJsonPath))
        {
            context.Information($"📄 Contents of {toolsJsonPath}:");
            var toolsContent = File.ReadAllText(toolsJsonPath);
            context.Information(toolsContent);
            context.Information(""); // Empty line for readability
        }

        // Log all .csproj files content
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
