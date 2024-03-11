// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Build.GitHubActions.Data;
using Cake.Core;
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
    }
}
