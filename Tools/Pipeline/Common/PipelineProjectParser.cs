// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using MGCB;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using PathHelper = MonoGame.Framework.Content.Pipeline.Builder.PathHelper;

namespace MonoGame.Tools.Pipeline
{
    /// <summary>
    /// Performs save, load, and import actions for/on a PipelineProject.
    /// </summary>
    internal class PipelineProjectParser
    {
        #region Other Data

        private readonly PipelineProject _project;
        private readonly IController _controller;
        private readonly IView _view;
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
        public void OnProcessorParam(string nameAndValue)
        {
            AddProcessorParam(nameAndValue);            
        }

        [CommandLineParameter(
            Name = "build",
            ValueName = "sourceFile",
            Description = "Build the content source file using the previously set switches and options.")]
        public void OnBuild(string path)
        {
            AddBuildItem(path, false);
        }

         [CommandLineParameter(
            Name = "reference",
            ValueName = "assemblyNameOrFile",
            Description = "Adds an assembly reference for resolving content importers, processors, and writers.")]
        public void OnReference(string path)
        {
            AddReference(path);
        }

        [CommandLineParameter(
            Name = "copy",
            ValueName = "sourceFile",
            Description = "Copy the content source file verbatim to the output directory.")]
        public void OnCopy(string path)
        {
            AddCopyItem(path);
        }        

        #endregion

        public PipelineProjectParser(IController controller, PipelineProject project, IView view)
        {
            _controller = controller;
            _project = project;
            _view = view;
        }        

        public void OpenProject(string projectFilePath)
        {
            _project.ContentItems.Clear();

            // Store the file name for saving later.
            _project.FilePath = projectFilePath;

            var parser = new CommandLineParser(this)
                {
                    Title = "Pipeline",
                    Error = _controller.OutputWriter,
                };            

            var commands = File.ReadAllLines(projectFilePath).
                                Select(x => x.Trim()).
                                Where(x => !string.IsNullOrEmpty(x) && !x.StartsWith("#")).
                                ToArray();

            parser.ParseCommandLine(commands);
        }

        public void SaveProject()
        {
            const string lineFormat = "/{0}:{1}";
            const string processorParamFormat = "{0}={1}";
            string line;

            using (var io = File.CreateText(_project.FilePath))
            {
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
                    // Wrap content item lines with a begin comment line
                    // to make them more cohesive (for version control).                  
                    line = string.Format("#begin {0}", i.SourceFile);
                    io.WriteLine(line);

                    if (i.BuildAction == BuildAction.Copy)
                    {
                        line = string.Format(lineFormat, "copy", i.SourceFile);
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
                                        var valueStr = converter.ConvertTo(value, typeof(string));
                                        line = string.Format(lineFormat, "processorParam", string.Format(processorParamFormat, j.Name, valueStr));
                                        io.WriteLine(line);
                                    }
                                }
                            }
                        }

                        line = string.Format(lineFormat, "build", i.SourceFile);
                        io.WriteLine(line);
                        io.WriteLine();
                    }
                }
            }
        }

        public void ImportProject(string projectFilePath)
        {
            _project.FilePath = projectFilePath.Remove(projectFilePath.LastIndexOf('.')) + ".mgcb";

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
                                AddReference(hintPath);
                            }
                        }
                        else if (buildAction.Equals("Content"))
                        {
                            string include, copyToOutputDirectory;
                            ReadIncludeContent(io, out include, out copyToOutputDirectory);

                            if (!copyToOutputDirectory.Equals("Never"))
                            {         
                                var sourceFilePath = Path.GetDirectoryName(projectFilePath);
                                sourceFilePath += "\\" + include;
                                
                                AddCopyItem(sourceFilePath);
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
                            
                            AddBuildItem(sourceFilePath, false);
                        }
                    }
                }
            }
        }

        public bool AddBuildItem(string path, bool skipDuplicate)
        {            
            // Make path relative to project.
            var projectDir = _project.Location + "\\";
            path = PathHelper.GetRelativePath(projectDir, path);            

            // Avoid duplicates.
            var previous = _project.ContentItems.FindIndex(e => string.Equals(e.SourceFile, path, StringComparison.InvariantCultureIgnoreCase));
            if (previous != -1)
            {
                if (skipDuplicate)
                {
                    _view.OutputAppendLine("Skipped adding '{0}' because Build ContentItem was a duplicate.", path);
                    return false;
                }

                _view.OutputAppendLine("Existing Build ContentItem with path '{0}' has been removed.", path);

                _project.ContentItems.RemoveAt(previous);
            }

            // Create the item for processing later.
            var item = new ContentItem
            {
                Controller = _controller,
                BuildAction = BuildAction.Build,
                SourceFile = path,
                ImporterName = Importer,
                ProcessorName = Processor,
                ProcessorParams = new OpaqueDataDictionary()
            };
            _project.ContentItems.Add(item);

            // Copy the current processor parameters blind as we
            // will validate and remove invalid parameters during
            // the build process later.
            foreach (var pair in _processorParams)
                item.ProcessorParams.Add(pair.Key, pair.Value);

            _view.OutputAppendLine("Adding Build ContentItem '{0}'.", path);

            return true;
        }

        public bool AddCopyItem(string path)
        {
            // Make path relative to project.
            var projectDir = _project.Location + "\\";
            path = PathHelper.GetRelativePath(projectDir, path);            

            // Avoid duplicates.
            var previous = _project.ContentItems.FirstOrDefault(e => e.SourceFile.Equals(path));
            if (previous != null)
            {
                _view.OutputAppendLine("Skipped adding '{0}' because Copy ContentItem was a duplicate.", path);
                return false;
            }

            // Create the item for processing later.
            var item = new ContentItem
            {
                BuildAction = BuildAction.Copy,
                SourceFile = path,
                ProcessorParams = new OpaqueDataDictionary()
            };
            _project.ContentItems.Add(item);

            _view.OutputAppendLine("Adding Copy ContentItem '{0}'.", path);

            return true;
        }

        public bool AddReference(string path)
        {
            // Make path relative to project.
            var projectDir = _project.Location + "\\";
            path = PathHelper.GetRelativePath(projectDir, path);            

            // Avoid duplicates.
            if (_project.References.Contains(path))
            {
                _view.OutputAppendLine("Skipped adding '{0}' because Reference was a duplicate.", path);
                return false;
            }

            _project.References.Add(path);

            _view.OutputAppendLine("Adding Reference '{0}'.", path);

            return true;
        }

        private bool AddProcessorParam(string namevalue)
        {
            var words = namevalue.Split('=');
            if (words.Length != 2)
                return false;

            _processorParams.Remove(words[0]);
            _processorParams.Add(words[0], words[1]);

            return true;
        }

        private void ReadIncludeReference(XmlReader io, out string include, out string hintPath)
        {
            include = io.GetAttribute("Include");
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
                        hintPath = io.Value;
                    }
                }
            }
        }

        private void ReadIncludeContent(XmlReader io, out string include, out string copyToOutputDirectory)
        {
            copyToOutputDirectory = null;
            include = io.GetAttribute("Include");

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
                                copyToOutputDirectory = io.Value;
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

            include = io.GetAttribute("Include");
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
                                name = io.Value;
                                break;
                            case "Importer":
                                io.Read();
                                importer = io.Value;
                                break;
                            case "Processor":
                                io.Read();
                                processor = io.Value;
                                break;
                            default:
                                if (io.LocalName.Contains("ProcessorParameters_"))
                                {
                                    var line = io.LocalName.Replace("ProcessorParameters_", "");
                                    line += "=";
                                    io.Read();
                                    line += io.Value;
                                    parameters.Add(line);
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
            const string commentFormat = "\n#----------------------------------------------------------------------------#\n";

            label = " " + label + " ";
            var src = commentFormat.Length / 2 - label.Length / 2;
            var dst = src + label.Length;

            return commentFormat.Substring(0, src) + label + commentFormat.Substring(dst);
        }
    }
}