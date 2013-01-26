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
        [Utilities.CommandLineParser.Name("Reference")]
        public List<string> References = new List<string>();

        [Utilities.CommandLineParser.Name("OutputDir")]
        [Utilities.CommandLineParser.Required]
        public string OutputDir;

        [Utilities.CommandLineParser.Name("IntermediateDir")]
        [Utilities.CommandLineParser.Required]
        public string IntermediateDir;

        [Utilities.CommandLineParser.Name("Importer")]
        public string Importer;

        [Utilities.CommandLineParser.Name("Processor")]
        public string Processor;

        [Utilities.CommandLineParser.Name("ProcessorParam")]
        public List<string> ProcessorParams = new List<string>();

        [Utilities.CommandLineParser.Name("Build")]
        public void OnBuild(string sourceFile)
        {
            var item = new ContentItem
            {
                SourceFile = sourceFile, 
                Importer = Importer, 
                Processor = Processor
            };
            Content.Add(item);
        }

        public class ContentItem
        {
            public string SourceFile;
            public string Importer;
            public string Processor;
        }

        public List<ContentItem> Content = new List<ContentItem>();

        private PipelineManager _manager;

        public void Build()
        {
            var projectDirectory = Directory.GetCurrentDirectory();
            var outputPath = Path.Combine(projectDirectory, OutputDir);
            var intermediatePath = Path.Combine(projectDirectory, IntermediateDir);
            _manager = new PipelineManager(projectDirectory, outputPath, intermediatePath);

            foreach(var r in References)
                _manager.AddAssembly(r);

            Console.WriteLine("Building {0} Items\n", Content.Count);

            var errorCount = 0;
            var fileCount = 0;

            foreach (var c in Content)
            {
                var sourceFile = c.SourceFile;
                if (!Path.IsPathRooted(sourceFile))
                    sourceFile = Path.Combine(projectDirectory, c.SourceFile);

                try
                {
                    _manager.BuildContent(sourceFile,
                                          null,
                                          c.Importer,
                                          c.Processor,
                                          null);

                    Console.WriteLine("{0}", sourceFile);
                    ++fileCount;
                }
                catch (PipelineException ex)
                {
                    ++errorCount;
                    Console.WriteLine("{0}: error: {1}", sourceFile, ex.Message);
                }
            }

            Console.WriteLine("\nBuild: {0} succeeded, {1} failed.", fileCount, errorCount);
        }
    }
}
