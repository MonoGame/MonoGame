// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Framework.Content.Pipeline.Builder;

record ContentFileCache : IContentFileCache
{
    // ContentFileCache instances are compared by reference (ReferenceEquals), not record value equality.
    private readonly object _syncLock = new();

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
            fullDependencyPath = Path.GetFullPath(dependencyPath);
            relativeDependencyPath = Path.GetRelativePath(builder.Parameters.RootedSourceDirectory, dependencyPath);
        }
        else
        {
            fullDependencyPath = Path.GetFullPath(Path.Combine(builder.Parameters.RootedSourceDirectory, dependencyPath));
            relativeDependencyPath = dependencyPath;
        }

        if (!File.Exists(fullDependencyPath))
        {
            return;
        }

        var lastModifiedTime = File.GetLastWriteTimeUtc(fullDependencyPath);

        var sanitized = relativeDependencyPath.Sanitize();
        lock (_syncLock)
        {
            Dependencies[sanitized] = lastModifiedTime;
        }
    }

    public void RemoveDependency(ContentBuilder builder, string dependencyPath)
    {
        string relativeDependencyPath = Path.IsPathRooted(dependencyPath) ?
            Path.GetRelativePath(builder.Parameters.RootedSourceDirectory, dependencyPath) :
            dependencyPath;
        Dependencies.Remove(relativeDependencyPath);
    }

    public void AddDependency(ContentBuilder builder, IContentFileCache fileCache)
    {
        if (fileCache is not ContentFileCache cache || ReferenceEquals(this, cache))
        {
            return;
        }

        KeyValuePair<string, DateTime>[] dependencySnapshot;
        KeyValuePair<string, DateTime>[] outputSnapshot;

        lock (cache._syncLock)
        {
            dependencySnapshot = [.. cache.Dependencies];
            outputSnapshot = [.. cache.Outputs];
        }

        lock (_syncLock)
        {
            foreach (var dependency in dependencySnapshot)
            {
                Dependencies[dependency.Key] = dependency.Value;
            }

            foreach (var output in outputSnapshot)
            {
                Outputs[output.Key] = output.Value;
            }
        }
    }

    public void AddOutputFile(ContentBuilder builder, string outputPath)
    {
        string fullOutputPath;
        string relativeOutputPath;

        if (Path.IsPathRooted(outputPath))
        {
            fullOutputPath = Path.GetFullPath(outputPath);
            relativeOutputPath = Path.GetRelativePath(builder.Parameters.RootedOutputDirectory, outputPath);
        }
        else
        {
            fullOutputPath = Path.GetFullPath(Path.Combine(builder.Parameters.RootedOutputDirectory, outputPath));
            relativeOutputPath = outputPath;
        }

        if (!File.Exists(fullOutputPath))
        {
            return;
        }

        var lastModifiedTime = File.GetLastWriteTimeUtc(fullOutputPath);

        var sanitized = relativeOutputPath.Sanitize();
        lock (_syncLock)
        {
            Outputs[sanitized] = lastModifiedTime;
        }
    }

    public void RemoveOutputFile(ContentBuilder builder, string outputPath)
    {
        var relativeOutputFile = Path.IsPathRooted(outputPath) ?
            Path.GetRelativePath(builder.Parameters.RootedOutputDirectory, outputPath) :
            outputPath;
        Outputs.Remove(relativeOutputFile);
    }

    // TODO: We can remove this once we are entirely happy with the parallel builds.
    // It's added solely to have a comparison between serial and parallel builds in the tests.
    internal void EnsureDeterministicOrder()
    {
        lock (_syncLock)
        {
            SortDictionary(Dependencies);
            SortDictionary(Outputs);
        }
    }

    /// <summary>
    /// Sorts the dictionary relying on insert-order.
    /// </summary>
    /// <remarks>
    /// Should be called within <see cref="_syncLock"/> lock.
    /// </remarks>
    /// <param name="dictionary">Dictionary to sort.</param>
    private static void SortDictionary(Dictionary<string, DateTime> dictionary)
    {
        if (dictionary.Count <= 1)
        {
            return;
        }

        var sorted = dictionary
            .OrderBy(static kvp => kvp.Key, StringComparer.Ordinal)
            .ToArray();

        dictionary.Clear();
        foreach (var (key, value) in sorted)
        {
            dictionary[key] = value;
        }
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

        KeyValuePair<string, DateTime>[] dependencySnapshot;
        KeyValuePair<string, DateTime>[] outputSnapshot;

        lock (_syncLock)
        {
            dependencySnapshot = [.. Dependencies];
            outputSnapshot = [.. Outputs];
        }

        foreach (var (dependencyFile, cachedModifiedTime) in dependencySnapshot)
        {
            var dependencyFilePath = Path.Combine(builder.Parameters.RootedSourceDirectory, dependencyFile);
            var modifiedTime = File.GetLastWriteTimeUtc(dependencyFilePath);

            if (modifiedTime != cachedModifiedTime)
            {
                return false;
            }
        }

        foreach (var (outputPath, cachedModifiedTime) in outputSnapshot)
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
