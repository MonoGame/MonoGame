using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Tools.Pipeline
{
    class ContentItem : IProjectItem
    {
        public string SourceFile;
        public string Importer;
        public string Processor;
        public OpaqueDataDictionary ProcessorParams;

        public string Label 
        { 
            get
            {
                return System.IO.Path.GetFileName(SourceFile);
            }
        }

        public string Path
        {
            get
            {
                return System.IO.Path.GetDirectoryName(SourceFile);
            }
        }

        public string Icon { get; private set; }
    }
}
