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
    private readonly Dictionary<string, List<ContentInfo>> _content = [];
    private readonly Dictionary<string, string> _outputContent = [];

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
    /// Returns the number of content items that failed to build.
    /// </summary>
    public uint FailedToBuild { get; private set; }

    /// <summary>
    /// Returns the number of content items that built successfully.
    /// </summary>
    public uint SucceededToBuild { get; private set; }

    /// <summary>
    /// Initiates a build of the specified asset and then writes down the result to disk..
    /// </summary>
    /// <param name="relativeSrcPath">A relative path to the source asset.</param>
    /// <param name="contentInfo">The desired <see cref="ContentInfo"/> to be used for the content building.</param>
    /// <param name="relativeDstPath">The desired relative output path.</param>
    /// <param name="parentContext">Only set when the method is being called by the ContentProcessorContext to build one of its children.</param>
    public void BuildAndWriteContent(string relativeSrcPath, ContentInfo contentInfo, string? relativeDstPath = null, ContentProcessorContext? parentContext = null)
    {
        Logger.PushFile(relativeSrcPath);
        try
        {
            ProcessContent(relativeSrcPath, contentInfo, true, relativeDstPath, parentContext);
            SucceededToBuild++;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Countent failed to build:\n{ex}");
            FailedToBuild++;
        }
        Logger.PopFile();
    }

    /// <summary>
    /// Initiates a build of the specified asset and then loads the result into memory.
    /// </summary>
    /// <param name="relativeSrcPath">A relative path to the source asset.</param>
    /// <param name="contentInfo">The desired <see cref="ContentInfo"/> to be used for the content building.</param>
    /// <param name="relativeDstPath">The desired relative output path.</param>
    /// <param name="parentContext">Only set when the method is being called by the ContentProcessorContext to build one of its children.</param>
    /// <returns>The built object that the <see cref="IContentProcessor.Process(object, ContentProcessorContext)"/> returned.</returns>
    public object? BuildAndLoadContent(string relativeSrcPath, ContentInfo contentInfo, string? relativeDstPath = null, ContentProcessorContext? parentContext = null)
    {
        Logger.PushFile(relativeSrcPath);
        try
        {
            var content = ProcessContent(relativeSrcPath, contentInfo, false, relativeDstPath, parentContext);
            SucceededToBuild++;
            return content;
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, $"Countent failed to build:\n{ex}");
            FailedToBuild++;
        }
        Logger.PopFile();
        return null;
    }

    private object? ProcessContent(string relativePath, ContentInfo contentInfo, bool writeToDisk, string? relativeOutputPath, ContentProcessorContext? parentContext)
    {
        var filePath = Path.Combine(Parameters.RootedSourceDirectory, relativePath);
        var relativeDestPath = Path.Combine(contentInfo.ContentRoot, string.IsNullOrEmpty(relativeOutputPath) ? relativePath.GetDestinationPath(contentInfo.ShouldBuild, contentInfo.GetOutputPath) : relativeOutputPath);
        var outputPath = Path.Combine(Parameters.RootedOutputDirectory, relativeDestPath);
        var outputDir = Path.GetDirectoryName(outputPath);

        if (string.IsNullOrWhiteSpace(outputDir))
        {
            return null;
        }

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        Logger.Log($"Output: {relativeDestPath}");
        if (!Parameters.Rebuild)
        {
            var fileCache = ContentCache.ReadContentFileCache(this, relativeDestPath);
            if (fileCache != null && fileCache.IsValid(this, contentInfo))
            {
                Logger.Log($"Cache: Found");
                ContentCache.MarkUsed(fileCache);
                return null;
            }
        }

        if (!contentInfo.ShouldBuild)
        {
            Logger.Log($"Cache: Not Found");

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            File.Copy(filePath, outputPath);

            var fileCache = ContentCache.CreateContentFileCache(this, contentInfo);
            fileCache.AddDependency(this, relativePath);
            fileCache.AddOutputFile(this, outputPath);
            ContentCache.WriteContentFileCache(this, relativeDestPath, fileCache);
            ContentCache.MarkUsed(fileCache);
            (parentContext as ContentBuilderProcessorContext)?.ContentFileCache.AddDependency(this, fileCache);
            return null;
        }

        if (!ContentBuilderHelper.GetImporter(relativePath, contentInfo.Importer, out IContentImporter importer))
        {
            Logger.Log(LogLevel.Warning, "Importer: Not found :(");
            return null;
        }
        Logger.Log($"Imposter: {importer.GetType().Name}");
        if (!ContentBuilderHelper.GetProcessor(importer, contentInfo.Processor, out IContentProcessor processor))
        {
            Logger.Log(LogLevel.Warning, "Processor: Not found :(");
            return null;
        }
        Logger.Log($"Processor: {processor.GetType().Name}");
        Logger.Log($"Cache: Not Found");

        var contentFileCache = ContentCache.CreateContentFileCache(this, contentInfo);
        contentFileCache.AddDependency(this, relativePath);

        var importContext = new ContentBuilderImporterContext(this, contentFileCache);
        var importedObject = importer.Import(filePath, importContext);

        var processorContext = new ContentBuilderProcessorContext(this, relativePath, contentInfo, contentFileCache, outputPath);
        var processedObject = processor.Process(importedObject, processorContext);

        if (writeToDisk)
        {
            var compiler = new ContentCompiler();
            using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            compiler.Compile(stream, processedObject, Parameters.Platform, Parameters.GraphicsProfile, Parameters.CompressContent, Parameters.RootedOutputDirectory, outputDir);
            contentFileCache.AddOutputFile(this, outputPath);
            ContentCache.WriteContentFileCache(this, relativeDestPath, contentFileCache);
            ContentCache.MarkUsed(contentFileCache);
            (parentContext as ContentBuilderProcessorContext)?.ContentFileCache.AddDependency(this, contentFileCache);
        }

        return processedObject;
    }

    /// <summary>
    /// Runs the <see cref="ContentBuilder"/> with the specified parameters.
    /// </summary>
    /// <param name="parameters">A <see cref="ContentBuilderParams"/> describing both the platform paramteres for the content compilation as well as the configuration of the <see cref="ContentBuilder"/> itself.</param>
    public bool Run(ContentBuilderParams parameters)
    {
        Parameters = parameters;
        if (parameters.Mode == ContentBuilderMode.None)
        {
            // This means we are just showing the help menu.
            return false;
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
                return RunBuild();
            case ContentBuilderMode.Server:
                RunServer();
                break;
        }

        return true;
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
            var relativePath = Path.GetRelativePath(Parameters.RootedSourceDirectory, filePath);
            relativePath = relativePath.Sanitize();
            if (contentCollection.GetContentInfo(relativePath, out List<ContentInfo> contentInfos) && contentInfos.Count > 0)
            {
                _content[relativePath] = contentInfos;
                foreach (var contentInfo in contentInfos)
                {
                    _outputContent[Path.Combine(contentInfo.ContentRoot, contentInfo.GetOutputPath(relativePath))] = relativePath;
                }
            }
        }
    }

    private bool RunBuild()
    {
        foreach (var pair in _content)
        {
            foreach(var contentInfo in pair.Value)
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
        Logger.Log($"{SucceededToBuild} succeeded, {FailedToBuild} failed");
        Logger.PopFile();

        return FailedToBuild == 0;
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

        var outputPath = args.ContentPath.Sanitize();
        if (_outputContent.TryGetValue(outputPath, out var inputPath))
        {
            if (_content.TryGetValue(inputPath, out List<ContentInfo>? contentInfos))
            {
                foreach (var contentInfo in contentInfos)
                {
                    if (contentInfo.GetOutputPath(inputPath) != outputPath)
                    {
                        continue;
                    }

                    if (ContentCache.ReadContentFileCache(this, outputPath) is not null)
                    {
                        // we've already found a valid cached version of content, so no need for any compilation here
                        args.FilePath = Path.Combine(Parameters.RootedOutputDirectory, outputPath);
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
}
