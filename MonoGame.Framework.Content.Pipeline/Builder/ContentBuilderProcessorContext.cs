// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Framework.Content.Pipeline.Builder;

class ContentBuilderProcessorContext(ContentBuilder builder, ContentFileCache contentFileCache, string outputRoot = "", string outputFilename = "") : ContentProcessorContext
{
    private readonly ContentBuilder _builder = builder;

    private readonly ContentFileCache _contentFileCache = contentFileCache;

    private readonly string _outputRoot = outputRoot;

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

    public override void AddDependency(string filename) => _contentFileCache.AddDependency(_builder, filename);

    public override void AddOutputFile(string filename) => _contentFileCache.AddOutputFile(_builder, filename);

    public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        string processorName, OpaqueDataDictionary processorParameters, string importerName)
    {
        throw new NotSupportedException(
            @"Converting from imposterName and processorName is not supported with the ContentBuilder.
            Please pass an importer and processor instance to the Convert method instead.");
    }

    public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, IContentImporter importer, IContentProcessor processor)
    {
        var content = _builder.BuildAndLoadContent(sourceAsset.Filename, new ContentInfo(_outputRoot, true, importer, processor));
        if (content.contentFileCache is ContentFileCache contentFileCache)
        {
            // Add its dependencies and built assets to ours.
            foreach (var (dependencyFile, _) in contentFileCache.Dependencies)
            {
                AddDependency(dependencyFile);
            }

            foreach (var outputFile in contentFileCache.Outputs)
            {
                AddOutputFile(outputFile);
            }
        }

        return (TOutput)content.processedObject!;
    }

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
        var outputRelativePath = string.IsNullOrWhiteSpace(assetName) ?
            ContentInfo.GetDefaultOutputPath(Path.GetRelativePath(_builder.Parameters.RootedSourceDirectory, sourceAsset.Filename)) :
            assetName;
        var content = _builder.BuildAndWriteContent(sourceAsset.Filename, new ContentInfo(_outputRoot, true, importer, processor, o => outputRelativePath));

        if (content is ContentFileCache contentFileCache)
        {
            // Add its dependencies and built assets to ours.
            foreach (var (dependencyFile, _) in contentFileCache.Dependencies)
            {
                AddDependency(dependencyFile);
            }

            foreach (var outputFile in contentFileCache.Outputs)
            {
                AddOutputFile(outputFile);
            }
        }

        return new ExternalReference<TOutput>(Path.Combine(_builder.Parameters.RootedOutputDirectory, outputRelativePath));
    }

    public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
    {
        throw new NotSupportedException(@"Converting from processorName is not supported with the ContentBuilder.
            Please pass a processor instance to the Convert method instead.");
    }

    public override TOutput Convert<TInput, TOutput>(TInput input, IContentProcessor processor)
    {
        var contentFileCache = new ContentFileCache();
        var processContext = new ContentBuilderProcessorContext(_builder, contentFileCache, _outputRoot);
        using var _ = ContextScopeFactory.BeginContext(processContext);
        var processedObject = processor.Process(input!, processContext);

        // Add its dependencies and built assets to ours.
        foreach (var (dependencyFile, _) in processContext._contentFileCache.Dependencies)
        {
            AddDependency(dependencyFile);
        }

        foreach (var outputFile in processContext._contentFileCache.Outputs)
        {
            AddOutputFile(outputFile);
        }

        return (TOutput)processedObject;
    }
}
