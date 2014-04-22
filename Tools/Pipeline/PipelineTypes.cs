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
using MonoGame.Framework.Content.Pipeline.Builder;

namespace MonoGame.Tools.Pipeline
{
    [DebuggerDisplay("{TypeName}")]
    public class ImporterTypeDescription
    {
        public string TypeName;
        public string DisplayName;
        public string DefaultProcessor;        
        public IEnumerable<string> FileExtensions;
    };

    [DebuggerDisplay("{TypeName}")]
    public class ProcessorTypeDescription
    {                
        public struct Property
        {
            public string Name;
            public Type Type;
            public object DefaultValue;
        }

        public string TypeName;
        public string DisplayName;
        public Property[] Properties;
    };

    internal class PipelineTypes
    {
        [DebuggerDisplay("ImporterInfo: {Type.Name}")]
        private struct ImporterInfo
        {
            public ContentImporterAttribute Attribute;
            public Type Type;
        }

        [DebuggerDisplay("ProcessorInfo: {Type.Name}")]
        private struct ProcessorInfo
        {
            public ContentProcessorAttribute Attribute;
            public Type Type;
        }

        private static List<ImporterInfo> _importers;
        private static List<ProcessorInfo> _processors;

        private List<Type> _writers;

        public string ProjectDirectory { get; private set; }
        public string OutputDirectory { get; private set; }
        public string IntermediateDirectory { get; private set; }

        public static ContentBuildLogger Logger { get; set; }

        public static List<string> Assemblies { get; private set; }

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

        public static ImporterTypeDescription[] Importers { get; private set; }
        public static ProcessorTypeDescription[] Processors { get; private set; }        

        public static void Load(PipelineProject project)
        {
            Assemblies = new List<string>();
            Logger = new PipelineBuildLogger();

            var dir = Path.GetDirectoryName(project.FilePath);
            //var ProjectDirectory = PathHelper.NormalizeDirectory(dir);
            //OutputDirectory = PathHelper.NormalizeDirectory(Path.Combine(dir, project.OutputDir));
            //IntermediateDirectory = PathHelper.NormalizeDirectory(Path.Combine(dir, project.IntermediateDir));

            foreach (var i in project.References)
            {
                var assemblyFilePath = Path.Combine(dir, i);

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
                }
            }

            ResolveAssemblies();

            var importerDescriptions = new ImporterTypeDescription[_importers.Count];
            var cur = 0;
            foreach (var item in _importers)
            {
                var desc = new ImporterTypeDescription()
                    {
                        TypeName = item.Type.Name,
                        DisplayName = item.Attribute.DisplayName,
                        DefaultProcessor = item.Attribute.DefaultProcessor,                        
                        FileExtensions = item.Attribute.FileExtensions,                        
                    };
                importerDescriptions[cur] = desc;
                cur++;
            }

            Importers = importerDescriptions;

            var processorDescriptions = new ProcessorTypeDescription[_processors.Count];

            cur = 0;
            foreach (var item in _processors)
            {
                var obj = Activator.CreateInstance(item.Type);
                var typeProperties = item.Type.GetRuntimeProperties();
                var properties = new List<ProcessorTypeDescription.Property>();
                foreach (var i in typeProperties)
                {
                    // TODO:
                    //p.GetCustomAttribute(typeof(ContentPipelineIgnore))

                    var p = new ProcessorTypeDescription.Property()
                        {
                            Name = i.Name,
                            Type = i.PropertyType,
                            DefaultValue = i.GetValue(obj),
                        };
                    properties.Add(p);
                }
                
                var desc = new ProcessorTypeDescription()
                {
                    TypeName = item.Type.Name,
                    DisplayName = item.Attribute.DisplayName,
                    Properties = properties.ToArray(),                    
                };
                if (string.IsNullOrEmpty(desc.DisplayName))
                    desc.DisplayName = desc.TypeName;

                processorDescriptions[cur] = desc;
                cur++;
            }

            Processors = processorDescriptions;

            foreach (var i in project.ContentItems)
            {
                i.ResolveTypes();
            }
        }

        public void Unload()
        {
            
        }

        public static ImporterTypeDescription FindImporter(string name, string fileExtension)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (var i in Importers)
                {
                    if (i.TypeName.Equals(name))
                        return i;
                }

                foreach (var i in Importers)
                {
                    if (i.DisplayName.Equals(name))
                        return i;
                }

                return null;
            }

            foreach (var i in Importers)
            {
                if (i.FileExtensions.Contains(fileExtension))
                    return i;
            }

            return null;
        }

        public static ProcessorTypeDescription FindProcessor(string name, ImporterTypeDescription importer)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (var i in Processors)
                {
                    if (i.TypeName.Equals(name))
                        return i;
                }

                return null;
            }

            foreach (var i in Processors)
            {
                if (i.TypeName.Equals(importer.DefaultProcessor))
                    return i;
            }

            return null;
        }

        private static void ResolveAssemblies()
        {
            _importers = new List<ImporterInfo>();
            _processors = new List<ProcessorInfo>();
            
            var assemblies = new List<Assembly>(AppDomain.CurrentDomain.GetAssemblies());

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var types = asm.GetTypes();
                    ProcessTypes(types);
                }
                catch (Exception e)
                {
                    // ??
                }
            }

            foreach (var assemblyPath in Assemblies)
            {
                Type[] types;
                try
                {
                    var a = Assembly.LoadFrom(assemblyPath);
                    types = a.GetExportedTypes();
                    ProcessTypes(types);
                }
                catch (Exception e)
                {
                    Logger.LogWarning(null, null, "Failed to load assembly '{0}': {1}", assemblyPath, e.Message);
                    // The assembly failed to load... nothing
                    // we can do but ignore it.
                    continue;
                }                
            }
        }

        private static void ProcessTypes(IEnumerable<Type> types)
        {
            foreach (var t in types)
            {
                if (t.IsAbstract)
                    continue;

                if (t.GetInterface(@"IContentImporter") != null)
                {
                    var attributes = t.GetCustomAttributes(typeof(ContentImporterAttribute), false);
                    if (attributes.Length != 0)
                    {
                        var importerAttribute = attributes[0] as ContentImporterAttribute;
                        _importers.Add(new ImporterInfo { Attribute = importerAttribute, Type = t });
                    }
                    else
                    {
                        // If no attribute specify default one
                        var importerAttribute = new ContentImporterAttribute(".*");
                        importerAttribute.DefaultProcessor = "";
                        importerAttribute.DisplayName = t.Name;
                        _importers.Add(new ImporterInfo { Attribute = importerAttribute, Type = t });
                    }
                }
                else if (t.GetInterface(@"IContentProcessor") != null)
                {
                    var attributes = t.GetCustomAttributes(typeof(ContentProcessorAttribute), false);
                    if (attributes.Length != 0)
                    {
                        var processorAttribute = attributes[0] as ContentProcessorAttribute;
                        _processors.Add(new ProcessorInfo { Attribute = processorAttribute, Type = t });
                    }
                }
            }
        }
    }
}
