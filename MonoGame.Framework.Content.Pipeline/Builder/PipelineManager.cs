// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;
using Microsoft.Xna.Framework.Content.Pipeline.Builder.Convertors;
using System.Diagnostics;

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    public class PipelineManager
    {
        [DebuggerDisplay("ImporterInfo: {type.Name}")]
        private struct ImporterInfo
        {
            public ContentImporterAttribute attribue;
            public Type type;
        };

        private List<ImporterInfo> _importers;

        [DebuggerDisplay("ProcessorInfo: {type.Name}")]
        private struct ProcessorInfo
        {
            public ContentProcessorAttribute attribue;
            public Type type;
        };

        private List<ProcessorInfo> _processors;

        private List<Type> _writers;

        public string ProjectDirectory { get; private set; }
        public string OutputDirectory { get; private set; }
        public string IntermediateDirectory { get; private set; }

        private ContentCompiler _compiler;
        private MethodInfo _compileMethod;

        public ContentBuildLogger Logger { get; set; }

        public List<string> Assemblies { get; private set; }

        /// <summary>
        /// The current target graphics profile for which all content is built.
        /// </summary>
        public GraphicsProfile Profile { get; set; }

        /// <summary>
        /// The current target platform for which all content is built.
        /// </summary>
        public TargetPlatform Platform { get; set; }

        /// <summary>
        /// The build configuration passed thru to content processors.
        /// </summary>
        public string Config { get; set; }


        public PipelineManager(string projectDir, string outputDir, string intermediateDir)
        {
            Assemblies = new List<string>();
            Assemblies.Add(null);
            Logger = new PipelineBuildLogger();

            ProjectDirectory = PathHelper.NormalizeDirectory(projectDir);
            OutputDirectory = PathHelper.NormalizeDirectory(outputDir);
            IntermediateDirectory = PathHelper.NormalizeDirectory(intermediateDir);

	    RegisterCustomConverters ();
        }

	public void AssignTypeConverter<IType, IConverterType> ()
	{
		TypeDescriptor.AddAttributes (typeof (IType), new TypeConverterAttribute (typeof (IConverterType)));
	}

	private void RegisterCustomConverters ()
	{
		AssignTypeConverter<Microsoft.Xna.Framework.Color, StringToColorConverter> ();
	}

        public void AddAssembly(string assemblyFilePath)
        {
            if (assemblyFilePath == null)
                throw new ArgumentException("assemblyFilePath cannot be null!");
            if (!Path.IsPathRooted(assemblyFilePath))
                throw new ArgumentException("assemblyFilePath must be absolute!");

            // Make sure we're not adding the same assembly twice.
            assemblyFilePath = PathHelper.Normalize(assemblyFilePath);
            if (!Assemblies.Contains(assemblyFilePath))
            {
                Assemblies.Add(assemblyFilePath);

                //TODO need better way to update caches
                _processors = null;
                _importers = null;
                _writers = null;
            }
        }

        private void ResolveAssemblies()
        {
            _importers = new List<ImporterInfo>();
            _processors = new List<ProcessorInfo>();
            _writers = new List<Type>();

            // Finally load the pipeline assemblies.
            foreach (var assemblyPath in Assemblies)
            {
                Type[] exportedTypes;
                try
                {
                    Assembly a;
                    if (string.IsNullOrEmpty(assemblyPath))
                    {
                        // Get the types from this assembly, which includes all of the
                        // built-in importers, processors and type writers
                        a = Assembly.GetExecutingAssembly();
                        // The built-in types may not be public, so get all types
                        exportedTypes = a.GetTypes();
                    }
                    else
                    {
                        a = Assembly.LoadFrom(assemblyPath);
                        // We only look at public types for external importers, processors
                        // and type writers.
                        exportedTypes = a.GetExportedTypes();
                    }
                }
                catch (Exception e)
                {
                    Logger.LogWarning(null, null, "Failed to load assembly '{0}': {1}", assemblyPath, e.Message);
                    // The assembly failed to load... nothing
                    // we can do but ignore it.
                    continue;
                }

                foreach (var t in exportedTypes)
                {
                    if (!t.IsPublic || t.IsAbstract) 
                        continue;

                    if (t.GetInterface(@"IContentImporter") != null)
                    {
                        var attributes = t.GetCustomAttributes(typeof (ContentImporterAttribute), false);
                        if (attributes.Length != 0)
                        {
                            var importerAttribute = attributes[0] as ContentImporterAttribute;
                            _importers.Add(new ImporterInfo { attribue = importerAttribute, type = t });
                        }
                        else
                        {
                            // If no attribute specify default one
                            var importerAttribute = new ContentImporterAttribute(".*");
                            importerAttribute.DefaultProcessor = "";
                            importerAttribute.DisplayName = t.Name;
                            _importers.Add(new ImporterInfo { attribue = importerAttribute, type = t });
                        }
                    }
                    else if (t.GetInterface(@"IContentProcessor") != null)
                    {
                        var attributes = t.GetCustomAttributes(typeof (ContentProcessorAttribute), false);
                        if (attributes.Length != 0)
                        {
                            var processorAttribute = attributes[0] as ContentProcessorAttribute;
                            _processors.Add(new ProcessorInfo {attribue = processorAttribute, type = t});
                        }
                    }
                    else if (t.GetInterface(@"ContentTypeWriter") != null)
                    {
						// TODO: This doesn't work... how do i find these?
                        _writers.Add(t);
                    }
                }
            }
        }

        public Type[] GetImporterTypes()
        {
            if (_importers == null)
                ResolveAssemblies();

            List<Type> types = new List<Type>();

            foreach (var item in _importers) 
            {
                types.Add(item.type);
            }

            return types.ToArray();
        }

        public Type[] GetProcessorTypes()
        {
            if (_processors == null)
                ResolveAssemblies();
            
            List<Type> types = new List<Type>();
            
            foreach (var item in _processors) 
            {
                types.Add(item.type);
            }
            
            return types.ToArray();
        }

        public IContentImporter CreateImporter(string name)
        {
            if (_importers == null)
                ResolveAssemblies();

            // Search for the importer.
            foreach (var info in _importers)
            {
                if (info.type.Name.Equals(name))
                    return Activator.CreateInstance(info.type) as IContentImporter;
            }

            return null;
        }

        public string FindImporterByExtension(string ext)
        {
            if (_importers == null)
                ResolveAssemblies();

            // Search for the importer.
            foreach (var info in _importers)
            {
                if (info.attribue.FileExtensions.Any(e => e.Equals(ext, StringComparison.InvariantCultureIgnoreCase)))
                    return info.type.Name;
            }

            return null;
        }

        public string FindDefaultProcessor(string importer)
        {
            if (_importers == null)
                ResolveAssemblies();

            // Search for the importer.
            foreach (var info in _importers)
            {
                if (info.type.Name == importer)
                    return info.attribue.DefaultProcessor;
            }

            return null;
        }

        public Type GetProcessorType(string name)
        {
            if (_processors == null)
                ResolveAssemblies();

            // Search for the processor type.
            foreach (var info in _processors)
            {
                if (info.type.Name.Equals(name))
                    return info.type;
            }

            return null;
        }

        public void ResolveImporterAndProcessor(string sourceFilepath, ref string importerName, ref string processorName)
        {
            // Resolve the importer name.
            if (string.IsNullOrEmpty(importerName))
                importerName = FindImporterByExtension(Path.GetExtension(sourceFilepath));
            if (string.IsNullOrEmpty(importerName))
                throw new Exception(string.Format("Couldn't find a default importer for '{0}'!", sourceFilepath));

            // Resolve the processor name.
            if (string.IsNullOrEmpty(processorName))
                processorName = FindDefaultProcessor(importerName);
            if (string.IsNullOrEmpty(processorName))
                throw new Exception(string.Format("Couldn't find a default processor for importer '{0}'!", importerName));
        }

        public IContentProcessor CreateProcessor(string name, OpaqueDataDictionary processorParameters)
        {
            var processorType = GetProcessorType(name);
            if (processorType == null)
                return null;

            // Create the processor.
            var processor = (IContentProcessor)Activator.CreateInstance(processorType);

            // Convert and set the parameters on the processor.
            foreach (var param in processorParameters)
            {
                var propInfo = processorType.GetProperty(param.Key, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                if (propInfo == null || propInfo.GetSetMethod(false) == null)
                    continue;

                // If the property value is already of the correct type then set it.
                if (propInfo.PropertyType.IsInstanceOfType(param.Value))
                    propInfo.SetValue(processor, param.Value, null);
                else
                {
                    // Find a type converter for this property.
                    var typeConverter = TypeDescriptor.GetConverter(propInfo.PropertyType);
                    if (typeConverter.CanConvertFrom(param.Value.GetType()))
                    {
                        var propValue = typeConverter.ConvertFrom(null, CultureInfo.InvariantCulture, param.Value);
                        propInfo.SetValue(processor, propValue, null);
                    }
                }
            }

            return processor;
        }

        public OpaqueDataDictionary ValidateProcessorParameters(string name, OpaqueDataDictionary processorParameters)
        {
            var result = new OpaqueDataDictionary();

            var processorType = GetProcessorType(name);
            if (processorType == null || processorParameters == null)
            {
                return result;
            }

            foreach (var param in processorParameters)
            {
                var propInfo = processorType.GetProperty(param.Key, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                if (propInfo == null || propInfo.GetSetMethod(false) == null)
                    continue;

                // Make sure we can assign the value.
                if (!propInfo.PropertyType.IsInstanceOfType(param.Value))
                {
                    // Make sure we can convert the value.
                    var typeConverter = TypeDescriptor.GetConverter(propInfo.PropertyType);
                    if (!typeConverter.CanConvertFrom(param.Value.GetType()))
                        continue;
                }

                result.Add(param.Key, param.Value);
            }

            return result;
        }

        private void ResolveOutputFilepath(string sourceFilepath, ref string outputFilepath)
        {
            // If the output path is null... build it from the source file path.
            if (string.IsNullOrEmpty(outputFilepath))
            {
                var filename = Path.GetFileNameWithoutExtension(sourceFilepath) + ".xnb";
                var directory = PathHelper.GetRelativePath(ProjectDirectory,
                                                           Path.GetDirectoryName(sourceFilepath) +
                                                           Path.DirectorySeparatorChar);
                outputFilepath = Path.Combine(OutputDirectory, directory, filename);
            }
            else
            {
                // If the extension is not XNB or the source file extension then add XNB.
                var sourceExt = Path.GetExtension(sourceFilepath);
                if (outputFilepath.EndsWith(sourceExt, StringComparison.InvariantCultureIgnoreCase))
                    outputFilepath = outputFilepath.Substring(0, outputFilepath.Length - sourceExt.Length);
                if (!outputFilepath.EndsWith(".xnb", StringComparison.InvariantCultureIgnoreCase))
                    outputFilepath += ".xnb";

                // If the path isn't rooted then put it into the output directory.
                if (!Path.IsPathRooted(outputFilepath))
                    outputFilepath = Path.Combine(OutputDirectory, outputFilepath);
            }

            outputFilepath = PathHelper.Normalize(outputFilepath);
        }

        private PipelineBuildEvent LoadBuildEvent(string destFile, out string eventFilepath)
        {
            var contentPath = Path.ChangeExtension(PathHelper.GetRelativePath(OutputDirectory, destFile), PipelineBuildEvent.Extension);
            eventFilepath = Path.Combine(IntermediateDirectory, contentPath);
            return PipelineBuildEvent.Load(eventFilepath);
        }

        public PipelineBuildEvent BuildContent(string sourceFilepath, string outputFilepath = null, string importerName = null, string processorName = null, OpaqueDataDictionary processorParameters = null)
        {
            sourceFilepath = PathHelper.Normalize(sourceFilepath);
            ResolveOutputFilepath(sourceFilepath, ref outputFilepath);
            
            ResolveImporterAndProcessor(sourceFilepath, ref importerName, ref processorName);

            // Record what we're building and how.
            var contentEvent = new PipelineBuildEvent
            {
                SourceFile = PathHelper.Normalize(sourceFilepath),
                DestFile = outputFilepath,
                Importer = importerName,
                Processor = processorName,
                Parameters = ValidateProcessorParameters(processorName, processorParameters),
            };

            // Load the previous content event if it exists.
            string eventFilepath;
            var cachedEvent = LoadBuildEvent(contentEvent.DestFile, out eventFilepath);

            BuildContent(contentEvent, cachedEvent, eventFilepath);

            return contentEvent;
        }

        public void BuildContent(PipelineBuildEvent pipelineEvent, PipelineBuildEvent cachedEvent, string eventFilepath)
        {
            if (!File.Exists(pipelineEvent.SourceFile))
                throw new PipelineException("The source file '{0}' does not exist!", pipelineEvent.SourceFile);

            Logger.PushFile(pipelineEvent.SourceFile);            

            var rebuild = pipelineEvent.NeedsRebuild(cachedEvent);
            if (!rebuild)
            {
                // While this asset doesn't need to be rebuilt the dependent assets might.
                foreach (var asset in cachedEvent.BuildAsset)
                {
                    string assetEventFilepath;
                    var assetCachedEvent = LoadBuildEvent(asset, out assetEventFilepath);

                    // If we cannot find the cached event for the dependancy
                    // then we have to trigger a rebuild of the parent content.
                    if (assetCachedEvent == null)
                    {
                        rebuild = true;
                        break;
                    }

                    var depEvent = new PipelineBuildEvent
                    {
                        SourceFile = assetCachedEvent.SourceFile,
                        DestFile = assetCachedEvent.DestFile,
                        Importer = assetCachedEvent.Importer,
                        Processor = assetCachedEvent.Processor,
                        Parameters = assetCachedEvent.Parameters,
                    };

                    // Give the asset a chance to rebuild.                    
                    BuildContent(depEvent, assetCachedEvent, assetEventFilepath);
                }
            }

            // Do we need to rebuild?
            if (rebuild)
            {
                Logger.LogMessage("{0}", pipelineEvent.SourceFile);

                // Import and process the content.
                var processedObject = ProcessContent(pipelineEvent);

                // Write the content to disk.
                WriteXnb(processedObject, pipelineEvent);

                // Store the new event into the intermediate folder.
                pipelineEvent.Save(eventFilepath);
            }
            else
            {
                Logger.LogMessage("Skipping {0}", pipelineEvent.SourceFile);
            }

            Logger.PopFile();
        }

        public object ProcessContent(PipelineBuildEvent pipelineEvent)
        {
            if (!File.Exists(pipelineEvent.SourceFile))
                throw new PipelineException("The source file '{0}' does not exist!", pipelineEvent.SourceFile);

            // Store the last write time of the source file
            // so we can detect if it has been changed.
            pipelineEvent.SourceTime = File.GetLastWriteTime(pipelineEvent.SourceFile);

            // Make sure we can find the importer and processor.
            var importer = CreateImporter(pipelineEvent.Importer);
            if (importer == null)
                throw new PipelineException("Failed to create importer '{0}'", pipelineEvent.Importer);
            var processor = CreateProcessor(pipelineEvent.Processor, pipelineEvent.Parameters);
            if (processor == null)
                throw new PipelineException("Failed to create processor '{0}'", pipelineEvent.Processor);

            // Try importing the content.
            object importedObject;
            try
            {
                var importContext = new PipelineImporterContext(this);
                importedObject = importer.Import(pipelineEvent.SourceFile, importContext);
            }
            catch (PipelineException)
            {
                throw;
            }
            catch (Exception inner)
            {
                throw new PipelineException(string.Format("Importer '{0}' had unexpected failure!", pipelineEvent.Importer), inner);
            }

            // Make sure the input type is valid.
            if (!processor.InputType.IsAssignableFrom(importedObject.GetType()))
            {
                throw new PipelineException(
                    string.Format("The type '{0}' cannot be processed by {1} as a {2}!",
                    importedObject.GetType().FullName,
                    pipelineEvent.Processor,
                    processor.InputType.FullName));
            }

            // Process the imported object.
            object processedObject;
            try
            {
                var processContext = new PipelineProcessorContext(this, pipelineEvent);
                processedObject = processor.Process(importedObject, processContext);
            }
            catch (PipelineException)
            {
                throw;
            }
            catch (Exception inner)
            {
                throw new PipelineException(string.Format("Processor '{0}' had unexpected failure!", pipelineEvent.Processor), inner);
            }

            return processedObject;
        }

        public void CleanContent(string sourceFilepath, string outputFilepath = null)
        {
            // First try to load the event file.
            ResolveOutputFilepath(sourceFilepath, ref outputFilepath);
            string eventFilepath;
            var cachedEvent = LoadBuildEvent(outputFilepath, out eventFilepath);

            if (cachedEvent != null)
            {
                foreach (var asset in cachedEvent.BuildAsset)
                {
                    string assetEventFilepath;
                    var assetCachedEvent = LoadBuildEvent(asset, out assetEventFilepath);

                    if (assetCachedEvent == null)
                    {
                        Logger.LogMessage("Cleaning {0}", asset);
                        FileHelper.DeleteIfExists(asset);
                        FileHelper.DeleteIfExists(assetEventFilepath);
                        continue;
                    }

                    // Give the asset a chance to rebuild.                    
                    CleanContent(string.Empty, asset);
                }

                foreach (var asset in cachedEvent.BuildOutput)
                {
                    Logger.LogMessage("Cleaning {0}", asset);
                    FileHelper.DeleteIfExists(asset);
                }
            }

            Logger.LogMessage("Cleaning {0}", outputFilepath);
            FileHelper.DeleteIfExists(outputFilepath);
            FileHelper.DeleteIfExists(eventFilepath);
        }

        private void WriteXnb(object content, PipelineBuildEvent pipelineEvent)
        {
            // Make sure the output directory exists.
            var outputFileDir = Path.GetDirectoryName(pipelineEvent.DestFile);

            Directory.CreateDirectory(outputFileDir);

            if (_compiler == null)
                _compiler = new ContentCompiler();

            // Write the XNB.
            using (var stream = new FileStream(pipelineEvent.DestFile, FileMode.Create, FileAccess.Write, FileShare.None))
                _compiler.Compile(stream, content, Platform, Profile, false, OutputDirectory, outputFileDir);

            // Store the last write time of the output XNB here
            // so we can verify it hasn't been tampered with.
            pipelineEvent.DestTime = File.GetLastWriteTime(pipelineEvent.DestFile);
        }
    }
}
