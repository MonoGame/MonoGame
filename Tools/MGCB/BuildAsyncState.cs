using Microsoft.Xna.Framework.Content.Pipeline;

namespace MGCB
{
    internal class BuildAsyncState
    {
        public string SourceFile { get; internal set; }
        public string Importer { get; internal set; }
        public string Processor { get; internal set; }
        public OpaqueDataDictionary ProcessorParams { get; internal set; }
        public ConsoleAsyncLogger Logger { get; internal set; }
    }
}