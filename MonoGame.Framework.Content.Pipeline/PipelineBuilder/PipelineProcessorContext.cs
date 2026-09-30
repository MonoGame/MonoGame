// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    /// <inheritdoc/>
    /// <summary>
    /// Creates a new pipeline processor context.
    /// </summary>
    /// <param name="manager">Pipeline manager.</param>
    /// <param name="pipelineEvent">Pipeline event.</param>
    public class PipelineProcessorContext(PipelineManager manager, PipelineBuildEvent pipelineEvent) : ContentProcessorContext
    {
        private readonly PipelineManager _manager = manager;

        private readonly PipelineBuildEvent _pipelineEvent = pipelineEvent;

        /// <inheritdoc/>
        public override TargetPlatform TargetPlatform => _manager.Platform;

        /// <inheritdoc/>
        public override GraphicsProfile TargetProfile => _manager.Profile;

        /// <inheritdoc/>
        public override string BuildConfiguration => _manager.Config;

        /// <inheritdoc/>
        public override string IntermediateDirectory => _manager.IntermediateDirectory;

        /// <inheritdoc/>
        public override string OutputDirectory => _manager.OutputDirectory;

        /// <inheritdoc/>
        public override string OutputFilename => _pipelineEvent.DestFile;

        /// <inheritdoc/>
        public override OpaqueDataDictionary Parameters => _pipelineEvent.Parameters;

        /// <inheritdoc/>
        public override string ProjectDirectory => _manager.ProjectDirectory;

        /// <inheritdoc/>
        public override ContentBuildLogger Logger => _manager.Logger;

        /// <inheritdoc/>
        public override ContentIdentity SourceIdentity => new(_pipelineEvent.SourceFile);

        /// <inheritdoc/>
        public override void AddDependency(string filename) => _pipelineEvent.Dependencies.AddUnique(filename);

        /// <inheritdoc/>
        public override void AddOutputFile(string filename)
        {
            _pipelineEvent.BuildOutput.AddUnique(filename);
        }

        /// <inheritdoc/>
        [Obsolete("Please pass importer and processor as instances instead of just their names.")]
        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            var processor = _manager.CreateProcessor(processorName, processorParameters)!;
            var processContext = new PipelineProcessorContext(_manager, new PipelineBuildEvent { Parameters = processorParameters });
            using var _ = ContextScopeFactory.BeginContext(processContext);
            var processedObject = processor.Process(input!, processContext);

            // Add its dependencies and built assets to ours.
            _pipelineEvent.Dependencies.AddRangeUnique(processContext._pipelineEvent.Dependencies);
            _pipelineEvent.BuildAsset.AddRangeUnique(processContext._pipelineEvent.BuildAsset);

            return (TOutput)processedObject!;
        }

        public override TOutput Convert<TInput, TOutput>(TInput input, IContentProcessor processor)
        {
            var processorName = processor.GetType().Name.ToString();
            var processorParameters = new OpaqueDataDictionary();

            foreach (var prop in processor.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    processorParameters.Add(prop.Name, prop.GetValue(processor)!);
                }
            }

#pragma warning disable CS0618 // Type or member is obsolete
            return Convert<TInput, TOutput>(input, processorName, processorParameters);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Obsolete("Please pass importer and processor as instances instead of just their names.")]
        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName,
            OpaqueDataDictionary? processorParameters, string? importerName)
        {
            var sourceFilepath = PathHelper.Normalize(sourceAsset.Filename);
            string? procName = processorName;

            // The processorName can be null or empty. In this case the asset should
            // be imported but not processed. This is, for example, necessary to merge
            // animation files as described here:
            // http://blogs.msdn.com/b/shawnhar/archive/2010/06/18/merging-animation-files.aspx.
            bool processAsset = !string.IsNullOrEmpty(processorName);
            _manager.ResolveImporterAndProcessor(sourceFilepath, ref importerName, ref procName);

            var buildEvent = new PipelineBuildEvent
            {
                SourceFile = sourceFilepath,
                Importer = importerName!,
                Processor = processAsset ? processorName : null,
                Parameters = _manager.ValidateProcessorParameters(processorName, processorParameters),
            };

            var processedObject = _manager.ProcessContent(buildEvent);

            // Record that we processed this dependent asset.
            _pipelineEvent.Dependencies.AddUnique(sourceFilepath);

            return (TOutput)processedObject;
        }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, IContentImporter importer, IContentProcessor processor)
        {
            var importerName = importer.GetType().Name;
            var processorName = processor.GetType().Name;
            var processorParameters = new OpaqueDataDictionary();

            foreach (var prop in processor.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    processorParameters.Add(prop.Name, prop.GetValue(processor)!);
                }
            }

#pragma warning disable CS0618 // Type or member is obsolete
            return BuildAndLoadAsset<TInput, TOutput>(sourceAsset, processorName, processorParameters, importerName);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Obsolete("Please pass importer and processor as instances instead of just their names.")]
        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName,
            OpaqueDataDictionary? processorParameters, string? importerName, string? assetName)
        {
            // Be sure we have a good absolute path to the source content
            // or it may not cache correctly and create duplicates.
            sourceAsset.Filename = _manager.ResolveSourceFilePath(sourceAsset.Filename);

            if (string.IsNullOrEmpty(assetName))
                assetName = _manager.GetAssetName(sourceAsset.Filename, importerName, processorName, processorParameters);

            // Build the content.
            var buildEvent = _manager.BuildContent(sourceAsset.Filename, assetName, importerName, processorName, processorParameters);

            // Record that we built this dependent asset.
            _pipelineEvent.BuildAsset.AddUnique(buildEvent.DestFile);

            return new ExternalReference<TOutput>(buildEvent.DestFile);
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, IContentImporter importer, IContentProcessor processor, string? assetName)
        {
            var importerName = importer.GetType().Name;
            var processorName = processor.GetType().Name;
            var processorParameters = new OpaqueDataDictionary();

            foreach (var prop in processor.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.CanRead && prop.CanWrite)
                {
                    processorParameters.Add(prop.Name, prop.GetValue(processor)!);
                }
            }

#pragma warning disable CS0618 // Type or member is obsolete
            return BuildAsset<TInput, TOutput>(sourceAsset, processorName, processorParameters, importerName, assetName);
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
