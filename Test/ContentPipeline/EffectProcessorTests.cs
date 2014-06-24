using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System.IO;

namespace MonoGame.Tests.ContentPipeline
{
    class EffectProcessorTests
    {
        class ImporterContext : ContentImporterContext
        {
            public override string IntermediateDirectory
            {
                get { throw new NotImplementedException(); }
            }

            public override ContentBuildLogger Logger
            {
                get { throw new NotImplementedException(); }
            }

            public override string OutputDirectory
            {
                get { throw new NotImplementedException(); }
            }

            public override void AddDependency(string filename)
            {
                throw new NotImplementedException();
            }
        }

        class ProcessorContext : ContentProcessorContext
        {
            private TargetPlatform _targetPlatform;
            private string _outputFilename;

            public ProcessorContext(    TargetPlatform targetPlatform,
                                        string outputFilename)
            {
                _targetPlatform = targetPlatform;
                _outputFilename = outputFilename;
            }

            public override string BuildConfiguration
            {
                get { throw new NotImplementedException(); }
            }

            public override string IntermediateDirectory
            {
                get { throw new NotImplementedException(); }
            }

            public override ContentBuildLogger Logger
            {
                get { throw new NotImplementedException(); }
            }

            public override string OutputDirectory
            {
                get { throw new NotImplementedException(); }
            }

            public override string OutputFilename
            {
                get { return _outputFilename; }
            }

            public override OpaqueDataDictionary Parameters
            {
                get { throw new NotImplementedException(); }
            }

            public override TargetPlatform TargetPlatform 
            {
                get { return _targetPlatform; }
            }

            public override GraphicsProfile TargetProfile
            {
                get { throw new NotImplementedException(); }
            }

            public override void AddDependency(string filename)
            {
            }

            public override void AddOutputFile(string filename)
            {
            }

            public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
            {
                throw new NotImplementedException();
            }

            public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
            {
                throw new NotImplementedException();
            }

            public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
            {
                throw new NotImplementedException();
            }
        }

        [Test]
        [TestCase("Effects/ParserTest.fx")]
        public void TestParser(string effectFile)
        {
            BuildEffect(effectFile, TargetPlatform.Windows);
        }

        [Test]
        [TestCase("Effects/Stock/AlphaTestEffect.fx")]
        [TestCase("Effects/Stock/BasicEffect.fx")]
        [TestCase("Effects/Stock/DualTextureEffect.fx")]
        [TestCase("Effects/Stock/EnvironmentMapEffect.fx")]
        [TestCase("Effects/Stock/SkinnedEffect.fx")]
        [TestCase("Effects/Stock/SpriteEffect.fx")]
        public void BuildStockEffect(string effectFile)
        {
            BuildEffect(effectFile, TargetPlatform.Windows);
        }

        private void BuildEffect(string effectFile, TargetPlatform targetPlatform)
        {
            var importerContext = new ImporterContext();
            var importer = new EffectImporter();
            var input = importer.Import(effectFile, importerContext);

            Assert.NotNull(input);

            var processorContext = new ProcessorContext(targetPlatform, Path.ChangeExtension(effectFile, ".xnb"));
            var processor = new EffectProcessor();
            var output = processor.Process(input, processorContext);

            Assert.NotNull(output);

            // TODO: Should we test the writer?
        }
    }
}