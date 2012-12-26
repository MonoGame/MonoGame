using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Xna.Framework.Content.Pipeline;
using MonoGame.Framework.Content.Pipeline.Builder;

namespace MGCB
{
    public class ContentProject
    {
        private ProjectRootElement _project;

        private PipelineManager _manager;

        public ContentProject(string projectPath, string outputDirectoryName)
        {
            _project = ProjectRootElement.Open(projectPath);

            var projectDirectory = _project.DirectoryPath;
            var outputPath = projectDirectory + @"\bin\" + outputDirectoryName;
            var intermediatePath = projectDirectory + @"\obj\" + outputDirectoryName;

            _manager = new PipelineManager(projectDirectory, outputPath, intermediatePath);

            var references = _project.Items.Where(e => e.ItemType == "Reference");
            foreach (var r in references)
            {
                var hintPath = r.Metadata.FirstOrDefault(e => e.Name == "HintPath");
                if (hintPath != null)
                    _manager.AddAssembly(hintPath.Value);
                else
                    _manager.AddAssembly(r.Include);
            }
        }

        public void Build(string target)
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
