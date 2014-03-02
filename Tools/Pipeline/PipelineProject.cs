// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MGCB;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tools.Pipeline
{
    /// <summary>
    /// The pipline project and helper methods.
    /// </summary>
    /// <remarks>
    /// NOTE: This class should never have any dependancy on the 
    /// controller or view... it is only the data "model".
    /// </remarks>
    class PipelineProject
    {
        private readonly List<ContentItem> _content = new List<ContentItem>();

        public string FilePath { get; set; }

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
            Name = "reference",
            ValueName = "assemblyNameOrFile",
            Description = "Adds an assembly reference for resolving content importers, processors, and writers.")]
        public readonly List<string> References = new List<string>();

        [CommandLineParameter(
            Name = "platform",
            ValueName = "targetPlatform",
            Description = "Set the target platform for this build.  Defaults to Windows.")]
        public TargetPlatform Platform;

        [CommandLineParameter(
            Name = "profile",
            ValueName = "graphicsProfile",
            Description = "Set the target graphics profile for this build.  Defaults to HiDef.")]
        public GraphicsProfile Profile;

        [CommandLineParameter(
            Name = "config",
            ValueName = "string",
            Description = "The optional build config string from the build system.")]
        public string Config = string.Empty;

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

        private readonly OpaqueDataDictionary _processorParams = new OpaqueDataDictionary();

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

            _processorParams.Remove(keyAndValue[0]);
            _processorParams.Add(keyAndValue[0], keyAndValue[1]);
        }

        [CommandLineParameter(
            Name = "build",
            ValueName = "sourceFile",
            Description = "Build the content source file using the previously set switches and options.")]
        public void OnBuild(string sourceFile)
        {
            // Make sure the source file is absolute.
            if (!Path.IsPathRooted(sourceFile))
                sourceFile = Path.Combine(Directory.GetCurrentDirectory(), sourceFile);

            // Remove duplicates... keep this new one.
            var previous = _content.FindIndex(e => string.Equals(e.SourceFile, sourceFile, StringComparison.InvariantCultureIgnoreCase));
            if (previous != -1)
                _content.RemoveAt(previous);

            // Create the item for processing later.
            var item = new ContentItem
            {
                SourceFile = sourceFile,
                Importer = Importer,
                Processor = Processor,
                ProcessorParams = new OpaqueDataDictionary()
            };
            _content.Add(item);

            // Copy the current processor parameters blind as we
            // will validate and remove invalid parameters during
            // the build process later.
            foreach (var pair in _processorParams)
                item.ProcessorParams.Add(pair.Key, pair.Value);
        }

        public bool IsDirty { get; set; }

        public PipelineProject()
        {
            IsDirty = true;
        }

        public void Attach(IProjectObserver observer)
        {            
        }

        public void NewProject()
        {
            _content.Clear();
        }

        public void LoadProject(string filePath)
        {
            _content.Clear();

            var parser = new CommandLineParser(this);
            parser.Title = "Pipeline";

            var commands =  File.ReadAllLines(filePath).
                            Select(x => x.Trim()).
                            Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("#")).
                            ToArray();

            parser.ParseCommandLine(commands);
        }

        public void CloseProject()
        {
            _content.Clear();
        }
    }
}