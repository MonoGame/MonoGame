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

namespace MonoGame.Framework.Content.Pipeline.Builder
{
    public class PipelineManager
    {
        private struct ImporterInfo
        {
            public ContentImporterAttribute attribue;
            public Type type;
        };

        private List<ImporterInfo> _importers;

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

        public PipelineBuildLogger Logger { get; private set; }

        public List<string> Assemblies { get; private set; }

        public PipelineManager(string projectDir, string outputDir, string intermediateDir)
        {
            Assemblies = new List<string>();
            Assemblies.Add(null);
            Logger = new PipelineBuildLogger();

            ProjectDirectory = projectDir + @"\";
            OutputDirectory = outputDir + @"\";
            IntermediateDirectory = intermediateDir + @"\";
        }

        public void AddAssembly(string assemblyFilePath)
        {
            if (assemblyFilePath == null)
                throw new NullReferenceException("assemblyFilePath cannot be null!");
            if (!Path.IsPathRooted(assemblyFilePath))
                throw new NullReferenceException("assemblyFilePath must be absolute!");

            // Make sure we're not adding the same assembly twice.
            assemblyFilePath = PathHelper.Normalize(assemblyFilePath);
            if (!Assemblies.Contains(assemblyFilePath))
                Assemblies.Add(assemblyFilePath);
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
                catch (Exception)
                {
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
                if (info.attribue.FileExtensions.Contains(ext))
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

        public IContentProcessor CreateProcessor(string name, OpaqueDataDictionary processorParameters)
        {
            if (_processors == null)
                ResolveAssemblies();

            // Search for the processor.
            IContentProcessor processor = null;
            foreach (var info in _processors)
            {
                if (info.type.Name.Equals(name))
                {
                    processor = (IContentProcessor)Activator.CreateInstance(info.type);
                    break;
                }
            }

            // No processor found... exception?
            if (processor == null)
                return null;

            // Convert and set the parameters on the processor.
            var processorType = processor.GetType();
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

        public PipelineBuildEvent BuildContent(string sourceFilepath, string outputFilepath = null, string importerName = null, string processorName = null, OpaqueDataDictionary processorParameters = null)
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

            // Resolve the importer name.
            if (string.IsNullOrEmpty(importerName))
                importerName = FindImporterByExtension(Path.GetExtension(sourceFilepath));

            // Resolve the processor name.
            if (string.IsNullOrEmpty(processorName))
                processorName = FindDefaultProcessor(importerName);

            // Record what we're building and how.
            var contentEvent = new PipelineBuildEvent
            {
                SourceFile = sourceFilepath,
                DestFile = outputFilepath,
                Importer = importerName,
                Processor = processorName,
                Parameters = processorParameters ?? new OpaqueDataDictionary(),
            };

            // Load the previous content event if it exists.
            var contentPath = Path.ChangeExtension(PathHelper.GetRelativePath(OutputDirectory, contentEvent.DestFile), ".content");
            var eventFilepath = Path.Combine(IntermediateDirectory, contentPath);
            var cachedEvent = PipelineBuildEvent.Load(eventFilepath);

            BuildContent(contentEvent, cachedEvent, eventFilepath);

            return contentEvent;
        }

        public void BuildContent(PipelineBuildEvent pipelineEvent, PipelineBuildEvent cachedEvent, string eventFilepath)
        {
            var rebuild = pipelineEvent.NeedsRebuild(cachedEvent);
            if (!rebuild)
            {
                // While this asset doesn't need to be rebuilt the dependent assets might.
                foreach (var asset in cachedEvent.BuildAsset)
                {                    
                    var assetPath = Path.ChangeExtension(PathHelper.GetRelativePath(OutputDirectory, asset), ".content");
                    var assetEventFilepath = Path.Combine(IntermediateDirectory, assetPath);
                    var assetCachedEvent = PipelineBuildEvent.Load(assetEventFilepath);

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
                // Make sure we can find the importer and processor.
                var importer = CreateImporter(pipelineEvent.Importer);
                var processor = CreateProcessor(pipelineEvent.Processor, pipelineEvent.Parameters);
                if (importer == null || processor == null)
                {
                    // TODO: Log error?
                    return;
                }

                // Try importing the content.
                object importedObject;
                try
                {
                    var importContext = new PipelineImporterContext(this);
                    importedObject = importer.Import(pipelineEvent.SourceFile, importContext);
                }
                catch (Exception)
                {
                    // TODO: Log error?
                    return;
                }

                // Process the imported object.
                object processedObject;
                try
                {
                    var processContext = new PipelineProcessorContext(this, pipelineEvent);
                    processedObject = processor.Process(importedObject, processContext);
                }
                catch (Exception)
                {
                    // TODO: Log error?
                    return;                   
                }

                // Write the content to disk.
                WriteXnb(processedObject, pipelineEvent);

                // Store the new event into the intermediate folder.
                pipelineEvent.Save(eventFilepath);
            }
        }

        private void WriteXnb(object content, PipelineBuildEvent pipelineEvent)
        {
            // Make sure the output directory exists.
            var outputFileDir = Path.GetDirectoryName(pipelineEvent.DestFile) + @"\";
            Directory.CreateDirectory(outputFileDir);

            // TODO: For now use XNA's ContentCompiler which knows 
            // how to write XNBs for us.
            //
            // http://xboxforums.create.msdn.com/forums/t/72563.aspx
            //
            // We need to replace this with our own implementation
            // that isn't all internal methods!
            //
            if (_compiler == null)
            {
                var ctor = typeof(ContentCompiler).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)[0];
                _compiler = (ContentCompiler)ctor.Invoke(new object[] { });
                _compileMethod = typeof(ContentCompiler).GetMethod("Compile", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            }

            // Write the XNB.
            using (var stream = new FileStream(pipelineEvent.DestFile, FileMode.Create, FileAccess.Write, FileShare.None))
                _compileMethod.Invoke(_compiler, new[] { stream, content, TargetPlatform.Windows, GraphicsProfile.Reach, false, OutputDirectory, outputFileDir });

            // Store the last write time of the output XNB here
            // so we can verify it hasn't been tampered with.
            pipelineEvent.DestTime = File.GetLastWriteTime(pipelineEvent.DestFile);
        }
    }
}
