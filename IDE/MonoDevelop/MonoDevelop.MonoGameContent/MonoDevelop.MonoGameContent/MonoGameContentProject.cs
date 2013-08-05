using System;
using MonoDevelop.Projects;
using System.Reflection;
using System.Collections.Generic;
using MonoDevelop.Core.Serialization;
using System.Xml;
using MonoDevelop.Core.Assemblies;
using MonoGame.Framework.Content.Pipeline.Builder;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoDevelop.MonoGameContent
{
	public class MonoGameContentProject : DotNetProject
	{
		private PipelineManager manager; 

		public PipelineManager Manager {
			get { return manager;}
		}

		public MonoGameContentProject ()
		{
			Init ();
		}

		public MonoGameContentProject (string languageName)
			: base (languageName)
		{
			Init ();
		}

		public MonoGameContentProject (string languageName, ProjectCreateInformation info, XmlElement projectOptions)
			: base (languageName, info, projectOptions)
		{
			Init ();
		}

		private void Init()
		{
			 manager = new PipelineManager(this.BaseDirectory.FullPath,
			                                  "",
			                                  "");
		}

		protected override void OnDefaultConfigurationChanged (ConfigurationEventArgs args)
		{
			base.OnDefaultConfigurationChanged (args);
		}

		public override SolutionItemConfiguration CreateConfiguration (string name)
		{
			var conf = new MonoGameContentProjectConfiguration (name);
			conf.CopyFrom (base.CreateConfiguration (name));
			return conf;
		}

		public override string GetDefaultBuildAction (string fileName)
		{
			return "Compile";
			//return base.GetDefaultBuildAction (fileName);
		}

		public override bool SupportsFormat (FileFormat format)
		{
			return format.Id == "MSBuild10";
		}

		public override TargetFrameworkMoniker GetDefaultTargetFrameworkForFormat (FileFormat format)
		{
			return new TargetFrameworkMoniker("4.0");
		}

		public override bool SupportsFramework (MonoDevelop.Core.Assemblies.TargetFramework framework)
		{
			if (!framework.IsCompatibleWithFramework (MonoDevelop.Core.Assemblies.TargetFrameworkMoniker.NET_4_0))
				return false;
			else
				return base.SupportsFramework (framework);
		}

		public bool IsProcessorNameValid (string str)
		{
			return true;
		}

		public System.Collections.ICollection GetProcessorNames ()
		{
			List<string> types = new List<string> ();
			foreach (var t in manager.GetProcessorTypes ()) {
				types.Add (t.Name);
			}
			return types.ToArray ();
		}

		public bool IsImporterNameValid (string str)
		{
			return true;
		}

		public System.Collections.ICollection GetImporterNames ()
		{
			List<string> types = new List<string> ();
			foreach (var t in manager.GetImporterTypes ()) {
				types.Add (t.Name);
			}
			return types.ToArray ();
		}

		protected override void OnItemsAdded (IEnumerable<ProjectItem> objs)
		{
			base.OnItemsAdded (objs);
			foreach (var item in objs) {
				var file = item as ProjectFile;
				if (file != null) {
					if (file.ExtendedProperties["Importer"] == null)
						file.ExtendedProperties ["Importer"] = manager.FindImporterByExtension(System.IO.Path.GetExtension(file.Name));
					if (file.ExtendedProperties["Processor"] == null)
						file.ExtendedProperties ["Processor"] = manager.FindDefaultProcessor ((string)file.ExtendedProperties ["Importer"]);
					if (file.ExtendedProperties["Name"] == null)
						file.ExtendedProperties ["Name"] = System.IO.Path.GetFileNameWithoutExtension (file.Name);
				}
			}
		}

		protected override void OnReferenceAddedToProject (ProjectReferenceEventArgs e)
		{
			try {
				if (manager != null) {
					manager.AddAssembly (e.ProjectReference.OwnerProject.GetOutputFileName(this.DefaultConfiguration.Selector).FullPath);
				}
			}
			catch {
			}
			base.OnReferenceAddedToProject (e);
		}
		
	}

	public class MonoGameContentProjectBinding : IProjectBinding
	{
		public Project CreateProject (ProjectCreateInformation info, System.Xml.XmlElement projectOptions)
		{ 
			string lang = projectOptions.GetAttribute ("language");
			return new MonoGameContentProject (lang, info, projectOptions);
		}

		public Project CreateSingleFileProject (string sourceFile)
		{
			throw new InvalidOperationException ();
		}

		public bool CanCreateSingleFileProject (string sourceFile)
		{
			return false;
		}

		public string Name {
			get { return "MonoGameContent"; }
		}
	}

	public class MonoGameContentProjectConfiguration : DotNetProjectConfiguration
	{
		public MonoGameContentProjectConfiguration () : base ()
		{
			monoGamePlatform = "Windows";
		}

		public MonoGameContentProjectConfiguration (string name) : base (name)
		{
			monoGamePlatform = "Windows";
		}		

		public override void CopyFrom (ItemConfiguration configuration)
		{
			base.CopyFrom (configuration);
			var cfg = configuration as MonoGameContentProjectConfiguration;
			if (cfg == null)
				return;

			monoGamePlatform = cfg.MonoGamePlatform;
			xnaCompressContent = cfg.XnaCompressContent;
		}

		[ItemProperty ("MonoGamePlatform", DefaultValue="Windows")]
		string monoGamePlatform;

		[ItemProperty ("XnaCompressContent", DefaultValue=false)]
		string xnaCompressContent = (false).ToString();

		public string MonoGamePlatform {
			get { return monoGamePlatform;}
			set {
				if (monoGamePlatform != value) {
					monoGamePlatform = value;
				}
			}
		}

		public string XnaCompressContent {
			get { return xnaCompressContent;}
			set {
				if (xnaCompressContent != value) {
					xnaCompressContent = value;
				}
			}
		}

	}

}
