// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder;

class ContentCache : IContentCache
{
    private const string CacheFileName = "cache.yaml";

    private readonly object _cacheLock = new();
    private Dictionary<string, ContentFileCache> _cache = [];
    private readonly HashSet<string> _unusedOutputs = [];

    public void LoadCache(ContentBuilder builder)
    {
        _cache.Clear();
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
            foreach (var (outputPath, _) in fileCache.Outputs)
            {
                _unusedOutputs.Add(outputPath);
            }
        }
    }

    public void FlushCache(ContentBuilder builder)
    {
        var cacheFilePath = Path.Combine(builder.Parameters.RootedIntermediateDirectory, CacheFileName);
        var dirPath = Path.GetDirectoryName(cacheFilePath);
        if (string.IsNullOrEmpty(dirPath))
        {
            return;
        }

        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }

        string text;
        lock (_cacheLock)
        {
            var sortedCache = new SortedDictionary<string, ContentFileCache>(StringComparer.Ordinal);
            foreach (var (key, fileCache) in _cache)
            {
                fileCache.EnsureDeterministicOrder();
                sortedCache[key] = fileCache;
            }

            text = ContentBuilderHelper.Serializer.Serialize(sortedCache);
        }

        File.WriteAllText(cacheFilePath, text);
    }

    public IContentFileCache CreateContentFileCache(ContentBuilder builder, ContentInfo info) => new ContentFileCache
    {
        ContentRoot = info.ContentRoot,
        CompressContent = builder.Parameters.CompressContent,
        GraphicsProfile = builder.Parameters.GraphicsProfile,
        ShouldBuild = info.ShouldBuild,
        Importer = info.Importer,
        Processor = info.Processor
    };

    public IContentFileCache? ReadContentFileCache(ContentBuilder builder, string relativeOutputPath)
    {
        var key = relativeOutputPath.Sanitize();
        lock (_cacheLock)
        {
            if (!_cache.TryGetValue(key, out ContentFileCache? fileCache))
            {
                return null;
            }
            return fileCache;
        }
    }

    public void WriteContentFileCache(ContentBuilder builder, string relativeOutputPath, IContentFileCache? fileCache)
    {
        if (fileCache is ContentFileCache builderFileCache)
        {
            var key = relativeOutputPath.Sanitize();
            lock (_cacheLock)
            {
                _cache[key] = builderFileCache;
            }
        }
    }

    public void MarkUsed(IContentFileCache fileCache)
    {
        if (fileCache is not ContentFileCache builderFileCache)
        {
            return;
        }

        lock (_cacheLock)
        {
            foreach (var (outputPath, _) in builderFileCache.Outputs)
            {
                _unusedOutputs.Remove(outputPath);
            }
        }
    }

    public void CleanCache(ContentBuilder builder)
    {
        foreach (var outputPath in _unusedOutputs)
        {
            _cache.Remove(outputPath);
        }

        foreach (var outputPath in _unusedOutputs)
        {
            var outputFilePath = Path.Combine(builder.Parameters.RootedOutputDirectory, outputPath);
            if (!IsOutputUsed(outputPath) && File.Exists(outputFilePath))
            {
                builder.Logger.Log("Deleting: " + outputPath);
                File.Delete(outputFilePath);
            }
        }

        _unusedOutputs.Clear();
    }

    private bool IsOutputUsed(string outputPathToCheck)
    {
        foreach (var (mainOutput, fileCache) in _cache)
        {
            if (mainOutput == outputPathToCheck)
            {
                return true;
            }

            foreach (var (outputPath, _) in fileCache.Outputs)
            {
                if (outputPath == outputPathToCheck)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
