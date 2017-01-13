// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Content.Pipeline.Builder;


namespace MGCB
{
    class BuildContent
    {        
        public bool LaunchDebugger;

        [CommandLineParameter(
            Name = "launchdebugger",
            Description = "Wait for debugger to attach before building content.")]
        public void OnLaunchDebugger(bool val)
        {
            LaunchDebugger = val;

            if (LaunchDebugger)
            {
                Debugger.Launch();
            }
        }

        [CommandLineParameter(
            Name = "quiet",
            Description = "Only output content build errors.")]
        public bool Quiet = false;

        [CommandLineParameter(
            Name = "@",
            ValueName = "responseFile",
            Description = "Read a text response file with additional command line options and switches.")]
        // This property only exists for documentation.
        // The actual handling of '/@' is done in the preprocess step.
        public List<string> ResponseFiles
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        [CommandLineParameter(
            Name = "workingDir",
            ValueName = "directoryPath",
            Description = "The working directory where all source content is located.")]
        public void SetWorkingDir(string path)
        {
            Directory.SetCurrentDirectory(path);
        }

        [CommandLineParameter(
            Name = "outputDir",
            ValueName = "directoryPath",
            Description = "The directory where all content is written.")]
        public string OutputDir = string.Empty;

        [CommandLineParameter(
            Name = "intermediateDir",
            ValueName = "directoryPath",
            Description = "The directory where all intermediate files are written.")]
        public string IntermediateDir = string.Empty;

        [CommandLineParameter(
            Name = "rebuild",
            Description = "Forces a full rebuild of all content.")]
        public bool Rebuild = false;

        [CommandLineParameter(
            Name = "clean",            
            Description = "Delete all previously built content and intermediate files.")]
        public bool Clean = false;

        [CommandLineParameter(
            Name = "incremental",
            Description = "Skip cleaning files not included in the current build.")]
        public bool Incremental = false;

        [CommandLineParameter(
            Name = "reference",
            ValueName = "assemblyNameOrFile",
            Description = "Adds an assembly reference for resolving content importers, processors, and writers.")]
        public readonly List<string> References = new List<string>();

        [CommandLineParameter(
            Name = "platform",
            ValueName = "targetPlatform",
            Description = "Set the target platform for this build.  Defaults to Windows desktop DirectX.")]
        public TargetPlatform Platform = TargetPlatform.Windows;

        [CommandLineParameter(
            Name = "profile",
            ValueName = "graphicsProfile",
            Description = "Set the target graphics profile for this build.  Defaults to HiDef.")]
        public GraphicsProfile Profile = GraphicsProfile.HiDef;

        [CommandLineParameter(
            Name = "config",
            ValueName = "string",
            Description = "The optional build config string from the build system.")]
        public string Config = string.Empty;

        [CommandLineParameter(
            Name = "importer",
            ValueName = "className",
            Description = "Defines the class name of the content importer for reading source content.")]
        public string Importer = null;

        [CommandLineParameter(
            Name = "processor",
            ValueName = "className",
            Description = "Defines the class name of the content processor for processing imported content.")]
        public void SetProcessor(string processor)
        {
            _processor = processor;
            
            // If you are changing the processor then reset all 
            // the processor parameters.
            _processorParams.Clear();
        }

        private string _processor = null;
        private readonly OpaqueDataDictionary _processorParams = new OpaqueDataDictionary();

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
            ValueName = "sourceFile;assetName",
            Description = "Build the content source file using the previously set switches and options.")]
        public void OnBuild(string sourceFileAssetName)
        {
            // Parse string into the sourceFile and assetName terms.
            var words = sourceFileAssetName.Split(';');
            var sourceFile = words[0];
            var assetName = string.Empty;
            if (words.Length > 1)
                assetName = words[1];

            // Make sure the source file is absolute.
            if (!Path.IsPathRooted(sourceFile))
                sourceFile = Path.Combine(Directory.GetCurrentDirectory(), sourceFile);

            sourceFile = PathHelper.Normalize(sourceFile);

            // Remove duplicates... keep this new one.
            var previous = _content.FindIndex(e => string.Equals(e.SourceFile, sourceFile, StringComparison.InvariantCultureIgnoreCase));
            if (previous != -1)
                _content.RemoveAt(previous);

            // Create the item for processing later.
            var item = new ContentItem
            {                
                SourceFile = sourceFile,
                AssetName = assetName,
                Importer = Importer, 
                Processor = _processor,
                ProcessorParams = new OpaqueDataDictionary()
            };
            _content.Add(item);

            // Copy the current processor parameters blind as we
            // will validate and remove invalid parameters during
            // the build process later.
            foreach (var pair in _processorParams)
                item.ProcessorParams.Add(pair.Key, pair.Value);
        }

        [CommandLineParameter(
            Name = "copy",
            ValueName = "sourceFile",
            Description = "Copy the content source file verbatim to the output directory.")]
        public void OnCopy(string sourceFileAssetName)
        {
            // Remove duplicates... keep this new one.
            var previous = _copyItems.FindIndex(e => string.Equals(e, sourceFileAssetName, StringComparison.InvariantCultureIgnoreCase));
            if (previous != -1)
                _copyItems.RemoveAt(previous);

            _copyItems.Add(sourceFileAssetName);
        }

        [CommandLineParameter(
            Name = "compress",
            Description = "Compress the XNB files for smaller file sizes.")]
        public bool CompressContent = false;

        public class ContentItem
        {
            public string AssetName;
            public string SourceFile;            
            public string Importer;
            public string Processor;
            public OpaqueDataDictionary ProcessorParams;
        }
        
        private readonly List<ContentItem> _content = new List<ContentItem>();

        private readonly List<string> _copyItems = new List<string>();

        private PipelineManager _manager;

        public bool HasWork
        {
            get { return _content.Count > 0 || _copyItems.Count > 0 || Clean; }    
        }

        string ReplaceSymbols(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
                return parameter;
            return parameter
                .Replace("$(Platform)", Platform.ToString())
                .Replace("$(Configuration)", Config)
                .Replace("$(Config)", Config)
                .Replace("$(Profile)", this.Profile.ToString());
        }

        public void Build(out int successCount, out int skipCount, out int errorCount)
        {
            var projectDirectory = PathHelper.Normalize(Directory.GetCurrentDirectory());

            var outputPath = ReplaceSymbols (OutputDir);
            if (!Path.IsPathRooted(outputPath))
                outputPath = PathHelper.Normalize(Path.GetFullPath(Path.Combine(projectDirectory, outputPath)));

            var intermediatePath = ReplaceSymbols (IntermediateDir);
            if (!Path.IsPathRooted(intermediatePath))
                intermediatePath = PathHelper.Normalize(Path.GetFullPath(Path.Combine(projectDirectory, intermediatePath)));
            
            _manager = new PipelineManager(projectDirectory, outputPath, intermediatePath);
            _manager.Logger = new ConsoleLogger();
            _manager.CompressContent = CompressContent;

            // If the intent is to debug build, break at the original location
            // of any exception, eg, within the actual importer/processor.
            if (LaunchDebugger)
                _manager.RethrowExceptions = false;

            // Feed all the assembly references to the pipeline manager
            // so it can resolve importers, processors, writers, and types.
            foreach (var r in References)
            {
                var assembly = r;
                if (!Path.IsPathRooted(assembly))
                    assembly = Path.GetFullPath(Path.Combine(projectDirectory, assembly));
                _manager.AddAssembly(assembly);
            }

            // Load the previously serialized list of built content.
            var contentFile = Path.Combine(intermediatePath, PipelineBuildEvent.Extension);
            var previousContent = SourceFileCollection.Read(contentFile);

            // If the target changed in any way then we need to force
            // a fuull rebuild even under incremental builds.
            var targetChanged = previousContent.Config != Config ||
                                previousContent.Platform != Platform ||
                                previousContent.Profile != Profile;

            // jcf: This looks super suspect to me.
            //      Either we are doing an incremintal build, in which case this doesn't need to happen, because each item decides if it needs to 'clean' or not individually as it is built
            //      Or, we aren't doing an incrimental build, in which case shouldn't someone have just called a separate Clean method before-hand, in which -all- previous content is cleaned irregardless of if they are present in the current build?            									
			//      ALSO, note that this is not cleaning copy items!
            foreach (var prev in previousContent.SourceFiles)
            {
                var words = prev.Split(';');

                // old format
                if (words.Length < 2)
                    continue;

                // old format
                var prevAbsOutputFilepath = words[1];
                if (!Path.IsPathRooted(prevAbsOutputFilepath))
                    continue;

                bool inContent = false;

                foreach (var cur in _content)
                {
                    var absOutputFilepath = cur.AssetName;
                    _manager.ResolveOutputFilepath(cur.SourceFile, ref absOutputFilepath, true);

                    if (absOutputFilepath.Equals(prevAbsOutputFilepath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        inContent = true;
                        break;
                    }
                }
                
                var cleanOldContent = !inContent && !Incremental;
                var cleanRebuiltContent = inContent && (Rebuild || Clean);

                if (cleanRebuiltContent || cleanOldContent || targetChanged)
                {
                    _manager.CleanContent(prevAbsOutputFilepath);
                }
            }
			
			// alternate implementation idea....
			
			// If requested, do a full clean.
            // The 'current' content and copy items are actually irrelevent for this case.
            // Because we serialize all the build events that occured during each build
            // so those are the items we want to clean. It doesn't matter what the 'current' items are
            // until later when we go to build.
            /*
            if ((Clean || Rebuild) && !Incremental)
            {
                // jcf: we should have written out the absolute-output-file-path...
                //      after all, we don't know what the previous build's output directory actually was...
                foreach (var prev in previousContent.SourceFiles)
                {
                    var words = prev.Split(';');

                    // old format
                    if (words.Length < 2)
                        continue;

                    // old format
                    var absOutputFilepath = words[1];
                    if (!Path.IsPathRooted(absOutputFilepath))
                        continue;

                    _manager.CleanContent(absOutputFilepath);
                }
            }
            */

            var newContent = new SourceFileCollection
            {
                Profile = _manager.Profile = Profile,
                Platform = _manager.Platform = Platform,
                Config = _manager.Config = Config
            };
            errorCount = 0;
            skipCount = 0;
            successCount = 0;

            // Before building the content, register all files to be built. (Necessary to
            // correctly resolve external references.)
            foreach (var c in _content)
            {
                try
                {   
                    _manager.RegisterContent(c.SourceFile, c.AssetName, c.Importer, c.Processor, c.ProcessorParams);
                }
                catch
                {
                    // Ignore exception. Exception will be handled below.
                }
            }

            foreach (var c in _copyItems)
            {
                var word = c.Split(';');
                var srcPath = word[0];
                var dstPath = word[1];
                
                _manager.ResolveOutputFilepath(srcPath, ref dstPath, false);
                var sourceFileAssetName = string.Concat(srcPath, ";", dstPath);

                try
                {
                    var wasCopied = _manager.CopyContent(srcPath, dstPath);

                    if (wasCopied)
                        successCount++;
                    else
                        skipCount++;

                    newContent.SourceFiles.Add(sourceFileAssetName);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("{0}: error: {1}", c, ex.Message);
                    if (ex.InnerException != null)
                        Console.Error.WriteLine(ex.InnerException.ToString());

                    ++errorCount;
                }
            }

            foreach (var item in _content)
            {
                var inputFilePath = item.SourceFile;
                var outputFilePath = item.AssetName;
                _manager.ResolveOutputFilepath(inputFilePath, ref outputFilePath, true);

                try
                {
                    PipelineBuildEvent buildEvent;
                    var wasBuilt = _manager.BuildContent(inputFilePath,
                                                         outputFilePath,
                                                         item.Importer,
                                                         item.Processor,
                                                         item.ProcessorParams,
                                                         out buildEvent);

                    var sourceFileAssetName = string.Concat(buildEvent.SourceFile, ";", buildEvent.DestFile);
                    newContent.SourceFiles.Add(sourceFileAssetName);

                    if (wasBuilt)
                        successCount++;
                    else
                        skipCount++;
                }
                catch (InvalidContentException ex)
                {
                    var message = string.Empty;
                    if (ex.ContentIdentity != null && !string.IsNullOrEmpty(ex.ContentIdentity.SourceFilename))
                    {
                        message = ex.ContentIdentity.SourceFilename;
                        if (!string.IsNullOrEmpty(ex.ContentIdentity.FragmentIdentifier))
                            message += "(" + ex.ContentIdentity.FragmentIdentifier + ")";
                        message += ": ";
                    }
                    message += ex.Message;
                    Console.WriteLine(message);
                    ++errorCount;
                }
                catch (PipelineException ex)
                {
                    Console.Error.WriteLine("{0}: error: {1}", inputFilePath, ex.Message);
                    if (ex.InnerException != null)
                        Console.Error.WriteLine(ex.InnerException.ToString());
                    ++errorCount;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("{0}: error: {1}", inputFilePath, ex.Message);
                    if (ex.InnerException != null)
                        Console.Error.WriteLine(ex.InnerException.ToString());
                    ++errorCount;
                }
            }

            // If this is an incremental build we merge the list
            // of previous content with the new list.
            if (Incremental && !targetChanged)
                newContent.Merge(previousContent);

            // Delete the old file and write the new content 
            // list if we have any to serialize.
            FileHelper.DeleteIfExists(contentFile);
            if (newContent.SourceFiles.Count > 0)
                newContent.Write(contentFile);            
        }
    }
}
