// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Construction;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace MGCB
{
    public class ContentProject
    {
        private readonly ProjectRootElement _project;

        private readonly PipelineManager _manager;

        public ContentProject(string projectPath, string configName)
        {
            _project = ProjectRootElement.Open(projectPath);

            var projectDirectory = _project.DirectoryPath;
            var outputPath = projectDirectory + @"\bin\" + configName;
            var intermediatePath = projectDirectory + @"\obj\" + configName;

            _manager = new PipelineManager(projectDirectory, outputPath, intermediatePath);

            // Find the XNA assembly path.
            var xnaGs = Environment.GetEnvironmentVariable("XNAGSv4");
            if (xnaGs == null)
                xnaGs = projectDirectory;
            else
                xnaGs = Path.Combine(xnaGs, @"References\Windows\x86");

            // Add the references.
            var references = _project.Items.Where(e => e.ItemType == "Reference");
            foreach (var r in references)
            {
                var hintPath = r.Metadata.FirstOrDefault(e => e.Name == "HintPath");
                if (hintPath != null)
                {
                    var filePath = hintPath.Value;

                    // Resolve the path to absolute.
                    if (!Path.IsPathRooted(filePath))
                        filePath = Path.GetFullPath(Path.Combine(projectDirectory, filePath));

                    _manager.AddAssembly(filePath);
                }
                else
                {
                    var assemblyName = r.Include;

                    try
                    {
                        // First try to use reflecation loading which should
                        // find the assembly if it is in the the GAC.
                        var filePath = Assembly.ReflectionOnlyLoad(assemblyName).Location;
                        _manager.AddAssembly(filePath);
                    }
                    catch (Exception)
                    {
                        // If this is an XNA assembly then resolve it to 
                        // the known XNA assembly path.                        
                        if (assemblyName.StartsWith("Microsoft.Xna.Framework."))
                        {
                            // We don't know where the assembly is 
                            // located, so try the XNA install path.
                            var filePath = Path.Combine(xnaGs, assemblyName.Split(',')[0] + ".dll");
                            _manager.AddAssembly(filePath);
                        }
                    }
                }
            }
        }

        public void Build()
        {
            var compile = _project.Items.Where(e => e.ItemType == "Compile");
            foreach (var c in compile)
            {
                var sourceFile = Path.Combine(_project.DirectoryPath, c.Include);
                var importer = c.Metadata.FirstOrDefault(e => e.Name == "Importer");
                var processor = c.Metadata.FirstOrDefault(e => e.Name == "Processor");

                var processorParameters = new OpaqueDataDictionary();
                const string propertyPrefix = "ProcessorParameters_";
                foreach (var meta in c.Metadata)
                {
                    if (!meta.Name.StartsWith(propertyPrefix))
                        continue;

                    var propName = meta.Name.Substring(propertyPrefix.Length);
                    processorParameters.Add(propName, meta.Value);
                }

                _manager.BuildContent(sourceFile, 
                    null,
                    importer != null ? importer.Value : null, 
                    processor != null ? processor.Value : null,
                    processorParameters);
            }
        }
    }
}
