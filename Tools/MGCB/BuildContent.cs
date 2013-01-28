﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace MGCB
{
    class BuildContent
    {
        [CommandLineParameter(
            Name = "@",
            ValueName = "responseFile",
            Description = "Read a text response file with additional command line options and switches.")]
        public readonly List<string> ResponseFiles = new List<string>();

        [CommandLineParameter(
            Name = "outputDir",
            ValueName = "directoryPath",
            Description = "The directory where all content is written.")]
        public string OutputDir = string.Empty;

        [CommandLineParameter(
            Name = "intermediateDir",
            ValueName = "directoryPath",
            Description = "The directory where all intermediate files are written.")]
        public string IntermediateDir = string.Empty;

        [CommandLineParameter(
            Name = "rebuild",
            Description = "Forces a full rebuild of all content.")]
        public bool Rebuild;

        [CommandLineParameter(
            Name = "clean",            
            Description = "Delete all previously built content and intermediate files.")]
        public bool Clean;

        [CommandLineParameter(
            Name = "reference",
            ValueName = "assemblyNameOrFile",
            Description = "Adds an assembly reference for resolving content importers, processors, and writers.")]
        public readonly List<string> References = new List<string>();

        [CommandLineParameter(
            Name = "importer",
            ValueName = "className",
            Description = "Defines the class name of the content importer for reading source content.")]
        public string Importer;

        [CommandLineParameter(
            Name = "processor",
            ValueName = "className",
            Description = "Defines the class name of the content processor for processing imported content.")]
        public string Processor;

        private OpaqueDataDictionary _processorParams = new OpaqueDataDictionary();

        [CommandLineParameter(
            Name = "processorParam",
            ValueName = "name=value",
            Description = "Defines a parameter name and value to set on a content processor.")]
        public void AddProcessorParam(string nameAndValue)
        {
            var keyAndValue = nameAndValue.Split('=');
            if (keyAndValue.Length != 2)
            {
                // Do we error out or something?
                return;
            }

            _processorParams.Add(keyAndValue[0], keyAndValue[1]);
        }

        [CommandLineParameter(
            Name = "build",
            ValueName = "sourceFile",
            Description = "Build the content source file using the previously set switches and options.")]
        public void OnBuild(string sourceFile)
        {
            var item = new ContentItem
            {
                SourceFile = sourceFile, 
                Importer = Importer, 
                Processor = Processor,
                ProcessorParams = new OpaqueDataDictionary()
            };

            // Copy the current processor parameters blind as we
            // will validate and remove invalid parameters during
            // the build process later.
            foreach (var pair in _processorParams)
                item.ProcessorParams.Add(pair.Key, pair.Value);

            _content.Add(item);
        }

        public class ContentItem
        {
            public string SourceFile;
            public string Importer;
            public string Processor;
            public OpaqueDataDictionary ProcessorParams;
        }

        private readonly List<ContentItem> _content = new List<ContentItem>();

        private PipelineManager _manager;

        public bool HasWork
        {
            get { return _content.Count > 0 || Clean; }    
        }

        public void Build(out int successCount, out int errorCount)
        {
            var projectDirectory = Directory.GetCurrentDirectory();
            var outputPath = Path.Combine(projectDirectory, OutputDir);
            var intermediatePath = Path.Combine(projectDirectory, IntermediateDir);
            _manager = new PipelineManager(projectDirectory, outputPath, intermediatePath);

            foreach(var r in References)
                _manager.AddAssembly(r);

            // TODO: We should be using the previously serialized list
            // of input files to the intermediate folder so we can clean
            // them here even if we don't build new content.

            errorCount = 0;
            successCount = 0;

            foreach (var c in _content)
            {
                var sourceFile = c.SourceFile;
                if (!Path.IsPathRooted(sourceFile))
                    sourceFile = Path.Combine(projectDirectory, c.SourceFile);

                // Clean any cached file first if requested.
                if (Clean || Rebuild)
                    _manager.CleanContent(sourceFile);

                try
                {
                    _manager.BuildContent(sourceFile,
                                          null,
                                          c.Importer,
                                          c.Processor,
                                          c.ProcessorParams);

                    ++successCount;
                }
                catch (PipelineException ex)
                {
                    Console.WriteLine("{0}: error: {1}", sourceFile, ex.Message);
                    ++errorCount;
                }
            }

            // TODO: If this isn't an incremental build we should
            // clean old content that is no longer a build item.

            // TODO: We should be using serializing the list
            // of input files we just built for use in cleaning
        }
    }
}
