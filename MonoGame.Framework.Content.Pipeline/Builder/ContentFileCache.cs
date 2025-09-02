// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Framework.Content.Pipeline.Builder;

record ContentFileCache : IContentFileCache
{
    public string ContentRoot { get; init; } = "";

    public bool ShouldBuild { get; init; } = false;

    public IContentImporter? Importer { get; init; } = null;

    public IContentProcessor? Processor { get; init; } = null;

    public bool CompressContent { get; init; } = false;

    public GraphicsProfile GraphicsProfile { get; init; } = GraphicsProfile.HiDef;

    public Dictionary<string, DateTime> Dependencies { get; init; } = [];

    public Dictionary<string, DateTime> Outputs { get; init; } = [];

    public void AddDependency(ContentBuilder builder, string dependencyPath)
    {
        string fullDependencyPath;
        string relativeDependencyPath;

        if (Path.IsPathRooted(dependencyPath))
        {
            fullDependencyPath = dependencyPath;
            relativeDependencyPath = Path.GetRelativePath(builder.Parameters.RootedSourceDirectory, dependencyPath);
        }
        else
        {
            fullDependencyPath = Path.Combine(builder.Parameters.RootedSourceDirectory, dependencyPath);
            relativeDependencyPath = dependencyPath;
        }

        if (!File.Exists(fullDependencyPath))
        {
            return;
        }

        var lastModifiedTime = File.GetLastWriteTimeUtc(fullDependencyPath);
        Dependencies[relativeDependencyPath.Sanitize()] = lastModifiedTime;
    }

    public void RemoveDependency(ContentBuilder builder, string dependencyPath)
    {
        string relativeDependencyPath = Path.IsPathRooted(dependencyPath) ?
            Path.GetRelativePath(builder.Parameters.RootedSourceDirectory, dependencyPath) :
            dependencyPath;
        Dependencies.Remove(relativeDependencyPath);
    }

    public void AddOutputFile(ContentBuilder builder, string outputPath)
    {
        string fullOutputPath;
        string relativeOutputPath;

        if (Path.IsPathRooted(outputPath))
        {
            fullOutputPath = outputPath;
            relativeOutputPath = Path.GetRelativePath(builder.Parameters.RootedOutputDirectory, outputPath);
        }
        else
        {
            fullOutputPath = Path.Combine(builder.Parameters.RootedOutputDirectory, outputPath);
            relativeOutputPath = outputPath;
        }

        if (!File.Exists(fullOutputPath))
        {
            return;
        }

        var lastModifiedTime = File.GetLastWriteTimeUtc(fullOutputPath);
        Outputs[relativeOutputPath.Sanitize()] = lastModifiedTime;
    }

    public void RemoveOutputFile(ContentBuilder builder, string outputPath)
    {
        var relativeOutputFile = Path.IsPathRooted(outputPath) ?
            Path.GetRelativePath(builder.Parameters.RootedOutputDirectory, outputPath) :
            outputPath;
        Outputs.Remove(relativeOutputFile);
    }

    public bool IsValid(ContentBuilder builder, ContentInfo info)
    {
        if (info.ShouldBuild || ShouldBuild)
        {
            if (builder.Parameters.GraphicsProfile != GraphicsProfile ||
                builder.Parameters.CompressContent != CompressContent ||
                info.ContentRoot != ContentRoot ||
                info.ShouldBuild != ShouldBuild ||
                !ContentBuilderHelper.ArePropsEqual(Importer, info.Importer) ||
                !ContentBuilderHelper.ArePropsEqual(Processor, info.Processor))
            {
                return false;
            }
        }

        foreach (var (dependencyFile, cachedModifiedTime) in Dependencies)
        {
            var dependencyFilePath = Path.Combine(builder.Parameters.RootedSourceDirectory, dependencyFile);
            var modifiedTime = File.GetLastWriteTimeUtc(dependencyFilePath);

            if (modifiedTime != cachedModifiedTime)
            {
                return false;
            }
        }

        foreach (var (outputPath, cachedModifiedTime) in Outputs)
        {
            var outputFilePath = Path.Combine(builder.Parameters.RootedOutputDirectory, outputPath);
            var modifiedTime = File.GetLastWriteTimeUtc(outputFilePath);

            if (modifiedTime != cachedModifiedTime)
            {
                return false;
            }
        }

        return true;
    }
}
