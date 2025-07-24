using System.Text.RegularExpressions;

namespace BuildScripts;

public abstract class TestMonoGameTemplateTaskBase : FrostingTask<BuildContext>
{
    protected abstract string TemplateName { get; }
    protected abstract string ProjectFolderName { get; }
    protected abstract string TemplateShortName { get; }

    public override void Run(BuildContext context)
    {
        var nugetSourcePath = System.IO.Path.GetFullPath(context.NuGetsDirectory);
        var testsPath = context.GetOutputPath("tests");
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

            // Step 5: Create the tests folder
            context.Information($"Creating tests folder: {testsPath}");
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

        // Use PowerShell directly to avoid CAKE argument parsing issues
        var result = context.StartProcess("powershell", new ProcessSettings
        {
            Arguments = $"-Command \"dotnet nuget add source '{nugetSourcePath}' --name {nugetSourceName}\""
        });
        
        if (result != 0)
        {
            throw new Exception($"Failed to add NuGet source {nugetSourceName}");
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

        string toolsJson = GetPlatformSpecificToolsJson(context, version);
        
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
        
        // Remove any existing NuGet source for this target
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

        // Get list of currently referenced MonoGame packages
        context.Information("Detecting existing MonoGame package references...");
        var listResult = context.StartProcess("dotnet", new ProcessSettings
        {
            Arguments = "list package",
            WorkingDirectory = projectDir,
            RedirectStandardOutput = true
        }, out var packageListOutput);

        var monoGamePackages = new List<string>();
        foreach (var line in packageListOutput)
        {
            // Look for lines containing MonoGame packages
            if (line.Contains("MonoGame.") && line.Contains(">"))
            {
                // Extract package name from the line format: "   > PackageName    Version"
                var parts = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 3 && parts[1] == ">")
                {
                    var packageName = parts[2];
                    monoGamePackages.Add(packageName);
                    context.Information($"Found MonoGame package: {packageName}");
                }
            }
        }

        // Update each detected MonoGame package to the specified version
        foreach (var packageName in monoGamePackages)
        {
            context.Information($"Updating {packageName} to version {version}");
            context.StartProcess("dotnet", new ProcessSettings
            {
                Arguments = $"add package {packageName} -v {version} -s {nugetSourceName}",
                WorkingDirectory = projectDir
            });
        }

        if (monoGamePackages.Count == 0)
        {
            context.Warning("No MonoGame packages found to update. This might indicate an issue with package detection.");
        }
        else
        {
            context.Information($"Successfully updated {monoGamePackages.Count} MonoGame package reference(s) using dotnet add package.");
        }
    }
}
