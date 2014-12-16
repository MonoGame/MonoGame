using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
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

        [Test]
        [TestCase("Assets/Effects/ParserTest.fx")]
        public void TestParser(string effectFile)
        {
            BuildEffect(effectFile, TargetPlatform.Windows);
        }

        [Test]
        [TestCase("Assets/Effects/Stock/AlphaTestEffect.fx")]
        [TestCase("Assets/Effects/Stock/BasicEffect.fx")]
        [TestCase("Assets/Effects/Stock/DualTextureEffect.fx")]
        [TestCase("Assets/Effects/Stock/EnvironmentMapEffect.fx")]
        [TestCase("Assets/Effects/Stock/SkinnedEffect.fx")]
        [TestCase("Assets/Effects/Stock/SpriteEffect.fx")]
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

            var processorContext = new TestProcessorContext(targetPlatform, Path.ChangeExtension(effectFile, ".xnb"));
            var processor = new EffectProcessor();
            var output = processor.Process(input, processorContext);

            Assert.NotNull(output);

            // TODO: Should we test the writer?
        }
    }
}