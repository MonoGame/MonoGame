// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder;

class ContentCache : IContentCache
{
    private const string CacheFileName = "cache.yaml";

    private Dictionary<string, ContentFileCache> _cache = [];
    private readonly HashSet<string> _unusedDependencies = [];
    private readonly HashSet<string> _unusedOutputs = [];

    public void LoadCache(ContentBuilder builder)
    {
        _cache.Clear();
        _unusedDependencies.Clear();
        _unusedOutputs.Clear();

        var cacheFilePath = Path.Combine(builder.Parameters.RootedIntermediateDirectory, CacheFileName);
        if (!File.Exists(cacheFilePath))
        {
            return;
        }

        try
        {
            var text = File.ReadAllText(cacheFilePath);
            _cache = ContentBuilderHelper.Deserializer.Deserialize<Dictionary<string, ContentFileCache>>(text) ?? [];
        }
        catch (Exception ex)
        {
            builder.Logger.Log(LogLevel.Error, "Failed to load the Cache!");
            builder.Logger.Log(LogLevel.Error, ex.ToString());
        }

        foreach (var (_, fileCache) in _cache)
        {
            foreach (var (depFile, _) in fileCache.Dependencies)
            {
                _unusedDependencies.Add(depFile);
            }

            foreach (var output in fileCache.Outputs)
            {
                _unusedOutputs.Add(output);
            }
        }
    }

    public ContentFileCache? ReadContentFileCache(ContentBuilder builder, string relativePath, string contentRoot, bool shouldBuild = false, IContentImporter? importer = null, IContentProcessor? processor = null)
    {
        if (!_cache.TryGetValue(relativePath, out ContentFileCache? fileCache))
        {
            return null;
        }

        if (builder.Parameters.GraphicsProfile != fileCache.GraphicsProfile ||
            builder.Parameters.CompressContent != fileCache.CompressContent ||
            contentRoot != fileCache.ContentRoot ||
            shouldBuild != fileCache.ShouldBuild ||
            !ContentBuilderHelper.ArePropsEqual(fileCache.Importer, importer) ||
            !ContentBuilderHelper.ArePropsEqual(fileCache.Processor, processor))
        {
            return null;
        }

        foreach (var (dependencyFile, cachedModifiedTime) in fileCache.Dependencies)
        {
            var dependencyFullPath = Path.Combine(builder.Parameters.RootedSourceDirectory, dependencyFile);
            var modifiedTime = File.GetLastWriteTimeUtc(dependencyFullPath);

            if (modifiedTime != cachedModifiedTime)
            {
                return null;
            }
        }

        foreach (var outputPath in fileCache.Outputs)
        {
            var fullOutputPath = Path.Combine(builder.Parameters.RootedOutputDirectory, outputPath);

            if (!File.Exists(fullOutputPath))
            {
                return null;
            }
        }

        MarkUsed(fileCache);
        return fileCache;
    }

    public void WriteContentFileCache(ContentBuilder builder, string relativePath, ContentFileCache fileCache)
    {
        _cache[relativePath] = fileCache;
        MarkUsed(fileCache);
    }

    public void FlushCache(ContentBuilder builder)
    {
        var cacheFilePath = Path.Combine(builder.Parameters.RootedIntermediateDirectory, CacheFileName);
        var dirPath = Path.GetDirectoryName(cacheFilePath);
        if (string.IsNullOrEmpty(dirPath))
        {
            return;
        }

        if (!File.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        var text = ContentBuilderHelper.Serializer.Serialize(_cache);
        File.WriteAllText(cacheFilePath, text);
    }

    private void MarkUsed(ContentFileCache fileCache)
    {
        foreach (var (depFile, _) in fileCache.Dependencies)
        {
            _unusedDependencies.Remove(depFile);
        }

        foreach (var output in fileCache.Outputs)
        {
            _unusedOutputs.Remove(output);
        }
    }

    public void CleanCache(ContentBuilder builder)
    {
        foreach (var depFile in _unusedDependencies)
        {
            _cache.Remove(depFile);
        }

        foreach (var outputFile in _unusedOutputs)
        {
            var outputFilePath = Path.Combine(builder.Parameters.RootedOutputDirectory, outputFile);
            if (File.Exists(outputFilePath))
            {
                builder.Logger.Log("Deleting: " + outputFile);
                File.Delete(outputFilePath);
            }
        }

        _unusedDependencies.Clear();
        _unusedOutputs.Clear();
    }
}
