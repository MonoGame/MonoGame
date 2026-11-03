// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using MonoGame.Framework.Content.Pipeline.Builder.Logger;
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

    private class SkipLogException() : Exception;

    private sealed class InFlightBuild
    {
        public required int OwnerThreadId { get; init; }

        public TaskCompletionSource<BuildResult> Completion { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    private readonly record struct BuildResult(
        string RelativeSourcePath,
        int BuildHash,
        IContentFileCache? FileCache,
        bool Failed);

    internal static readonly StringComparer PathComparer =
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;

    private static readonly ImmutableHashSet<string> EmptyBuildStack =
        ImmutableHashSet<string>
            .Empty
            .WithComparer(PathComparer);

    private readonly Queue<ContentRequest> _contentRequestQueue = [];
    private readonly object _contentRequestLock = new();
    private readonly Dictionary<string, List<ContentInfo>> _content = [];

    /// <summary>
    /// Destination to source mapping.
    /// </summary>
    private readonly Dictionary<string, string> _outputContent = new(PathComparer);

    private readonly object _coordinatorLock = new();

    /// <summary>
    /// Resource to build mapping.
    /// </summary>
    private readonly Dictionary<string, InFlightBuild> _inFlightByPath = new(PathComparer);

    /// <summary>
    /// Build to resource mapping.
    /// </summary>
    private readonly Dictionary<int, string> _threadWaitingOnPath = [];

    /// <summary>
    /// Resource stack for the current build.
    /// </summary>
    private readonly AsyncLocal<ImmutableHashSet<string>> _activeBuildStack = new();

    /// <summary>
    /// Contains the build results in the current batch.
    /// </summary>
    /// <remarks>
    /// It should remain <see langword="null"/> in server mode.
    /// </remarks>
    private Dictionary<string, BuildResult>? _batchResultsInCurrentRun;

    private readonly Lazy<ContentCompiler> _compiler = new(static () => new ContentCompiler());

    private uint _failedToBuild;
    private uint _succeededToBuild;

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
    public ContentBuildLogger Logger { get; set; } = new ContentBuilderLogger();

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
    public uint FailedToBuild
    {
        get => Volatile.Read(ref _failedToBuild);
        private set => Interlocked.Exchange(ref _failedToBuild, value);
    }

    /// <summary>
    /// Returns the number of content items that built successfully.
    /// </summary>
    public uint SucceededToBuild
    {
        get => Volatile.Read(ref _succeededToBuild);
        private set => Interlocked.Exchange(ref _succeededToBuild, value);
    }

    /// <summary>
    /// Initiates a build of the specified asset and then writes down the result to disk..
    /// </summary>
    /// <param name="relativeSrcPath">A relative path to the source asset.</param>
    /// <param name="contentInfo">The desired <see cref="ContentInfo"/> to be used for the content building.</param>
    /// <param name="relativeDstPath">Optional name of the final compiled content.</param>
    /// <param name="parentContext">Set when building content dependencies by the ContentProcessorContext.</param>
    /// <returns>The complete name of the final compiled content including the content root.</returns>
    public string? BuildAndWriteContent(string relativeSrcPath, ContentInfo contentInfo, string? relativeDstPath = null, ContentProcessorContext? parentContext = null)
    {
        // If we get a absolute path to a source asset try
        // to make it relative as ProcessContent expects that.
        if (Path.IsPathRooted(relativeSrcPath))
        {
            if (parentContext != null)
            {
                var assetRoot = PathHelper.NormalizeDirectory(parentContext.ProjectDirectory);
                relativeSrcPath = PathHelper.GetRelativePath(assetRoot, relativeSrcPath);
            }
        }

        Logger.PushFile(Path.Combine(Parameters.RootedSourceDirectory, relativeSrcPath));
        try
        {
            ProcessContent(relativeSrcPath, contentInfo, true, ref relativeDstPath, parentContext);
            Interlocked.Increment(ref _succeededToBuild);

            return relativeDstPath;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _failedToBuild);
            if (ex is not SkipLogException)
            {
                Logger.Log(LogLevel.Error, $"Content failed to build: {ex}");
            }

            return parentContext != null ? throw new SkipLogException() : null;
        }
        finally
        {
            Logger.PopFile();
        }
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
        if (Path.IsPathRooted(relativeSrcPath))
        {
            if (parentContext != null)
            {
                var assetRoot = PathHelper.NormalizeDirectory(parentContext.ProjectDirectory);
                relativeSrcPath = PathHelper.GetRelativePath(assetRoot, relativeSrcPath);
            }
        }

        Logger.PushFile(Path.Combine(Parameters.RootedSourceDirectory, relativeSrcPath));
        try
        {
            var content = ProcessContent(relativeSrcPath, contentInfo, false, ref relativeDstPath, parentContext);
            Interlocked.Increment(ref _succeededToBuild);

            return content;
        }
        catch (Exception ex)
        {
            Interlocked.Increment(ref _failedToBuild);
            if (ex is not SkipLogException)
            {
                Logger.Log(LogLevel.Error, $"Content failed to build: {ex}");
            }
            if (parentContext != null)
            {
                throw new SkipLogException();
            }
        }
        finally
        {
            Logger.PopFile();
        }

        return null;
    }

    private object? ProcessContent(string relativeSrcPath,
        ContentInfo contentInfo,
        bool writeToDisk,
        ref string? relativeDstPath,
        ContentProcessorContext? parentContext)
    {
        if (string.IsNullOrEmpty(relativeDstPath))
        {
            relativeDstPath = relativeSrcPath.GetDestinationPath(contentInfo.ShouldBuild, contentInfo.GetOutputPath);
        }
        else if (contentInfo.ShouldBuild && !relativeDstPath.EndsWith(".xnb", StringComparison.OrdinalIgnoreCase))
        {
            relativeDstPath += ".xnb";
        }

        if (contentInfo.ShouldBuild) // ensure importer and processor are set
        {
            if (!ContentBuilderHelper.GetImporter(relativeSrcPath, contentInfo.Importer, out IContentImporter importer))
            {
                Logger.Log(LogLevel.Warning, "Importer: Not found");
                return null;
            }
            if (!ContentBuilderHelper.GetProcessor(importer, contentInfo.Processor, out IContentProcessor processor))
            {
                Logger.Log(LogLevel.Warning, "Processor: Not found");
                return null;
            }
            if (contentInfo.Importer != importer || contentInfo.Processor != processor)
            {
                contentInfo = new ContentInfo(contentInfo.ContentRoot, contentInfo.ShouldBuild, importer, processor, contentInfo.GetOutputPath);
            }
        }

        var relativeDestPath = Path.Combine(contentInfo.ContentRoot, relativeDstPath).Sanitize();
        var buildHash = contentInfo.ShouldBuild
            ? contentInfo.MakeBuildHash()
            : 0;

        // Dependency content that is imported/processed gets a
        // hash code appended to the file name to avoid conflicts.
        if (parentContext != null && contentInfo.ShouldBuild)
        {
            relativeDestPath = $"{relativeDestPath[..^4]}.{buildHash:x}.xnb";
        }

        // Return the caller the final completed content path.
        relativeDstPath = relativeDestPath;

        var previousBuildStack = _activeBuildStack.Value ?? EmptyBuildStack;
        var buildStackKey = writeToDisk ? relativeDestPath : $"{relativeSrcPath}#{buildHash:x}";
        if (previousBuildStack.Contains(relativeDestPath) || previousBuildStack.Contains(buildStackKey))
        {
            throw new PipelineException($"Circular content dependency detected while building '{relativeDestPath}'.");
        }

        if (!writeToDisk)
        {
            _activeBuildStack.Value = previousBuildStack.Add(buildStackKey);
            try
            {
                return ExecuteBuild(relativeSrcPath, contentInfo, writeToDisk: false, relativeDestPath, parentContext, out _);
            }
            finally
            {
                _activeBuildStack.Value = previousBuildStack;
            }
        }

        RunSingleFlight(relativeSrcPath, contentInfo, relativeDestPath, buildHash, previousBuildStack, parentContext);

        return null;
    }

    private object? ExecuteBuild(
        string relativeSrcPath,
        ContentInfo contentInfo,
        bool writeToDisk,
        string relativeDstPath,
        ContentProcessorContext? parentContext,
        out IContentFileCache? resultingFileCache)
    {
        resultingFileCache = null;

        var filePath = Path.Combine(Parameters.RootedSourceDirectory, relativeSrcPath);
        var outputPath = Path.Combine(Parameters.RootedOutputDirectory, relativeDstPath).Sanitize();
        var outputDir = Path.GetDirectoryName(outputPath);

        if (string.IsNullOrWhiteSpace(outputDir))
            return null;

        if (!Directory.Exists(outputDir))
            Directory.CreateDirectory(outputDir);

        if (writeToDisk)
        {
            var fileCache = ContentCache.ReadContentFileCache(this, relativeDstPath);
            if (fileCache != null && fileCache.IsValid(this, contentInfo))
            {
                Logger.Log(LogLevel.Debug, $"Cache: Found");
                ContentCache.MarkUsed(fileCache);
                (parentContext as ContentBuilderProcessorContext)?.ContentFileCache.AddDependency(this, fileCache);
                resultingFileCache = fileCache;

                return null;
            }
        }

        if (!contentInfo.ShouldBuild)
        {
            Logger.Log(Path.GetRelativePath(Logger.LoggerRootDirectory, outputPath).Sanitize());
            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }
            File.Copy(filePath, outputPath);

            var fileCopyCache = ContentCache.CreateContentFileCache(this, contentInfo);
            fileCopyCache.AddDependency(this, relativeSrcPath);
            fileCopyCache.AddOutputFile(this, outputPath);
            ContentCache.WriteContentFileCache(this, relativeDstPath, fileCopyCache);
            ContentCache.MarkUsed(fileCopyCache);
            (parentContext as ContentBuilderProcessorContext)?.ContentFileCache.AddDependency(this, fileCopyCache);
            resultingFileCache = fileCopyCache;

            return null;
        }

        Logger.Log(LogLevel.Debug, $"Cache: Not Found");
        Logger.Log(LogLevel.Debug, $"Importer: {contentInfo.Importer!.GetType().Name}");
        Logger.Log(LogLevel.Debug, $"Processor: {contentInfo.Processor!.GetType().Name}");
        Logger.Log(Path.GetRelativePath(Logger.LoggerRootDirectory, outputPath).Sanitize());

        var contentFileCache = ContentCache.CreateContentFileCache(this, contentInfo);
        contentFileCache.AddDependency(this, relativeSrcPath);

        var importContext = new ContentBuilderImporterContext(this, contentFileCache);
        var importedObject = contentInfo.Importer!.Import(filePath, importContext);

        var processorContext = new ContentBuilderProcessorContext(this, relativeSrcPath, contentInfo, contentFileCache, outputPath);
        using var _ = ContextScopeFactory.BeginContext(processorContext);
        var processedObject = contentInfo.Processor!.Process(importedObject, processorContext);

        if (writeToDisk)
        {
            using var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
            _compiler.Value.Compile(stream, processedObject, Parameters.Platform, Parameters.GraphicsProfile, Parameters.CompressContent, Parameters.RootedOutputDirectory, outputDir);
            contentFileCache.AddOutputFile(this, outputPath);
            ContentCache.WriteContentFileCache(this, relativeDstPath, contentFileCache);
            ContentCache.MarkUsed(contentFileCache);
        }

        (parentContext as ContentBuilderProcessorContext)?.ContentFileCache.AddDependency(this, contentFileCache);
        resultingFileCache = contentFileCache;

        return processedObject;
    }

    private void RunSingleFlight(
        string relativeSrcPath,
        ContentInfo contentInfo,
        string relativeDstPath,
        int buildHash,
        ImmutableHashSet<string> previousBuildStack,
        ContentProcessorContext? parentContext)
    {
        var currentThreadId = Environment.CurrentManagedThreadId;
        InFlightBuild? ownedBuild = null;
        InFlightBuild? existingEntry = null;
        BuildResult? cachedBatchResult = null;

        lock (_coordinatorLock)
        {
            if (_batchResultsInCurrentRun != null
                && _batchResultsInCurrentRun.TryGetValue(relativeDstPath, out var completedBatchResult))
            {
                cachedBatchResult = completedBatchResult;
            }
            else if (_inFlightByPath.TryGetValue(relativeDstPath, out existingEntry))
            {
                if (IsCircularWait(currentThreadId, existingEntry.OwnerThreadId, previousBuildStack))
                {
                    throw new PipelineException($"Circular content dependency detected while building '{relativeDstPath}'!");
                }

                _threadWaitingOnPath[currentThreadId] = relativeDstPath;
            }
            else
            {
                ownedBuild = new InFlightBuild
                {
                    OwnerThreadId = currentThreadId,
                };

                _inFlightByPath[relativeDstPath] = ownedBuild;
            }
        }

        if (cachedBatchResult is { } batchResult)
        {
            HandleSharedBuildResult(batchResult, relativeSrcPath, relativeDstPath, buildHash, parentContext);

            return;
        }

        if (existingEntry != null)
        {
            BuildResult sharedResult;
            try
            {
                sharedResult = existingEntry
                    .Completion
                    .Task
                    .GetAwaiter()
                    .GetResult();
            }
            finally
            {
                lock (_coordinatorLock)
                {
                    _threadWaitingOnPath.Remove(currentThreadId);
                }
            }

            HandleSharedBuildResult(sharedResult, relativeSrcPath, relativeDstPath, buildHash, parentContext);

            return;
        }

        Debug.Assert(ownedBuild != null);

        IContentFileCache? producedFileCache = null;
        var failed = true;
        _activeBuildStack.Value = previousBuildStack.Add(relativeDstPath);
        try
        {
            ExecuteBuild(relativeSrcPath, contentInfo, writeToDisk: true, relativeDstPath, parentContext, out producedFileCache);
            failed = false;
        }
        finally
        {
            _activeBuildStack.Value = previousBuildStack;
            var finalResult = new BuildResult(relativeSrcPath, buildHash, producedFileCache, failed);
            lock (_coordinatorLock)
            {
                if (_batchResultsInCurrentRun != null)
                {
                    _batchResultsInCurrentRun[relativeDstPath] = finalResult;
                }

                _inFlightByPath.Remove(relativeDstPath);
            }

            ownedBuild.Completion.SetResult(finalResult);
        }
    }

    private void HandleSharedBuildResult(
        BuildResult sharedResult,
        string relativeSrcPath,
        string relativeDstPath,
        int buildHash,
        ContentProcessorContext? parentContext)
    {
        // A different source or different build settings targeting the same output would overwrite each other's .xnb.
        // Checked first so a failed first writer won't hide the conflict.
        if (!PathComparer.Equals(sharedResult.RelativeSourcePath, relativeSrcPath)
            || sharedResult.BuildHash != buildHash)
        {
            throw new PipelineException($"Content collision: '{relativeSrcPath}' and '{sharedResult.RelativeSourcePath}' both build to '{relativeDstPath}'.");
        }

        if (sharedResult.Failed)
        {
            throw new SkipLogException();
        }

        if (sharedResult.FileCache != null)
        {
            ContentCache.MarkUsed(sharedResult.FileCache);
            (parentContext as ContentBuilderProcessorContext)?.ContentFileCache.AddDependency(this, sharedResult.FileCache);
        }
    }

    private bool IsCircularWait(int currentThreadId, int existingOwnerThreadId, ImmutableHashSet<string> activeBuildStack)
    {
        if (currentThreadId == existingOwnerThreadId)
        {
            return true;
        }

        // Follow the chain of waiting threads:
        // Owner thread -> waited path -> in-flight thread -> waited path -> ...
        var inFlightThreadId = existingOwnerThreadId;
        while (_threadWaitingOnPath.TryGetValue(inFlightThreadId, out var waitedPath)
            && _inFlightByPath.TryGetValue(waitedPath, out var inFlightEntry))
        {
            // Check if the thread we want to wait on is waiting on us in return.
            // Cycle found if the chain leads back to currentThreadId
            if (currentThreadId == inFlightEntry.OwnerThreadId
                // Or to a dependency on current active build stack.
                // This is to prevent missing dependencies created by using something like Task.Run(), etc.
                || activeBuildStack.Contains(waitedPath))
            {
                Debug.WriteLine($"Circular dependency detected! Current Thread: {currentThreadId}, In-flight Thread: {inFlightEntry.OwnerThreadId}, Waited Path: {waitedPath}");

                return true;
            }

            inFlightThreadId = inFlightEntry.OwnerThreadId;
        }

        return false;
    }

    /// <summary>
    /// Runs the <see cref="ContentBuilder"/> with the specified parameters.
    /// </summary>
    /// <param name="parameters">A <see cref="ContentBuilderParams"/> describing both the platform paramteres for the content compilation as well as the configuration of the <see cref="ContentBuilder"/> itself.</param>
    public bool Run(ContentBuilderParams parameters)
    {
        ContentBuilderHelper.LoadAssemblies();

        Parameters = parameters;
        Logger.LoggerLogLevel = Parameters.LogLevel;
        Logger.LoggerRootDirectory = Parameters.WorkingDirectory;

        if (parameters.Mode == ContentBuilderMode.None)
        {
            // This means we are just showing the help menu.
            return false;
        }

        Directory.SetCurrentDirectory(Parameters.WorkingDirectory);

        Logger.Log("Starting Content Builder");
        Logger.Indent();
        foreach (var prop in Parameters.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetValue(Parameters) is IList list)
            {
                Logger.Log(LogLevel.Debug, $"{prop.Name}:");
                foreach (var item in list)
                {
                    Logger.Log(LogLevel.Debug, $"- {item}");
                }
            }
            else
            {
                Logger.Log(LogLevel.Debug, $"{prop.Name}: {prop.GetValue(Parameters)}");
            }
        }
        Logger.Unindent();

        ContentCache.LoadCache(this);

        // If we're rebuilding then clear all previously
        // built content which will force a rebuild.
        if (Parameters.Rebuild)
            ContentCache.CleanCache(this);

        FailedToBuild = 0;
        SucceededToBuild = 0;
        _content.Clear();
        _outputContent.Clear();

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
            if (Path.GetFileName(dir).StartsWith('.'))
                continue;

            ScanFiles(contentCollection, dir);
        }

        foreach (var filePath in Directory.GetFiles(directory))
        {
            if (Path.GetFileName(filePath).StartsWith('.'))
                continue;

            var relativeSrcPath = Path.GetRelativePath(Parameters.RootedSourceDirectory, filePath);
            relativeSrcPath = relativeSrcPath.Sanitize();
            if (contentCollection.GetContentInfo(relativeSrcPath, out List<ContentInfo> contentInfos) && contentInfos.Count > 0)
            {
                List<ContentInfo> validContentInfos = [];
                foreach (var contentInfo in contentInfos)
                {
                    var outputPath = Path.Combine(contentInfo.ContentRoot,
                        relativeSrcPath.GetDestinationPath(contentInfo.ShouldBuild, contentInfo.GetOutputPath)).Sanitize();

                    if (_outputContent.TryGetValue(outputPath, out var existingSrcPath))
                    {
                        Logger.PushFile(filePath);
                        Logger.Log(LogLevel.Error, $"Content collision: '{relativeSrcPath}' and '{existingSrcPath}' both build to '{outputPath}'.");
                        Logger.PopFile();

                        Interlocked.Increment(ref _failedToBuild);

                        continue;
                    }

                    _outputContent[outputPath] = relativeSrcPath;
                    validContentInfos.Add(contentInfo);
                }

                if (validContentInfos.Count > 0)
                {
                    _content[relativeSrcPath] = validContentInfos;
                }
            }
        }
    }

    private bool RunBuild()
    {
        _batchResultsInCurrentRun = new Dictionary<string, BuildResult>(PathComparer);

        var contentToBuild = new List<(string RelativeSrcPath, ContentInfo ContentInfo)>();
        foreach (var pair in _content)
        {
            foreach (var contentInfo in pair.Value)
            {
                contentToBuild.Add((pair.Key, contentInfo));
            }
        }

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Parameters.MaxDegreeOfParallelism,
        };
        try
        {
            Parallel.ForEach(contentToBuild, parallelOptions, item =>
            {
                BuildAndWriteContent(item.RelativeSrcPath, item.ContentInfo);
            });
        }
        finally
        {
            _batchResultsInCurrentRun = null;
        }

        if (!Parameters.SkipClean && FailedToBuild == 0)
        {
            ContentCache.CleanCache(this);
        }
        ContentCache.FlushCache(this);

        Logger.Log("Content Builder Finished");
        Logger.Indent();
        Logger.Log($"{SucceededToBuild} succeeded, {FailedToBuild} failed");
        Logger.Unindent();

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
                request.Args.FilePath = Path.Combine(Parameters.RootedOutputDirectory, Path.Combine(request.ContentInfo.ContentRoot, request.InputPath.GetDestinationPath(request.ContentInfo.ShouldBuild, request.ContentInfo.GetOutputPath))).Sanitize();
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
                    var finalOutputPath = Path.Combine(contentInfo.ContentRoot, inputPath.GetDestinationPath(contentInfo.ShouldBuild, contentInfo.GetOutputPath)).Sanitize();
                    if (!PathComparer.Equals(finalOutputPath, outputPath))
                    {
                        continue;
                    }

                    if (ContentCache.ReadContentFileCache(this, outputPath) is ContentFileCache fileCache && fileCache.IsValid(this, contentInfo))
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
