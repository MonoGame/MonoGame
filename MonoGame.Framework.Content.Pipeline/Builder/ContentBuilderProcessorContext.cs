// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Framework.Content.Pipeline.Builder;

class ContentBuilderProcessorContext(ContentBuilder builder, string relativePath, ContentInfo contentInfo, IContentFileCache contentFileCache, string outputFilename = "") : ContentProcessorContext
{
    private readonly ContentBuilder _builder = builder;

    private readonly string _relativeContentPath = relativePath;

    private readonly ContentInfo _contentInfo = contentInfo;

    private readonly IContentFileCache _contentFileCache = contentFileCache;

    private int _contentIndex = 0;

    private readonly ContentBuilderProcessorContext? _parentContext = ContextScopeFactory.HasActiveContext ? ContextScopeFactory.ActiveContext as ContentBuilderProcessorContext : null;

    public override string BuildConfiguration { get; } = "";

    public override string IntermediateDirectory => _builder.Parameters.RootedIntermediateDirectory;

    public override ContentBuildLogger Logger => _builder.Logger;

    public override ContentIdentity SourceIdentity => throw new NotImplementedException();

    public override string OutputDirectory => _builder.Parameters.OutputDirectory;

    public override string OutputFilename { get; } = outputFilename;

    public override OpaqueDataDictionary Parameters { get; } = [];

    public override string ProjectDirectory => _builder.Parameters.RootedSourceDirectory;

    public override TargetPlatform TargetPlatform => _builder.Parameters.Platform;

    public override GraphicsProfile TargetProfile => _builder.Parameters.GraphicsProfile;

    public override void AddDependency(string filename)
    {
        if (_parentContext != null)
        {
            _parentContext.AddDependency(filename);
            return;
        }

        _contentFileCache.AddDependency(_builder, filename);
    }

    public override void AddOutputFile(string filename)
    {
        if (_parentContext != null)
        {
            _parentContext.AddOutputFile(filename);
            return;
        }

        _contentFileCache.AddOutputFile(_builder, filename);
    }

    public string GetNextOutputPath()
    {
        if (_parentContext != null)
        {
            return _parentContext.GetNextOutputPath();
        }

        _contentIndex++;
        return $"{_relativeContentPath.GetDestinationPath(true, _contentInfo.GetOutputPath)[0..^4]}_{_contentIndex}.xnb";
    }

    [Obsolete]
    public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        string processorName, OpaqueDataDictionary processorParameters, string importerName)
    {
        throw new NotSupportedException(
            @"Converting from importerName and processorName is not supported with the ContentBuilder.
            Please pass an importer and processor instance to the Convert method instead.");
    }

    public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, IContentImporter importer, IContentProcessor processor)
    {
        var processedObject = _builder.BuildAndLoadContent(sourceAsset.Filename, new ContentInfo(_contentInfo.ContentRoot, true, importer, processor), GetNextOutputPath());
        return (TOutput)processedObject!;
    }

    [Obsolete]
    public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
    {
        throw new NotSupportedException(
            @"Converting from imposterName and processorName is not supported with the ContentBuilder.
            Please pass an importer and processor instance to the Convert method instead.");
    }

    public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        IContentImporter importer, IContentProcessor processor, string? assetName)
    {
        var outputRelativePath = string.IsNullOrWhiteSpace(assetName) ? GetNextOutputPath() : assetName;
        _builder.BuildAndWriteContent(sourceAsset.Filename, new ContentInfo(_contentInfo.ContentRoot, true, importer, processor), outputRelativePath);

        return new ExternalReference<TOutput>(Path.Combine(_builder.Parameters.RootedOutputDirectory, outputRelativePath));
    }

    [Obsolete]
    public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
    {
        throw new NotSupportedException(@"Converting from processorName is not supported with the ContentBuilder.
            Please pass a processor instance to the Convert method instead.");
    }

    public override TOutput Convert<TInput, TOutput>(TInput input, IContentProcessor processor)
    {
        var processContext = new ContentBuilderProcessorContext(_builder, _relativeContentPath, _contentInfo, _contentFileCache);
        using var _ = ContextScopeFactory.BeginContext(processContext);
        var processedObject = processor.Process(input!, processContext);

        return (TOutput)processedObject;
    }
}
