// MonoGame - Copyright (C) The MonoGame Team
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
        [CommandLineParameter("@")]
        public readonly List<string> ResponseFiles = new List<string>();

        [CommandLineParameter("outputDir")]
        public string OutputDir;

        [CommandLineParameter("intermediateDir")]
        public string IntermediateDir;

        [CommandLineParameter("rebuild")]
        public bool Rebuild;

        [CommandLineParameter("clean")]
        public bool Clean;

        [CommandLineParameter("reference")]
        public readonly List<string> References = new List<string>();

        [CommandLineParameter("importer")]
        public string Importer;

        [CommandLineParameter("processor")]
        public string Processor;

        [CommandLineParameter("processorParam")]
        public readonly List<string> ProcessorParams = new List<string>();

        [CommandLineParameter("build")]
        public void OnBuild(string sourceFile)
        {
            var item = new ContentItem
            {
                SourceFile = sourceFile, 
                Importer = Importer, 
                Processor = Processor
            };

            foreach (var p in ProcessorParams)
            {
                var nameAndValue = p.Split('=');

                // We should have two objects here.
                if (nameAndValue.Length != 2)
                {
                    continue;
                }

                item.ProcessorParams.Add(nameAndValue[0], nameAndValue[1]);
            }
            ProcessorParams.Clear();

            _content.Add(item);
        }

        public class ContentItem
        {
            public string SourceFile;
            public string Importer;
            public string Processor;
            public OpaqueDataDictionary ProcessorParams = new OpaqueDataDictionary();
        }

        private readonly List<ContentItem> _content = new List<ContentItem>();

        private PipelineManager _manager;

        public void Build(out int fileCount, out int errorCount)
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
            fileCount = 0;

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

                    Console.WriteLine("{0}", sourceFile);
                    ++fileCount;
                }
                catch (PipelineException ex)
                {
                    ++errorCount;
                    Console.WriteLine("{0}: error: {1}", sourceFile, ex.Message);
                }
            }

            // TODO: If this isn't an incremental build we should
            // clean old content that is no longer a build item.

            // TODO: We should be using serializing the list
            // of input files we just built for use in cleaning
        }
    }
}
