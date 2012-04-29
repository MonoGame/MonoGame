using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    public abstract class ContentProcessorContext
    {
        public abstract string BuildConfiguration { get; }

        public abstract string IntermediateDirectory { get; }

        public abstract ContentBuildLogger Logger { get; }

        public abstract string OutputDirectory { get; }

        public abstract string OutputFilename { get; }

        public abstract OpaqueDataDictionary Parameters { get; }

        public abstract TargetPlatform TargetPlatform { get; }

        public abstract GraphicsProfile TargetProfile { get; }

        public ContentProcessorContext()
        {
        }

        public abstract void AddDependency(string filename)
        {
        }

        public abstract void AddOutputFile(string filename)
        {
        }

        public TOutput BuildAndLoadAsset<TInput,TOutput>(
            ExternalReference<TInput> sourceAsset,
            string processorName
            )
        {
            return BuildAndLoadAsset<TInput, TOutput>(sourceAsset, processorName, null, null);
        }

        public abstract TOutput BuildAndLoadAsset<TInput,TOutput>(
            ExternalReference<TInput> sourceAsset,
            string processorName,
            OpaqueDataDictionary processorParameters,
            string importerName
            )
        {
        }

        public ExternalReference<TOutput> BuildAsset<TInput,TOutput>(
            ExternalReference<TInput> sourceAsset,
            string processorName
            )
        {
            return BuildAsset<TInput, TOutput>(sourceAsset, processorName, null, null, null);
        }

        public abstract ExternalReference<TOutput> BuildAsset<TInput,TOutput>(
            ExternalReference<TInput> sourceAsset,
            string processorName,
            OpaqueDataDictionary processorParameters,
            string importerName,
            string assetName
            )
        {
        }

        public TOutput Convert<TInput,TOutput>(
            TInput input,
            string processorName
            )
        {
            return Convert<TInput, TOutput>(input, processorName, new OpaqueDataDictionary());
        }

        public abstract TOutput Convert<TInput,TOutput>(
            TInput input,
            string processorName,
            OpaqueDataDictionary processorParameters
            )
        {
        }
    }
}
