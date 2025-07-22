// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections;
using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MonoGame.Framework.Content.Pipeline.Builder.Server;

namespace MonoGame.Framework.Content.Pipeline.Builder;

/// <summary>
/// This class is the entry point for the content builder system.
/// </summary>
public abstract class ContentBuilder
{
    private class ContentRequest
    {
        public required string InputPath { get; init; }

        public required ContentInfo ContentInfo { get; init; }

        public required ContentServer Server { get; set; }

        public required ContentRequestedArgs Args { get; set; }
    }

    private readonly Queue<ContentRequest> _contentRequestQueue = [];
    private readonly object _contentRequestLock = new();
    private readonly Dictionary<string, ContentInfo> _content = [];
    private readonly Dictionary<string, string> _outputContent = [];
    private uint _succeededToBuild = 0;
    private uint _failedToBuild = 0;

    /// <summary>
    /// Parameters to be used by the <see cref="ContentBuilder"/> or any of its subsystems.
    ///
    /// Can be passed from CLI args, see <see cref="Run(string[])"/>.
    /// </summary>
    public ContentBuilderParams Parameters { get; set; } = new ContentBuilderParams();

    /// <summary>
    /// Gets or sets the logger to be used by the <see cref="ContentBuilder"/>.
    /// </summary>
    /// <value><see cref="ContentBuildLogger"/> by default.</value>
    public ContentBuildLogger Logger { get; set; } = new ContentBuildLogger();

    /// <summary>
    /// Gets or sets the content cahcing system to be used by the <see cref="ContentBuilder"/>.
    /// </summary>
    public virtual IContentCache ContentCache { get; init; } = new ContentCache();

    /// <summary>
    /// Called to build system to gather information about how to handle the content. It gets called only once during initialization.
    /// </summary>
    /// <returns>An <see cref="IContentCollection"/> that contains information about the content handling.</returns>
    public abstract IContentCollection GetContentCollection();

    /// <summary>
    /// Initiates a build of the specified asset and then writes down the result to disk..
    /// </summary>
    /// <param name="relativePath">A relative path to the source asset.</param>
    /// <param name="contentInfo">The desired <see cref="ContentInfo"/> to be used for the content building.</param>
    /// <returns></returns>
    public ContentFileCache? BuildAndWriteContent(string relativePath, ContentInfo contentInfo)
    {
        ContentFileCache? contentFileCache = null;
        Logger.PushFile(relativePath);
        try
        {
            contentFileCache = ProcessContent(relativePath, contentInfo, true).contentFileCache;
            _succeededToBuild++;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Countent failed to build:\n{ex}");
            _failedToBuild++;
        }
        Logger.PopFile();
        return contentFileCache;
    }

    /// <summary>
    /// Initiates a build of the specified asset and then loads the result into memory.
    /// </summary>
    /// <param name="relativePath">A relative path to the source asset.</param>
    /// <param name="contentInfo">The desired <see cref="ContentInfo"/> to be used for the content building.</param>
    /// <returns></returns>
    public (ContentFileCache? contentFileCache, object? processedObject) BuildAndLoadContent(string relativePath, ContentInfo contentInfo)
    {
        Logger.PushFile(relativePath);
        try
        {
            var content = ProcessContent(relativePath, contentInfo, false);
            _succeededToBuild++;
            return content;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Countent failed to build:\n{ex}");
            _failedToBuild++;
        }
        Logger.PopFile();
        return (null, null);
    }

    private (ContentFileCache? contentFileCache, object? processedObject) ProcessContent(string relativePath, ContentInfo contentInfo, bool writeToDisk)
    {
        ContentFileCache? contentFileCache = null;
        var filePath = Path.Combine(Parameters.RootedSourceDirectory, relativePath);
        var relativeDestPath = Path.Combine(contentInfo.ContentRoot, contentInfo.GetOutputPath(relativePath));
        var outputPath = Path.Combine(Parameters.RootedOutputDirectory, relativeDestPath);
        var outputDir = Path.GetDirectoryName(outputPath);

        if (string.IsNullOrWhiteSpace(outputDir))
        {
            return (contentFileCache, null);
        }

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        if (!contentInfo.ShouldBuild)
        {
            Logger.Log($"Output: {relativeDestPath}");
            contentFileCache = ContentCache.ReadContentFileCache(this, relativePath, contentInfo.ContentRoot);
            if (contentFileCache != null)
            {
                Logger.Log($"Cache: Found");
                return (contentFileCache, null);
            }
            Logger.Log($"Cache: Not Found");

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            File.Copy(filePath, outputPath);

            contentFileCache = new ContentFileCache
            {
                ContentRoot = contentInfo.ContentRoot
            };
            contentFileCache.AddDependency(this, relativePath);
            contentFileCache.AddOutputFile(this, outputPath);
            ContentCache.WriteContentFileCache(this, relativePath, contentFileCache);
            return (contentFileCache, null);
        }

        if (!ContentBuilderHelper.GetImporter(relativePath, contentInfo.Importer, out IContentImporter importer))
        {
            Logger.Log(LogLevel.Warning, "Importer: Not found :(");
            return (contentFileCache, null);
        }
        Logger.Log($"Imposter: {importer.GetType().Name}");
        if (!ContentBuilderHelper.GetProcessor(importer, contentInfo.Processor, out IContentProcessor processor))
        {
            Logger.Log(LogLevel.Warning, "Processor: Not found :(");
            return (contentFileCache, null);
        }
        Logger.Log($"Processor: {processor.GetType().Name}");
        Logger.Log($"Output: {relativeDestPath}");

        contentFileCache = ContentCache.ReadContentFileCache(this, relativePath, contentInfo.ContentRoot, true, importer, processor);
        if (contentFileCache != null)
        {
            Logger.Log($"Cache: Found");
            return (contentFileCache, null);
        }
        Logger.Log($"Cache: Not Found");

        contentFileCache = new ContentFileCache
        {
            ContentRoot = contentInfo.ContentRoot,
            CompressContent = Parameters.CompressContent,
            GraphicsProfile = Parameters.GraphicsProfile,
            ShouldBuild = true,
            Importer = importer,
            Processor = processor
        };
        contentFileCache.AddDependency(this, relativePath);
        contentFileCache.AddOutputFile(this, outputPath);

        var importContext = new ContentBuilderImporterContext(this, contentFileCache);
        var importedObject = importer.Import(filePath, importContext);

        var processorContext = new ContentBuilderProcessorContext(this, contentFileCache, contentInfo.ContentRoot, outputPath);
        var processedObject = processor.Process(importedObject, processorContext);

        if (writeToDisk)
        {
            var compiler = new ContentCompiler();
            using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            compiler.Compile(stream, processedObject, Parameters.Platform, Parameters.GraphicsProfile, Parameters.CompressContent, Parameters.RootedOutputDirectory, outputDir);
            ContentCache.WriteContentFileCache(this, relativePath, contentFileCache);
        }

        return (contentFileCache, processedObject);
    }

    private ContentFileCache? CheckContentCache(string relativePath, ContentInfo contentInfo)
    {
        if (!contentInfo.ShouldBuild)
        {
            return ContentCache.ReadContentFileCache(this, relativePath, contentInfo.ContentRoot);
        }

        if (!ContentBuilderHelper.GetImporter(relativePath, contentInfo.Importer, out IContentImporter importer) ||
            !ContentBuilderHelper.GetProcessor(importer, contentInfo.Processor, out IContentProcessor processor))
        {
            return null;
        }

        return ContentCache.ReadContentFileCache(this, relativePath, contentInfo.ContentRoot, true, importer, processor);
    }

    /// <summary>
    /// Runs the <see cref="ContentBuilder"/> with the specified parameters.
    /// </summary>
    /// <param name="parameters">A <see cref="ContentBuilderParams"/> describing both the platform paramteres for the content compilation as well as the configuration of the <see cref="ContentBuilder"/> itself.</param>
    public void Run(ContentBuilderParams parameters)
    {
        Parameters = parameters;
        if (parameters.Mode == ContentBuilderMode.None)
        {
            // This means we are just showing the help menu.
            return;
        }

        Directory.SetCurrentDirectory(Parameters.WorkingDirectory);

        Logger.IndentCharacter = ' ';
        Logger.IndentCharacterSize = 2;
        Logger.ShowRealTime = Parameters.Mode == ContentBuilderMode.Server;

        Logger.PushFile("Starting Content Builder");
        foreach (var prop in Parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetValue(Parameters) is IList list)
            {
                Logger.Log($"{prop.Name}:");
                foreach (var item in list)
                {
                    Logger.Log($"- {item}");
                }
            }
            else
            {
                Logger.Log($"{prop.Name}: {prop.GetValue(Parameters)}");
            }
        }
        Logger.PopFile();

        ContentCache.LoadCache(this);
        var contentCollection = GetContentCollection();
        ScanFiles(contentCollection, Parameters.RootedSourceDirectory);

        switch (Parameters.Mode)
        {
            case ContentBuilderMode.Builder:
                RunBuild();
                break;
            case ContentBuilderMode.Server:
                RunServer();
                break;
        }
    }

    /// <summary>
    /// A helper method to run the <see cref="ContentBuilder"/> with the passed <see cref="ContentBuilderParams"/> from the entry point args.
    /// </summary>
    /// <param name="args">An array of string to be deserialized into <see cref="ContentBuilderParams"/>.</param>
    public void Run(string[] args) => Run(ContentBuilderParams.Parse(args));

    private void ScanFiles(IContentCollection contentCollection, string directory)
    {
        foreach (var dir in Directory.GetDirectories(directory))
        {
            ScanFiles(contentCollection, dir);
        }

        foreach (var filePath in Directory.GetFiles(directory))
        {
            ContentInfo? contentInfo = null;
            var relativePath = Path.GetRelativePath(Parameters.RootedSourceDirectory, filePath);
            if (contentCollection.GetContentInfo(relativePath, ref contentInfo) && contentInfo != null)
            {
                _content[relativePath] = contentInfo;
                _outputContent[Path.Combine(contentInfo.ContentRoot, contentInfo.GetOutputPath(relativePath))] = relativePath;
            }
        }
    }

    private void RunBuild()
    {
        foreach (var pair in _content)
        {
            if (_content.TryGetValue(pair.Key, out ContentInfo? contentInfo))
            {
                BuildAndWriteContent(pair.Key, contentInfo);
            }
        }

        if (!Parameters.SkipClean)
        {
            ContentCache.CleanCache(this);
        }
        ContentCache.FlushCache(this);

        Logger.PushFile("Content Builder Finished");
        Logger.Log($"{_succeededToBuild} succeeded, {_failedToBuild} failed");
        Logger.PopFile();
    }

    private void RunServer()
    {
        Console.CancelKeyPress += delegate
        {
            foreach (var server in Parameters.Servers)
            {
                server.StopListening();
            }

            // We don't want to call CleanCache in server mode as we don't go through all the files!
            ContentCache.FlushCache(this);
        };

        foreach (var server in Parameters.Servers)
        {
            server.Logger = Logger;
            server.ContentRequested += ServerContentRequested;
            server.StartListening();
        }

        while (true)
        {
            ContentRequest? request = null;

            lock (_contentRequestQueue)
            {
                if (_contentRequestQueue.Count > 0)
                {
                    request = _contentRequestQueue.Dequeue();
                }
            }

            if (request != null)
            {
                BuildAndWriteContent(request.InputPath, request.ContentInfo);
                request.Args.FilePath = Path.Combine(Parameters.RootedOutputDirectory, Path.Combine(request.ContentInfo.ContentRoot, request.ContentInfo.GetOutputPath(request.InputPath)));
                request.Server.NotifyContentRequestCompiled();
            }
            else
            {
                lock (_contentRequestLock)
                {
                    Monitor.Wait(_contentRequestLock);
                }
            }
        }
    }

    private void ServerContentRequested(object? server, ContentRequestedArgs args)
    {
        if (server is not ContentServer contentServer)
        {
            return;
        }

        if (_outputContent.TryGetValue(args.ContentPath, out var inputPath))
        {
            if (_content.TryGetValue(inputPath, out ContentInfo? contentInfo))
            {
                if (CheckContentCache(inputPath, contentInfo) is not null)
                {
                    // we've already found a valid cached version of content, so no need for any compilation here
                    args.FilePath = Path.Combine(Parameters.RootedOutputDirectory, Path.Combine(contentInfo.ContentRoot, contentInfo.GetOutputPath(inputPath)));
                    return;
                }

                args.CompilationStarted = true;
                lock (_contentRequestQueue)
                {
                    _contentRequestQueue.Enqueue(new ContentRequest
                    {
                        InputPath = inputPath,
                        ContentInfo = contentInfo,
                        Server = contentServer,
                        Args = args
                    });
                    lock (_contentRequestLock)
                    {
                        Monitor.Pulse(_contentRequestLock);
                    }
                }
            }
        }
    }
}
