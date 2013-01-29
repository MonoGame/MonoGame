﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;

namespace MGCB
{
    [XmlRoot(ElementName = "SourceFileCollection")]
    public sealed class SourceFileCollection
    {
        public GraphicsProfile Profile { get; set; }

        public TargetPlatform Platform { get; set; }

        public string Config { get; set; }

        [XmlArrayItem("File")]
        public List<string> SourceFiles { get; set; }

        public SourceFileCollection()
        {
            SourceFiles = new List<string>();
            Config = string.Empty;
        }

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
            foreach (var sourceFile in other.SourceFiles)
            {
                var inContent = SourceFiles.Any(e => string.Equals(e, sourceFile, StringComparison.InvariantCultureIgnoreCase));
                if (!inContent)
                    SourceFiles.Add(sourceFile);
            }
        }
    }
}
