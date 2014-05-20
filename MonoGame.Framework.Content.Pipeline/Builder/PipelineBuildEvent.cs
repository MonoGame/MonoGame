// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    public class PipelineBuildEvent
    {
        public static readonly string Extension = ".mgcontent";

        public PipelineBuildEvent()
        {
            SourceFile = string.Empty;
            DestFile = string.Empty;
            Importer = string.Empty;
            Processor = string.Empty;
            Parameters = new OpaqueDataDictionary();
            ParametersXml = new List<Pair>();
            Dependencies = new List<string>();
            BuildAsset = new List<string>();
            BuildOutput = new List<string>();
        }

        /// <summary>
        /// Absolute path to the source file.
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// Absolute path to the output file.
        /// </summary>
        public string DestFile { get; set; }

        public DateTime DestTime { get; set; }

        public string Importer { get; set; }

        public string Processor { get; set; }

        [XmlIgnore]
        public OpaqueDataDictionary Parameters { get; set; }

        public class Pair
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        [XmlElement("Parameters")]
        public List<Pair> ParametersXml { get; set; }

        public List<string> Dependencies { get; set; }

        public List<string> BuildAsset { get; set; }

        public List<string> BuildOutput { get; set; }

        public static PipelineBuildEvent Load(string filePath)
        {
            var fullFilePath = Path.GetFullPath(filePath);
            var deserializer = new XmlSerializer(typeof (PipelineBuildEvent));
            PipelineBuildEvent pipelineEvent;
            try
            {
                using (var textReader = new StreamReader(fullFilePath))
                    pipelineEvent = (PipelineBuildEvent) deserializer.Deserialize(textReader);
            }
            catch (Exception)
            {
                return null;
            }

            // Repopulate the parameters from the serialized state.            
            foreach (var pair in pipelineEvent.ParametersXml)
                pipelineEvent.Parameters.Add(pair.Key, pair.Value);
            pipelineEvent.ParametersXml.Clear();

            return pipelineEvent;
        }

        public void Save(string filePath)
        {
			var fullFilePath = Path.GetFullPath(filePath);
            // Make sure the directory exists.
            Directory.CreateDirectory(Path.GetDirectoryName(fullFilePath) + Path.DirectorySeparatorChar);

            // Convert the parameters into something we can serialize.
            ParametersXml.Clear();
            foreach (var pair in Parameters)
            {
                var converter = TypeDescriptor.GetConverter(pair.Value.GetType());
                ParametersXml.Add(new Pair { Key = pair.Key, Value = converter.ConvertToString(pair.Value) });
            }

            // Serialize our state.
            var serializer = new XmlSerializer(typeof (PipelineBuildEvent));
            using (var textWriter = new StreamWriter(fullFilePath, false, new UTF8Encoding(false)))
                serializer.Serialize(textWriter, this);
        }

        public bool NeedsRebuild(PipelineBuildEvent cachedEvent)
        {
            // If we have no previously cached build event then we cannot
            // be sure that the state hasn't changed... force a rebuild.
            if (cachedEvent == null)
                return true;

            // Verify that the last write time of the dest file matches
            // what we recorded when it was built.  If it is different
            // that means someone modified it and we need to rebuild.
            var destWriteTime = File.GetLastWriteTime(DestFile);
            if (cachedEvent.DestTime != destWriteTime)
                return true;

            // If the source and dest files changed... this is always a rebuild.
            if (File.GetLastWriteTime(SourceFile) >= destWriteTime)
                return true;

            // Are any of the dependancy files newer than the dest file?
            foreach (var depFile in cachedEvent.Dependencies)
            {
                if (File.GetLastWriteTime(depFile) >= destWriteTime)
                    return true;
            }

            // This shouldn't happen...  but if the source or dest files changed
            // then force a rebuild.
            if (cachedEvent.SourceFile != SourceFile ||
                cachedEvent.DestFile != DestFile)
                return true;

            // Did the importer change?
            // TODO: I need to test the assembly versions here!           
            if (cachedEvent.Importer != Importer)
                return true;

            // Did the processor change?
            // TODO: I need to test the assembly versions here!
            if (cachedEvent.Processor != Processor)
                return true;

            // If the count of parameters is different then we have
            // to assume the results of processing are different.
            if (cachedEvent.Parameters.Count != Parameters.Count)
                return true;

            // Finally did any of the processor parameters change?
            foreach (var pair in cachedEvent.Parameters)
            {
                // If the key value doesn't exist... then rebuild.
                object value;
                if (!Parameters.TryGetValue(pair.Key, out value))
                    return true;

                // If the values are the same type and do not match... rebuild.
                if (value.GetType().IsInstanceOfType(pair.Value))
                {
                    if (!value.Equals(pair.Value))
                        return true;
                }
                else
                {
                    var typeConverter = TypeDescriptor.GetConverter(value.GetType());
                    var converted = typeConverter.ConvertTo(value, pair.Value.GetType());
                    if (!converted.Equals(pair.Value))
                        return true;
                }
            }

            return false;
        }
    };
}