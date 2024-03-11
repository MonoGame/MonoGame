// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Build.GitHubActions.Data;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Pack;
using Cake.Common.Tools.DotNet.Publish;
using Cake.Common.Tools.DotNet.Restore;
using Cake.Common.Tools.DotNet.Test;
using Cake.Common.Tools.MSBuild;
using Cake.Common.Tools.VSWhere;
using Cake.Common.Tools.VSWhere.Latest;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using System;

namespace MonoGame.Framework.Build;

public class BuildContext : FrostingContext
{
    public static readonly Version DefaultVersionNumber = new Version("3.8.1.303");

    public string Version { get; }
    public string RepositoryUrl { get; }
    public string BuildConfiguration { get; }
    public string ArtifactsDirectory { get; }
    public string NuGetsDirectory { get; }
    public DotNetBuildSettings DotNetBuildSettings { get; }
    public DotNetMSBuildSettings DotNetMSBuildSettings { get; }
    public DotNetPackSettings DotNetPackSettings { get; }
    public DotNetPublishSettings DotNetPublishSettings { get; }
    public DotNetRestoreSettings DotNetRestoreSettings { get; }
    public MSBuildSettings MSBuildSettings { get; }
    public MSBuildSettings MSPackSettings { get; }
    public DotNetTestSettings DotNetTestSettings { get; }

    public BuildContext(ICakeContext context) : base(context)
    {
        BuildConfiguration = context.Argument<string>("build-configuration", "Release");
        ArtifactsDirectory = context.Argument<string>("artifacts-directory", "Artifacts");

        NuGetsDirectory = $"{ArtifactsDirectory}/NuGet";

        if (context.BuildSystem().IsRunningOnGitHubActions)
        {
            GitHubActionsWorkflowInfo workflow = context.BuildSystem().GitHubActions.Environment.Workflow;
            string version = $"{DefaultVersionNumber.Major}.{DefaultVersionNumber.Minor}.{DefaultVersionNumber.Build}.{workflow.RunNumber}";

            if (workflow.Repository == "MonoGame/MonoGame" &&
               workflow.RefType == GitHubActionsRefType.Branch &&
               workflow.RefName == "refs/heads/master")
            {
                Version = $"{version}-develop";
            }
            else
            {
                Version = $"{version}-{workflow.RepositoryOwner}";
            }

            RepositoryUrl = $"https://github.com/{workflow.Repository}";
        }
        else
        {
            string buildNumber = context.EnvironmentVariable("BUILD_NUMBER", DefaultVersionNumber.ToString());
            string version = context.Argument("build-version", buildNumber);
            string branchName = context.EnvironmentVariable("BRANCH_NAME", string.Empty);

            if (!branchName.Contains("master"))
            {
                Version = $"{version}-develop";
            }
            else
            {
                Version = version;
            }

            RepositoryUrl = context.EnvironmentVariable("repository-url", "https://github.com/MonoGame/MonoGame");
        }

        DotNetMSBuildSettings = new DotNetMSBuildSettings();
        DotNetMSBuildSettings.WithProperty(nameof(Version), Version);
        DotNetMSBuildSettings.WithProperty(nameof(RepositoryUrl), RepositoryUrl);

        DotNetBuildSettings = new DotNetBuildSettings();
        DotNetBuildSettings.MSBuildSettings = DotNetMSBuildSettings;
        DotNetBuildSettings.Verbosity = DotNetVerbosity.Minimal;
        DotNetBuildSettings.Configuration = BuildConfiguration;

        DotNetPackSettings = new DotNetPackSettings();
        DotNetPackSettings.MSBuildSettings = DotNetMSBuildSettings;
        DotNetPackSettings.Verbosity = DotNetVerbosity.Minimal;
        DotNetPackSettings.OutputDirectory = NuGetsDirectory;
        DotNetPackSettings.Configuration = BuildConfiguration;

        DotNetRestoreSettings = new DotNetRestoreSettings();
        DotNetRestoreSettings.MSBuildSettings = DotNetMSBuildSettings;
        DotNetRestoreSettings.Verbosity = DotNetVerbosity.Minimal;

        MSBuildSettings = new MSBuildSettings();
        MSBuildSettings.Verbosity = Verbosity.Minimal;
        MSBuildSettings.Configuration = BuildConfiguration;
        MSBuildSettings.WithProperty(nameof(Version), Version);
        MSBuildSettings.WithProperty(nameof(RepositoryUrl), RepositoryUrl);

        MSPackSettings = new MSBuildSettings();
        MSPackSettings.Verbosity = Verbosity.Minimal;
        MSPackSettings.Configuration = BuildConfiguration;
        MSPackSettings.Restore = true;
        MSPackSettings.WithProperty(nameof(Version), Version);
        MSPackSettings.WithProperty(nameof(RepositoryUrl), RepositoryUrl);
        MSPackSettings.WithProperty("OutputDirectory", NuGetsDirectory);
        MSPackSettings.WithTarget("Pack");

        DotNetPublishSettings = new DotNetPublishSettings();
        DotNetPublishSettings.MSBuildSettings = DotNetMSBuildSettings;
        DotNetPublishSettings.Verbosity = DotNetVerbosity.Minimal;
        DotNetPublishSettings.Configuration = BuildConfiguration;
        DotNetPublishSettings.SelfContained = false;

        DotNetTestSettings = new DotNetTestSettings();
        DotNetTestSettings.MSBuildSettings = DotNetMSBuildSettings;
        DotNetTestSettings.Verbosity = DotNetVerbosity.Minimal;
        DotNetTestSettings.Configuration = BuildConfiguration;

        Console.WriteLine($"Version: {Version}");
        Console.WriteLine($"RepositoryUrl: {RepositoryUrl}");
        Console.WriteLine($"BuildConfiguration: {BuildConfiguration}");
    }

    public bool GetMSBuildWith(string requires)
    {
        if (this.IsRunningOnWindows())
        {
            return false;
        }

        DirectoryPath vsLatest = this.VSWhereLatest(new VSWhereLatestSettings() { Requires = requires });
        if (vsLatest == null)
        {
            return false;
        }

        FilePathCollection files = this.GetFiles(vsLatest.FullPath + "/**/MSBuild.exe");
        return files.Count > 0;
    }
}
