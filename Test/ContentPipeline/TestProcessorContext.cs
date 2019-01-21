using System;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.ContentPipeline
{
    class TestProcessorContext : ContentProcessorContext
    {
        private readonly TargetPlatform _targetPlatform;
        private readonly string _outputFilename;
        private readonly TestContentBuildLogger _logger;

        public TestProcessorContext(    TargetPlatform targetPlatform,
                                        string outputFilename)
        {
            _targetPlatform = targetPlatform;
            _outputFilename = outputFilename;
            _logger = new TestContentBuildLogger();
        }

        public override string BuildConfiguration
        {
            get { return "Debug"; }
        }

        public override string IntermediateDirectory
        {
            get { throw new NotImplementedException(); }
        }

        public override ContentBuildLogger Logger
        {
            get { return _logger; }
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

#if !XNA
        public override ContentIdentity SourceIdentity
        {
            get { throw new NotImplementedException(); }
        }
#endif

        public override TargetPlatform TargetPlatform
        {
            get { return _targetPlatform; }
        }

        public override GraphicsProfile TargetProfile
        {
            get { return GraphicsProfile.HiDef; }
        }

        public override void AddDependency(string filename)
        {
        }

        public override void AddOutputFile(string filename)
        {
        }

        public override TOutput BuildAndLoadAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            return default(TOutput);
        }

        public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            throw new NotImplementedException();
        }

        public override TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            // MaterialProcessor essentially transforms its
            // input and returns it... not a copy.  So this
            // seems like a reasonable shortcut for testing.
            if (typeof(TOutput) == typeof(MaterialContent) && typeof(TInput).IsAssignableFrom(typeof(MaterialContent)))
                return (TOutput)((object)input);

            var processor = (ContentProcessor<TInput, TOutput>)typeof(ContentProcessor<TInput, TOutput>).Assembly.CreateInstance("Microsoft.Xna.Framework.Content.Pipeline.Processors."+ processorName);
            if (processor != null) {
                var type = processor.GetType();
                foreach (var kvp in processorParameters)
                {
                    var property = type.GetProperty(kvp.Key);
                    if (property == null)
                        continue;
                    property.SetValue(processor, kvp.Value);
                }
                return processor.Process(input, this);
            }

            throw new NotImplementedException();
        }
    }
}
