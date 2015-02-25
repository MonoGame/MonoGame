﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    public class PipelineProcessorContext : ContentProcessorContext
    {
        private readonly PipelineManager _manager;

        private readonly PipelineBuildEvent _pipelineEvent;

        public PipelineProcessorContext(PipelineManager manager, PipelineBuildEvent pipelineEvent)
        {
            _manager = manager;
            _pipelineEvent = pipelineEvent;
        }

        public override TargetPlatform TargetPlatform { get { return _manager.Platform; } }
        public override GraphicsProfile TargetProfile { get { return _manager.Profile; } }

        public override string BuildConfiguration { get { return _manager.Config; } }

        public override string IntermediateDirectory { get { return _manager.IntermediateDirectory; } }
        public override string OutputDirectory { get { return _manager.OutputDirectory; } }
        public override string OutputFilename { get { return _pipelineEvent.DestFile; } }

        public override OpaqueDataDictionary Parameters { get { return _pipelineEvent.Parameters; } }

        public override ContentBuildLogger Logger { get { return _manager.Logger; } }

        public override void AddDependency(string filename)
        {
            _pipelineEvent.Dependencies.AddUnique(filename);
        }

        public override void AddOutputFile(string filename)
        {
            _pipelineEvent.BuildOutput.AddUnique(filename);
        }

        public override TOutput Convert<TInput, TOutput>(   TInput input, 
                                                            string processorName,
                                                            OpaqueDataDictionary processorParameters)
        {
            var processor = _manager.CreateProcessor(processorName, processorParameters);
            var processContext = new PipelineProcessorContext(_manager, new PipelineBuildEvent { Parameters = processorParameters } );
            var processedObject = processor.Process(input, processContext);
           
            // Add its dependencies and built assets to ours.
            _pipelineEvent.Dependencies.AddRangeUnique(processContext._pipelineEvent.Dependencies);
            _pipelineEvent.BuildAsset.AddRangeUnique(processContext._pipelineEvent.BuildAsset);

            return (TOutput)processedObject;
        }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>( ExternalReference<TInput> sourceAsset,
                                                                    string processorName,
                                                                    OpaqueDataDictionary processorParameters,
                                                                    string importerName)
        {
            var sourceFilepath = PathHelper.Normalize(sourceAsset.Filename);

            // The processorName can be null or empty. In this case the asset should
            // be imported but not processed. This is, for example, necessary to merge
            // animation files as described here:
            // http://blogs.msdn.com/b/shawnhar/archive/2010/06/18/merging-animation-files.aspx.
            bool processAsset = !string.IsNullOrEmpty(processorName);
            _manager.ResolveImporterAndProcessor(sourceFilepath, ref importerName, ref processorName);

            var buildEvent = new PipelineBuildEvent 
            { 
                SourceFile = sourceFilepath,
                Importer = importerName,
                Processor = processAsset ? processorName : null,
                Parameters = _manager.ValidateProcessorParameters(processorName, processorParameters),
            };

            var processedObject = _manager.ProcessContent(buildEvent);

            // Record that we processed this dependent asset.
            _pipelineEvent.Dependencies.AddUnique(sourceFilepath);

            return (TOutput)processedObject;
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>( ExternalReference<TInput> sourceAsset,
                                                                                string processorName,
                                                                                OpaqueDataDictionary processorParameters,
                                                                                string importerName, 
                                                                                string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
                assetName = _manager.GetAssetName(sourceAsset.Filename, importerName, processorName, processorParameters);

            // Build the content.
            var buildEvent = _manager.BuildContent(sourceAsset.Filename, assetName, importerName, processorName, processorParameters);

            // Record that we built this dependent asset.
            _pipelineEvent.BuildAsset.AddUnique(buildEvent.DestFile);

            return new ExternalReference<TOutput>(buildEvent.DestFile);
        }
    }
}