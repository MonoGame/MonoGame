// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Framework.Content.Pipeline.Builder;

class ContentBuilderProcessorContext(ContentBuilder builder, string relativePath, ContentInfo contentInfo, IContentFileCache contentFileCache, string outputFilename = "") : ContentProcessorContext
{
    private int _contentIndex = 0;

    public IContentFileCache ContentFileCache { get; } = contentFileCache;

    public override string BuildConfiguration { get; } = "";

    public override string IntermediateDirectory => builder.Parameters.RootedIntermediateDirectory;

    public override ContentBuildLogger Logger => builder.Logger;

    public override ContentIdentity SourceIdentity => new(sourceFilename: relativePath);

    public override string OutputDirectory => builder.Parameters.RootedOutputDirectory;

    public override string OutputFilename { get; } = outputFilename;

    public override OpaqueDataDictionary Parameters { get; } = [];

    public override string ProjectDirectory => builder.Parameters.RootedSourceDirectory;

    public override TargetPlatform TargetPlatform => builder.Parameters.Platform;

    public override GraphicsProfile TargetProfile => builder.Parameters.GraphicsProfile;

    public override void AddDependency(string filename) => ContentFileCache.AddDependency(builder, filename);

    public override void AddOutputFile(string filename) => ContentFileCache.AddOutputFile(builder, filename);

    public string GetNextOutputPath()
    {
        _contentIndex++;
        return $"{relativePath.GetDestinationPath(true, contentInfo.GetOutputPath)[0..^4]}_{_contentIndex}.xnb";
    }

    [Obsolete]
    public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        string processorName, OpaqueDataDictionary? processorParameters, string? importerName)
    {
        throw new NotSupportedException("""
            Converting from importerName and processorName is not supported with the ContentBuilder.
            Please pass an importer and processor instance to the Convert method instead.
            """);
    }

    public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, IContentImporter importer, IContentProcessor processor)
    {
        var processedObject = builder.BuildAndLoadContent(sourceAsset.Filename, new ContentInfo(contentInfo.ContentRoot, true, importer, processor), GetNextOutputPath(), this);
        return (TOutput)processedObject!;
    }

    [Obsolete]
    public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        string processorName, OpaqueDataDictionary? processorParameters, string? importerName, string? assetName)
    {
        throw new NotSupportedException("""
            Converting from imposterName and processorName is not supported with the ContentBuilder.
            Please pass an importer and processor instance to the Convert method instead.
            """);
    }

    public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset,
        IContentImporter importer, IContentProcessor processor, string? assetName = null)
    {
        var outputRelativePath = builder.BuildAndWriteContent(sourceAsset.Filename, new ContentInfo(contentInfo.ContentRoot, true, importer, processor), assetName, this);
        return string.IsNullOrEmpty(outputRelativePath) ?
            throw new Exception("This exception should never be reached, if it happens, there is an error in BuildAndWriteContent failing to write content and not calling an exception due to it.") :
            new ExternalReference<TOutput>(Path.Combine(builder.Parameters.RootedOutputDirectory, outputRelativePath));
    }

    [Obsolete]
    public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
    {
        throw new NotSupportedException("""
            Converting from processorName is not supported with the ContentBuilder.
            Please pass a processor instance to the Convert method instead.
            """);
    }

    public override TOutput Convert<TInput, TOutput>(TInput input, IContentProcessor processor)
    {
        var processContext = new ContentBuilderProcessorContext(builder, relativePath, contentInfo, ContentFileCache);
        using var _ = ContextScopeFactory.BeginContext(processContext);
        var processedObject = processor.Process(input!, processContext);
        return (TOutput)processedObject;
    }
}
