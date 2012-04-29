using System;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    [Serializable]
    public class ContentIdentity
    {
        public string FragmentIdentifier { get; set; }

        public string SourceFilename { get; set; }

        public string SourceTool { get; set; }

        public ContentIdentity()
            : this(string.Empty, string.Empty, string.Empty)
        {
        }

        public ContentIdentity(string sourceFilename)
            : this(sourceFilename, string.Empty, string.Empty)
        {
        }

        public ContentIdentity(string sourceFilename, string sourceTool)
            : this(sourceFilename, sourceTool, string.Empty)
        {
        }

        public ContentIdentity(string sourceFilename, string sourceTool, string fragmentIdentifier)
        {
            SourceFilename = sourceFilename;
            SourceTool = sourceTool;
            FragmentIdentifier = fragmentIdentifier;
        }
    }
}
