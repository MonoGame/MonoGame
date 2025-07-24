using System.Text.RegularExpressions;

namespace BuildScripts;

public abstract class TestMonoGameTemplateTaskBase : FrostingTask<BuildContext>
{
    protected abstract string TemplateName { get; }
    protected abstract string ProjectFolderName { get; }
    protected abstract string TemplateShortName { get; }
    protected abstract PlatformFamily[] SupportedPlatforms { get; }

    public override bool ShouldRun(BuildContext context)
    {
        var currentPlatform = context.Environment.Platform.Family;
        var isSupported = SupportedPlatforms.Contains(currentPlatform);
        
        if (!isSupported)
        {
            context.Information($"⏭️ Skipping {TemplateName} test - not supported on {currentPlatform}");
            context.Information($"   Supported platforms: {string.Join(", ", SupportedPlatforms)}");
        }
        
        return isSupported;
    }

    public override void Run(BuildContext context)
    {
        var nugetSourcePath = System.IO.Path.GetFullPath(context.NuGetsDirectory);
        var testsPath = context.GetOutputPath("TemplateTests");
        var projectPath = $"{testsPath}/{ProjectFolderName}";
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
            var projectDir = $"{projectPath}/TestProject";
            ReplaceDotnetToolsConfig(context, projectDir, templateVersion);

            // Step 7: Update the project references to use the version being tested
            UpdateProjectReferences(context, projectDir, templateVersion, nugetSourceName);

            // Step 8: Restore packages for the project
            RestoreProject(context, projectDir);

            // Step 9: Run dotnet build to verify the project builds
            BuildProject(context, projectDir);

            context.Information($"✅ Test completed successfully! MonoGame {TemplateName} project built without errors.");
            context.Information($"📁 Test project preserved at: {projectDir}");
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
        var configDir = $"{projectDir}/.config";
        var dotnetToolsPath = $"{configDir}/dotnet-tools.json";
        
        context.Information($"Replacing dotnet-tools.json with platform-specific version for: {context.Environment.Platform.Family}");
        
        if (!System.IO.Directory.Exists(configDir))
        {
            context.CreateDirectory(configDir);
        }

        // Always replace with platform-specific version to avoid cross-platform tool issues
        var toolsJson = GetPlatformSpecificToolsJson(context, version);
        System.IO.File.WriteAllText(dotnetToolsPath, toolsJson);
        
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
        var buildResult = context.StartProcess("dotnet", new ProcessSettings
        {
            Arguments = "build --verbosity normal --no-restore",
            WorkingDirectory = projectDir
        });

        if (buildResult != 0)
        {
            throw new Exception($"Test project failed to build!");
        }
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
        var normalizedPath = nugetPath.TrimEnd('/', '\\');
        var templateFiles = context.GetFiles($"{normalizedPath}/MonoGame.Templates.CSharp.*.nupkg");
        
        if (!templateFiles.Any())
        {
            throw new FileNotFoundException($"No MonoGame.Templates.CSharp NuGet package found in {nugetPath}");
        }

        var latestTemplateFile = templateFiles.OrderByDescending(f => f.GetFilename().ToString()).First();
        var fileName = latestTemplateFile.GetFilenameWithoutExtension().ToString();
        
        // Remove "MonoGame.Templates.CSharp." prefix to get the version
        var version = fileName.Substring("MonoGame.Templates.CSharp.".Length);
        
        return version;
    }

    private void UpdateProjectReferences(BuildContext context, string projectDir, string version, string nugetSourceName)
    {
        context.Information($"Updating project references to version {version} in: {projectDir}");
        
        if (!System.IO.Directory.Exists(projectDir))
        {
            throw new DirectoryNotFoundException($"Project directory not found: {projectDir}");
        }

        // Find all .csproj files recursively in the project directory
        var csprojFiles = System.IO.Directory.GetFiles(projectDir, "*.csproj", System.IO.SearchOption.AllDirectories);
        
        if (csprojFiles.Length == 0)
        {
            context.Warning($"No .csproj files found in directory: {projectDir}");
            return;
        }

        context.Information($"Found {csprojFiles.Length} .csproj file(s) to process");
        
        var totalSuccessfulUpdates = 0;

        foreach (var csprojPath in csprojFiles)
        {
            var relativePath = System.IO.Path.GetRelativePath(projectDir, csprojPath);
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
        var csprojContent = System.IO.File.ReadAllText(csprojPath);
        var monoGamePackages = new List<string>();
        
        // Find all MonoGame PackageReference elements using regex
        var packageReferencePattern = @"<PackageReference\s+Include=""(MonoGame\.[^""]*)""\s+Version=""([^""]*)""\s*/>";
        var matches = System.Text.RegularExpressions.Regex.Matches(csprojContent, packageReferencePattern);
        
        foreach (System.Text.RegularExpressions.Match match in matches)
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
            // Replace the version for this specific package
            var pattern = $@"<PackageReference\s+Include=""{System.Text.RegularExpressions.Regex.Escape(packageName)}""\s+Version=""[^""]*""\s*/>";
            var replacement = $@"<PackageReference Include=""{packageName}"" Version=""{version}"" />";
            
            var newContent = System.Text.RegularExpressions.Regex.Replace(updatedContent, pattern, replacement);
            
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
            System.IO.File.WriteAllText(csprojPath, updatedContent);
            context.Information($"   ✅ Updated {successfulUpdates} package reference(s) in this file");
        }
        
        return successfulUpdates;
    }
}
