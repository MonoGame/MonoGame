// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using MGCB;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using PathHelper = MonoGame.Framework.Content.Pipeline.Builder.PathHelper;

namespace MonoGame.Tools.Pipeline
{
    public class PipelineProjectParser
    {
        #region Other Data

        private readonly PipelineProject _project;
        private readonly IContentItemObserver _observer;
        private readonly OpaqueDataDictionary _processorParams = new OpaqueDataDictionary();
        
        private string _processor;
        
        #endregion

        #region CommandLineParameters
        
        [CommandLineParameter(
            Name = "outputDir",
            ValueName = "directoryPath",
            Description = "The directory where all content is written.")]
        public string OutputDir { set { _project.OutputDir = value; } }

        [CommandLineParameter(
            Name = "intermediateDir",
            ValueName = "directoryPath",
            Description = "The directory where all intermediate files are written.")]
        public string IntermediateDir { set { _project.IntermediateDir = value; } }

        [CommandLineParameter(
            Name = "reference",
            ValueName = "assemblyNameOrFile",
            Description = "Adds an assembly reference for resolving content importers, processors, and writers.")]
        public List<string> References 
        {
            set { _project.References = value; }
            get { return _project.References; } 
        }

        [CommandLineParameter(
            Name = "platform",
            ValueName = "targetPlatform",
            Description = "Set the target platform for this build.  Defaults to Windows.")]
        public TargetPlatform Platform { set { _project.Platform = value; } }

        [CommandLineParameter(
            Name = "profile",
            ValueName = "graphicsProfile",
            Description = "Set the target graphics profile for this build.  Defaults to HiDef.")]
        public GraphicsProfile Profile { set { _project.Profile = value; } }

        [CommandLineParameter(
            Name = "config",
            ValueName = "string",
            Description = "The optional build config string from the build system.")]
        public string Config { set { _project.Config = value; } }

        #pragma warning disable 414

        // Allow a MGCB file containing the /rebuild parameter to be imported without error
        [CommandLineParameter(
            Name = "rebuild",
            ValueName = "bool",
            Description = "Forces a rebuild of the project.")]
        public bool Rebuild { set { _rebuild = value; } }
        private bool _rebuild;

        // Allow a MGCB file containing the /clean parameter to be imported without error
        [CommandLineParameter(
            Name = "clean",
            ValueName = "bool",
            Description = "Removes intermediate and output files.")]
        public bool Clean { set { _clean = value; } }
        private bool _clean;

        #pragma warning restore 414

        [CommandLineParameter(
            Name = "compress",
            ValueName = "bool",
            Description = "Content files can be compressed for smaller file sizes.")]
        public bool Compress { set { _project.Compress = value; } }

        [CommandLineParameter(
            Name = "importer",
            ValueName = "className",
            Description = "Defines the class name of the content importer for reading source content.")]
        public string Importer;

        [CommandLineParameter(
            Name = "processor",
            ValueName = "className",
            Description = "Defines the class name of the content processor for processing imported content.")]
        public string Processor
        {
            get { return _processor; }
            set
            {
                _processor = value;
                _processorParams.Clear();
            }
        }

        [CommandLineParameter(
            Name = "processorParam",
            ValueName = "name=value",
            Description = "Defines a parameter name and value to set on a content processor.")]
        public void AddProcessorParam(string nameAndValue)
        {
            var keyAndValue = nameAndValue.Split('=');
            if (keyAndValue.Length != 2)
            {
                // Do we error out or something?
                return;
            }

            _processorParams.Remove(keyAndValue[0]);
            _processorParams.Add(keyAndValue[0], keyAndValue[1]);
        }

        [CommandLineParameter(
            Name = "build",
            ValueName = "sourceFile",
            Description = "Build the content source file using the previously set switches and options.")]
        public void OnBuild(string sourceFile)
        {
            AddContent(sourceFile, false);
        }

        public bool AddContent(string sourceFile, bool skipDuplicates)
        {
            // Make sure the source file is relative to the project.
            var projectDir = ProjectDirectory + Path.DirectorySeparatorChar;

            sourceFile = PathHelper.GetRelativePath(projectDir, sourceFile);

            // Do we have a duplicate?
            var previous = _project.ContentItems.FindIndex(e => string.Equals(e.OriginalPath, sourceFile, StringComparison.InvariantCultureIgnoreCase));
            if (previous != -1)
            {
                if (skipDuplicates)
                    return false;

                // Replace the duplicate.
                _project.ContentItems.RemoveAt(previous);
            }

            // Create the item for processing later.
            var item = new ContentItem
            {
                Observer = _observer,
                BuildAction = BuildAction.Build,
                OriginalPath = sourceFile,
                ImporterName = Importer,
                ProcessorName = Processor,
                ProcessorParams = new OpaqueDataDictionary(),
                Exists = File.Exists(projectDir + sourceFile)
            };
            _project.ContentItems.Add(item);

            // Copy the current processor parameters blind as we
            // will validate and remove invalid parameters during
            // the build process later.
            foreach (var pair in _processorParams)
                item.ProcessorParams.Add(pair.Key, pair.Value);

            return true;
        }

        [CommandLineParameter(
            Name = "copy",
            ValueName = "sourceFile",
            Description = "Copy the content source file verbatim to the output directory.")]
        public void OnCopy(string sourceFile)
        {
            // Make sure the source file is relative to the project.
            var projectDir = ProjectDirectory + Path.DirectorySeparatorChar;

            sourceFile = PathHelper.GetRelativePath(projectDir, sourceFile);

            // Remove duplicates... keep this new one.
            var previous = _project.ContentItems.FirstOrDefault(e => e.OriginalPath.Equals(sourceFile));
            if (previous != null)
                _project.ContentItems.Remove(previous);

            // Create the item for processing later.
            var item = new ContentItem
            {
                BuildAction = BuildAction.Copy,
                OriginalPath = sourceFile,
                ProcessorParams = new OpaqueDataDictionary(),
                Exists = File.Exists(projectDir + sourceFile)
            };
            _project.ContentItems.Add(item);

            // Copy the current processor parameters blind as we
            // will validate and remove invalid parameters during
            // the build process later.
            foreach (var pair in _processorParams)
                item.ProcessorParams.Add(pair.Key, pair.Value);
        }

        #endregion

        public PipelineProjectParser(IContentItemObserver observer, PipelineProject project)
        {
            _observer = observer;
            _project = project;
        }        

        public void OpenProject(string projectFilePath, MGBuildParser.ErrorCallback errorCallback)
        {
            _project.ContentItems.Clear();

            // Store the file name for saving later.
            _project.OriginalPath = projectFilePath;

            var parser = new MGBuildParser(this);
            parser.Title = "Pipeline";

            if (errorCallback != null)
                parser.OnError += errorCallback;

            var commands = new string[]
                {
                    string.Format("/@:{0}", projectFilePath),
                };
            parser.Parse(commands);
        }

        public void SaveProject()
        {
            using (var io = File.CreateText(_project.OriginalPath))
                SaveProject(io, null);
        }
        
        public void SaveProject(TextWriter io, Func<ContentItem, bool> filterItem)
        {
            const string lineFormat = "/{0}:{1}";
            const string processorParamFormat = "{0}={1}";
            string line;

            line = FormatDivider("Global Properties");
            io.WriteLine(line);

            line = string.Format(lineFormat, "outputDir", _project.OutputDir);
            io.WriteLine(line);

            line = string.Format(lineFormat, "intermediateDir", _project.IntermediateDir);
            io.WriteLine(line);

            line = string.Format(lineFormat, "platform", _project.Platform);
            io.WriteLine(line);

            line = string.Format(lineFormat, "config", _project.Config);
            io.WriteLine(line);

            line = string.Format(lineFormat, "profile", _project.Profile);
            io.WriteLine(line);

            line = string.Format(lineFormat, "compress", _project.Compress);
            io.WriteLine(line);

            line = FormatDivider("References");
            io.WriteLine(line);

            foreach (var i in _project.References)
            {
                line = string.Format(lineFormat, "reference", i);
                io.WriteLine(line);
            }

            line = FormatDivider("Content");
            io.WriteLine(line);

            foreach (var i in _project.ContentItems)
            {
                // Reject any items that don't pass the filter.              
                if (filterItem != null && filterItem(i))
                    continue;

                // Wrap content item lines with a begin comment line
                // to make them more cohesive (for version control).                  
                line = string.Format("#begin {0}", i.OriginalPath);
                io.WriteLine(line);

                if (i.BuildAction == BuildAction.Copy)
                {
                    line = string.Format(lineFormat, "copy", i.OriginalPath);
                    io.WriteLine(line);
                    io.WriteLine();
                }
                else
                {

                    // Write importer.
                    {
                        line = string.Format(lineFormat, "importer", i.ImporterName);
                        io.WriteLine(line);
                    }

                    // Write processor.
                    {
                        line = string.Format(lineFormat, "processor", i.ProcessorName);
                        io.WriteLine(line);
                    }

                    // Write processor parameters.
                    {
                        if (i.Processor == PipelineTypes.MissingProcessor)
                        {
                            // Could still be missing the real processor.
                            // If so, write the string parameters from import.
                            foreach (var j in i.ProcessorParams)
                            {
                                line = string.Format(lineFormat, "processorParam", string.Format(processorParamFormat, j.Key, j.Value));
                                io.WriteLine(line);
                            }
                        }
                        else
                        {
                            // Otherwise, write only values which are defined by the real processor.
                            foreach (var j in i.Processor.Properties)
                            {
                                object value = null;
                                if (i.ProcessorParams.ContainsKey(j.Name))
                                    value = i.ProcessorParams[j.Name];

                                // JCF: I 'think' writting an empty string for null would be appropriate but to be on the safe side
                                //      im just not writting the value at all.
                                if (value != null)
                                {
                                    var converter = PipelineTypes.FindConverter(value.GetType());
                                    var valueStr = converter.ConvertTo(null, CultureInfo.InvariantCulture, value, typeof(string));
                                    line = string.Format(lineFormat, "processorParam", string.Format(processorParamFormat, j.Name, valueStr));
                                    io.WriteLine(line);
                                }
                            }
                        }
                    }

                    line = string.Format(lineFormat, "build", i.OriginalPath);
                    io.WriteLine(line);
                    io.WriteLine();
                }
            }
        }

        public void ImportProject(string projectFilePath)
        {
            _project.OriginalPath = projectFilePath.Remove(projectFilePath.LastIndexOf('.')) + ".mgcb";

            using (var io = XmlReader.Create(File.OpenText(projectFilePath)))
            {
                while (io.Read())
                {
                    if (io.NodeType == XmlNodeType.Element)
                    {
                        var buildAction = io.LocalName;
                        if (buildAction.Equals("Reference"))
                        {
                            string include, hintPath;
                            ReadIncludeReference(io, out include, out hintPath);

                            if (!string.IsNullOrEmpty(hintPath) &&
                                hintPath.IndexOf("microsoft", StringComparison.CurrentCultureIgnoreCase) == -1 &&
                                hintPath.IndexOf("monogamecontentprocessors", StringComparison.CurrentCultureIgnoreCase) == -1)
                            {
                                _project.References.Add(hintPath);
                            }
                        }
                        else if (buildAction.Equals("Content") || buildAction.Equals("None"))
                        {
                            string include, copyToOutputDirectory;
                            ReadIncludeContent(io, out include, out copyToOutputDirectory);

                            if (!string.IsNullOrEmpty(copyToOutputDirectory) && !copyToOutputDirectory.Equals("Never"))
                            {
                                var sourceFilePath = Path.GetDirectoryName(projectFilePath);
                                sourceFilePath += "\\" + include;

                                OnCopy(sourceFilePath);
                            }
                        }
                        else if (buildAction.Equals("Compile"))
                        {
                            string include, name, importer, processor;
                            string[] processorParams;
                            ReadIncludeCompile(io, out include, out name, out importer, out processor, out processorParams);

                            Importer = importer;
                            Processor = processor;
                            if (processorParams != null)
                            {
                                foreach (var i in processorParams)
                                    AddProcessorParam(i);
                            }

                            var sourceFilePath = Path.GetDirectoryName(projectFilePath);
                            sourceFilePath += "\\" + include;

                            OnBuild(sourceFilePath);
                        }
                    }
                }
            }
        }

        private string ProjectDirectory
        {
            get
            {
                return _project.Location;                
            }
        }

        private void ReadIncludeReference(XmlReader io, out string include, out string hintPath)
        {
            include = io.GetAttribute("Include").Unescape();            
            hintPath = null;

            if (!io.IsEmptyElement)
            {
                var depth = io.Depth;
                for (io.Read(); io.Depth != depth; io.Read())
                {
                    // process sub nodes
                    if (io.IsStartElement("HintPath"))
                    {
                        io.Read();
                        hintPath = io.Value.Unescape();
                    }
                }
            }
        }

        private void ReadIncludeContent(XmlReader io, out string include, out string copyToOutputDirectory)
        {
            copyToOutputDirectory = null;
            include = io.GetAttribute("Include").Unescape();

            if (!io.IsEmptyElement)
            {
                var depth = io.Depth;
                for (io.Read(); io.Depth != depth; io.Read())
                {
                    // process sub nodes here.

                    if (io.IsStartElement())
                    {
                        switch (io.LocalName)
                        {
                            case "CopyToOutputDirectory":
                                io.Read();
                                copyToOutputDirectory = io.Value.Unescape();
                                break;
                        }
                    }
                }
            }
        }

        private void ReadIncludeCompile(XmlReader io,
                                        out string include,
                                        out string name,
                                        out string importer,
                                        out string processor,
                                        out string[] processorParams)
        {
            name = null;
            importer = null;
            processor = null;

            include = io.GetAttribute("Include").Unescape();
            var parameters = new List<string>();

            if (!io.IsEmptyElement)
            {
                var depth = io.Depth;
                for (io.Read(); io.Depth != depth; io.Read())
                {
                    // process sub nodes here.

                    if (io.IsStartElement())
                    {
                        switch (io.LocalName)
                        {
                            case "Name":
                                io.Read();
                                name = io.Value.Unescape();
                                break;
                            case "Importer":
                                io.Read();
                                importer = io.Value.Unescape();
                                break;
                            case "Processor":
                                io.Read();
                                processor = io.Value.Unescape();
                                break;
                            default:
                                if (io.LocalName.Contains("ProcessorParameters_"))
                                {
                                    var line = io.LocalName.Replace("ProcessorParameters_", "");
                                    line += "=";
                                    io.Read();
                                    line += io.Value;
                                    parameters.Add(line.Unescape());
                                }
                                break;
                        }
                    }
                }
            }

            processorParams = parameters.ToArray();
        }

        private string FormatDivider(string label)
        {
            var commentFormat = Environment.NewLine + "#----------------------------------------------------------------------------#" + Environment.NewLine;

            label = " " + label + " ";
            var src = commentFormat.Length / 2 - label.Length / 2;
            var dst = src + label.Length;

            return commentFormat.Substring(0, src) + label + commentFormat.Substring(dst);
        }
    }
}
