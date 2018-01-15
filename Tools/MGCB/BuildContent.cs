// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Content.Pipeline.Builder;


namespace MGCB
{
    class BuildContent
    {
        [CommandLineParameter(
            Name = "launchdebugger",
            Flag = "d",
            Description = "Wait for debugger to attach before building content.")]
        public bool LaunchDebugger = false;

        [CommandLineParameter(
            Name = "multithreadMode",
            Description = "Multithread Mode (experimental).")]
        public bool MultithreadMode = false;

        [CommandLineParameter(
            Name = "quiet",
            Flag = "q",
            Description = "Only output content build errors.")]
        public bool Quiet = false;

        [CommandLineParameter(
            Name = "@",
            Flag = "@",
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
            Flag = "w",
            ValueName = "directoryPath",
            Description = "The working directory where all source content is located.")]
        public void SetWorkingDir(string path)
        {
            Directory.SetCurrentDirectory(path);
        }

        [CommandLineParameter(
            Name = "outputDir",
            Flag = "o",
            ValueName = "path",
            Description = "The directory where all content is written.")]
        public string OutputDir = string.Empty;

        [CommandLineParameter(
            Name = "intermediateDir",
            Flag = "n",
            ValueName = "path",
            Description = "The directory where all intermediate files are written.")]
        public string IntermediateDir = string.Empty;

        [CommandLineParameter(
            Name = "rebuild",
            Flag = "r",
            Description = "Forces a full rebuild of all content.")]
        public bool Rebuild = false;

        [CommandLineParameter(
            Name = "clean",
            Flag = "c",
            Description = "Delete all previously built content and intermediate files.")]
        public bool Clean = false;

        [CommandLineParameter(
            Name = "incremental",
            Flag = "I",
            Description = "Skip cleaning files not included in the current build.")]
        public bool Incremental = false;

        [CommandLineParameter(
            Name = "reference",
            Flag = "f",
            ValueName = "assembly",
            Description = "Adds an assembly reference for resolving content importers, processors, and writers.")]
        public readonly List<string> References = new List<string>();

        [CommandLineParameter(
            Name = "platform",
            Flag = "t",
            ValueName = "targetPlatform",
            Description = "Set the target platform for this build.  Defaults to Windows desktop DirectX.")]
        public TargetPlatform Platform = TargetPlatform.Windows;

        [CommandLineParameter(
            Name = "profile",
            Flag = "g",
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
            Flag = "i",
            ValueName = "className",
            Description = "Defines the class name of the content importer for reading source content.")]
        public string Importer = null;

        [CommandLineParameter(
            Name = "processor",
            Flag = "p",
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
            Flag = "P",
            ValueName = "name=value",
            Description = "Defines a parameter name and value to set on a content processor.")]
        public void AddProcessorParam(string nameAndValue)
        {
            var keyAndValue = nameAndValue.Split('=', ':');
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
            Flag = "b",
            ValueName = "sourceFile",
            Description = "Build the content source file using the previously set switches and options.")]
        public void OnBuild(string sourceFile)
        {
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
        public void OnCopy(string sourceFile)
        {
            if (!Path.IsPathRooted(sourceFile))
                sourceFile = Path.Combine(Directory.GetCurrentDirectory(), sourceFile);

            sourceFile = PathHelper.Normalize(sourceFile);

            // Remove duplicates... keep this new one.
            var previous = _copyItems.FindIndex(e => string.Equals(e, sourceFile, StringComparison.InvariantCultureIgnoreCase));
            if (previous != -1)
                _copyItems.RemoveAt(previous);

            _copyItems.Add(sourceFile);
        }

        [CommandLineParameter(
            Name = "compress",
            Description = "Compress the XNB files for smaller file sizes.")]
        public bool CompressContent = false;

        public class ContentItem
        {
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

        public void Build(out int successCount, out int errorCount)
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

            // First clean previously built content.
            foreach (var sourceFile in previousContent.SourceFiles)
            {
                var inContent = _content.Any(e => string.Equals(e.SourceFile, sourceFile, StringComparison.InvariantCultureIgnoreCase));
                var cleanOldContent = !inContent && !Incremental;
                var cleanRebuiltContent = inContent && (Rebuild || Clean);
                if (cleanRebuiltContent || cleanOldContent || targetChanged)
                    _manager.CleanContent(sourceFile);                
            }

            var newContent = new SourceFileCollection
            {
                Profile = _manager.Profile = Profile,
                Platform = _manager.Platform = Platform,
                Config = _manager.Config = Config
            };
            errorCount = 0;
            successCount = 0;

            // Before building the content, register all files to be built. (Necessary to
            // correctly resolve external references.)
            foreach (var c in _content)
            {
                try
                {
                    _manager.RegisterContent(c.SourceFile, null, c.Importer, c.Processor, c.ProcessorParams);
                }
                catch
                {
                    // Ignore exception. Exception will be handled below.
                }
            }

            if (!MultithreadMode) // single thread
            {
                foreach (var c in _content)
                {
                    try
                    {
                        _manager.BuildContent(_manager.Logger,
                                              c.SourceFile,
                                              null,
                                              c.Importer,
                                              c.Processor,
                                              c.ProcessorParams);

                        newContent.SourceFiles.Add(c.SourceFile);

                        ++successCount;
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
                        Console.Error.WriteLine("{0}: error: {1}", c.SourceFile, ex.Message);
                        if (ex.InnerException != null)
                            Console.Error.WriteLine(ex.InnerException.ToString());
                        ++errorCount;
                    }
                    catch (Exception ex)
                    {
                        Console.Error.WriteLine("{0}: error: {1}", c.SourceFile, ex.Message);
                        if (ex.InnerException != null)
                            Console.Error.WriteLine(ex.InnerException.ToString());
                        ++errorCount;
                    }
                }
            }
            else // multithread
            {
                var buildTaskQueue = new Queue<Task<PipelineBuildEvent>>();
                var activeBuildTasks = new List<Task<PipelineBuildEvent>>();
                bool firstTask = true;

                int ci = 0;
                while (ci < _content.Count || activeBuildTasks.Count > 0 || buildTaskQueue.Count > 0)
                {
                    // Create build tasks.
                    while (activeBuildTasks.Count < Environment.ProcessorCount && ci < _content.Count)
                    {
                        BuildAsyncState buildState = new BuildAsyncState()
                        {
                            SourceFile = _content[ci].SourceFile,
                            Importer = _content[ci].Importer,
                            Processor = _content[ci].Processor,
                            ProcessorParams = _content[ci].ProcessorParams,
                            Logger = new ConsoleAsyncLogger(_manager.Logger),
                        };
                        buildState.Logger.Immediate = firstTask;

                        var task = Task.Factory.StartNew<PipelineBuildEvent>((stateobj) =>
                        {
                            var state = stateobj as BuildAsyncState;
                            return _manager.BuildContent(state.Logger,
                                                  state.SourceFile,
                                                  null,
                                                  state.Importer,
                                                  state.Processor,
                                                  state.ProcessorParams);
                        }, buildState, TaskCreationOptions.PreferFairness);
                        buildTaskQueue.Enqueue(task);
                        activeBuildTasks.Add(task);
                        firstTask = false;
                        ci++;
                    }

                    if (buildTaskQueue.Count > 0)
                    {
                        // Get task at the top of the queue.
                        var topTask = buildTaskQueue.Peek();
                        var topBuildState = topTask.AsyncState as BuildAsyncState;
                        topBuildState.Logger.Immediate = true;

                        // Remove task from queue if completed.
                        if (topTask.IsCompleted || topTask.IsCanceled || topTask.IsFaulted)
                        {
                            buildTaskQueue.Dequeue();
                            //flash log
                            topBuildState.Logger.Flush();

                            if (topTask.IsFaulted)
                            {
                                if (topTask.Exception.InnerException is InvalidContentException)
                                {
                                    InvalidContentException ex = topTask.Exception.InnerException as InvalidContentException;
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
                                else
                                {
                                    Exception ex = topTask.Exception.InnerException;
                                    Console.Error.WriteLine("{0}: error: {1}", topBuildState.SourceFile, ex.Message);
                                    if (ex.InnerException != null)
                                        Console.Error.WriteLine(ex.InnerException.ToString());
                                    ++errorCount;
                                }
                            }
                            else if (topTask.IsCanceled)
                            {
                                // ?
                            }

                            continue;
                        }
                    }

                    Task.WaitAny(activeBuildTasks.ToArray());

                    // Remove completed tasks.
                    for (int i = activeBuildTasks.Count - 1; i >= 0; i--)
                    {
                        var task = activeBuildTasks[i];
                        if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
                        {
                            activeBuildTasks.RemoveAt(i);
                            if (task.IsCompleted)
                            {
                                var buildState = task.AsyncState as BuildAsyncState;
                                newContent.SourceFiles.Add(buildState.SourceFile);
                                ++successCount;
                            }
                        }
                    }
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

            // Process copy items (files that bypass the content pipeline)
            foreach (var c in _copyItems)
            {
                try
                {
                    // Figure out an asset name relative to the project directory,
                    // retaining the file extension.
                    // Note that replacing a sub-path like this requires consistent
                    // directory separator characters.
                    var relativeName = c.Replace(projectDirectory, string.Empty)
                                            .TrimStart(Path.DirectorySeparatorChar)
                                            .TrimStart(Path.AltDirectorySeparatorChar);
                    var dest = Path.Combine(outputPath, relativeName);

                    // Only copy if the source file is newer than the destination.
                    // We may want to provide an option for overriding this, but for
                    // nearly all cases this is the desired behavior.
                    if (File.Exists(dest))
                    {
                        var srcTime = File.GetLastWriteTimeUtc(c);
                        var dstTime = File.GetLastWriteTimeUtc(dest);
                        if (srcTime <= dstTime)
                        {
                            Console.WriteLine("Skipping {0}", c);
                            continue;
                        }
                    }

                    // Create the destination directory if it doesn't already exist.
                    var destPath = Path.GetDirectoryName(dest);
                    if (!Directory.Exists(destPath))
                        Directory.CreateDirectory(destPath);

                    File.Copy(c, dest, true);

                    // Destination file should not be read-only even if original was.
                    var fileAttr = File.GetAttributes(dest);
                    fileAttr = fileAttr & (~FileAttributes.ReadOnly);
                    File.SetAttributes(dest, fileAttr);

                    ++successCount;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("{0}: error: {1}", c, ex.Message);
                    if (ex.InnerException != null)
                        Console.Error.WriteLine(ex.InnerException.ToString());

                    ++errorCount;
                }
            }
        }

        [CommandLineParameter(
            Name = "help",
            Flag = "h",
            Description = "Displays this help.")]
        public void Help()
        {
            MGBuildParser.Instance.ShowError(null);
        }
    }
}
