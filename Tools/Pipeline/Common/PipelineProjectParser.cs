// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// ReSharper disable ValueParameterNotUsed
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
    public enum ParseResult
    {
        Ok,
        Error,
    }

    public class PipelineProjectParser
    {
        #region Other Data

        private readonly PipelineProject _project;
        private readonly IContentItemObserver _observer;
        private readonly OpaqueDataDictionary _processorParams = new OpaqueDataDictionary();
        
        private string _processor;
        private string _importer;
        
        #endregion

        #region CommandLineParameters

        [CommandLineParameter(Name = "outputDir")]
        private string OutputDir
        {
            set
            {
                _project.OutputDir = value;
            }
        }

        [CommandLineParameter(Name = "intermediateDir")]
        private string IntermediateDir
        {
            set
            {
                _project.IntermediateDir = value;                 
            }        
        }

        [CommandLineParameter(Name = "reference")]
        private List<string> References 
        {
            get { return _project.References; }
            set
            {
                _project.References = value;
            }            
        }

        [CommandLineParameter(Name = "platform")]
        private TargetPlatform Platform
        {
            set
            {
                _project.Platform = value;                 
            }
        }

        [CommandLineParameter(Name = "profile")]
        private GraphicsProfile Profile
        {
            set
            {
                _project.Profile = value;
            }
        }

        [CommandLineParameter(Name = "config")]
        private string Config
        {
            set
            {
                _project.Config = value;                 
            }
        }

        // Allow a MGCB file containing the /rebuild parameter to be imported without error.
        [CommandLineParameter(Name = "rebuild")]
        private bool Rebuild
        {
            set { }
        }

        // Allow a MGCB file containing the /clean parameter to be imported without error.
        [CommandLineParameter(Name = "clean")]
        private bool Clean
        {
            set { }
        }

        [CommandLineParameter(Name = "compress")]
        private bool Compress
        {
            set
            {
                _project.Compress = value;                 
            }
        }

        [CommandLineParameter(Name = "importer")]
        private string Importer
        {
            set
            {
                _importer = value;
            }
        }

        [CommandLineParameter(Name = "processor")]
        private string Processor
        {            
            set
            {
                _processor = value;
                _processorParams.Clear();
            }
        }

        [CommandLineParameter(Name = "processorParam")]
        private void AddProcessorParam(string nameAndValue)
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

        [CommandLineParameter(Name = "build")]
        private void OnBuild(string sourceFile)
        {
            AddContent(sourceFile, false);
        }

        [CommandLineParameter(Name = "copy")]
        private void OnCopy(string sourceFile)
        {
            // Make sure the source file is relative to the project.
            var projectDir = ProjectDirectory + "\\";
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
                ProcessorParams = new OpaqueDataDictionary()
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

        public ParseResult OpenProject(string projectFilePath, MGBuildParser.ErrorCallback errorCallback)
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
            if (parser.Parse(commands))
                return ParseResult.Ok;

            return ParseResult.Error;
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
                                    var valueStr = converter.ConvertTo(value, typeof(string));
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

        public ParseResult ImportProject(string projectFilePath)
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

            return ParseResult.Ok;
        }

        public bool AddContent(string sourceFile, bool skipDuplicates)
        {
            // Make sure the source file is relative to the project.
            var projectDir = ProjectDirectory + "\\";
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
                ImporterName = _importer,
                ProcessorName = _processor,
                ProcessorParams = new OpaqueDataDictionary()
            };
            _project.ContentItems.Add(item);

            // Copy the current processor parameters blind as we
            // will validate and remove invalid parameters during
            // the build process later.
            foreach (var pair in _processorParams)
                item.ProcessorParams.Add(pair.Key, pair.Value);

            return true;
        }

        public static bool OpenProjectContainingItem(string contentItemPath, out PipelineProject project)
        {
            const string searchPattern = "*.mgcb";

            var searchItemFullPath = Path.GetFullPath(contentItemPath);

            var path = Path.GetFullPath(Path.GetDirectoryName(searchItemFullPath));
            var root = Path.GetPathRoot(path);

            while (!root.Equals(path))
            {
                // Search for project files in the current directory.
                var projectFiles = Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

                // Search for the specified content item within the project.
                foreach (var projectFile in projectFiles)
                {
                    var pipelineProject = new PipelineProject();
                    var parser = new PipelineProjectParser(null, pipelineProject);
                    if (parser.OpenProject(projectFile, null) == ParseResult.Error)
                        continue;

                    foreach (var i in pipelineProject.ContentItems)
                    {
                        var itemFullPath = PipelineUtil.GetFullPath(i.OriginalPath, pipelineProject.Location);
                        if (itemFullPath.Equals(searchItemFullPath, StringComparison.OrdinalIgnoreCase))
                        {
                            project = pipelineProject;
                            return true;
                        }
                    }
                }

                // Recurse up to the parent directory.
                path = Path.GetDirectoryName(path);
            }

            project = null;
            return false;
        }

        #region Private Helpers

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

        #endregion
    }
}
// ReSharper restore ValueParameterNotUsed