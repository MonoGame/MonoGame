using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    class ContentItem
    {
        public string SourceFile;
        public string Importer;
        public string Processor;
        public OpaqueDataDictionary ProcessorParams;
    }
}
