using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MGCB
{
    [XmlRoot(ElementName = "SourceFileCollection")]
    public sealed class SourceFileCollection : List<string>
    {
        static public SourceFileCollection Read(string filePath)
        {
            var deserializer = new XmlSerializer(typeof(SourceFileCollection));
            try
            {
                using (var textReader = new StreamReader(filePath))
                    return (SourceFileCollection)deserializer.Deserialize(textReader);
            }
            catch (Exception)
            {
            }

            return new SourceFileCollection();
        }

        public void Write(string filePath)
        {
            var serializer = new XmlSerializer(typeof(SourceFileCollection));
            using (var textWriter = new StreamWriter(filePath, false, new UTF8Encoding(false)))
                serializer.Serialize(textWriter, this);            
        }

        public void Merge(SourceFileCollection other)
        {
            foreach (var sourceFile in other)
            {
                var inContent = this.Any(e => string.Equals(e, sourceFile, StringComparison.InvariantCultureIgnoreCase));
                if (!inContent)
                    Add(sourceFile);
            }
        }
    }
}
