using System;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    public class ExternalReference<T> : ContentItem
    {
        public string Filename { get; set; }

        public ExternalReference()
        {
            Filename = string.Empty;
        }

        public ExternalReference(string filename)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            Filename = filename;
        }

        public ExternalReference(string filename, ContentIdentity relativeToContent)
        {
            if (string.IsNullOrEmpty(filename))
                throw new ArgumentNullException("filename");
            if (relativeToContent == null)
                throw new ArgumentNullException("relativeToContent");
            if (string.IsNullOrEmpty(relativeToContent.SourceFilename))
                throw new ArgumentNullException("relativeToContent.SourceFilename");
            Filename = Path.Combine(relativeToContent.SourceFilename, filename);
        }
    }
}
