using System;
using MonoDevelop.Projects;
using System.Xml;
using MonoDevelop.Core.Assemblies;

namespace MonoDevelop.MonoGame
{
	public class MonoGameProject :  DotNetAssemblyProject
	{
		public MonoGameProject ()
		{
			Init ();
		}
		
		public MonoGameProject (string languageName)
			: base (languageName)
		{
			Init ();
		}
		
		public MonoGameProject (string languageName, ProjectCreateInformation info, XmlElement projectOptions)
			: base (languageName, info, projectOptions)
		{
			Init ();
		}
		
		private void Init()
		{
		}
		
		public override SolutionItemConfiguration CreateConfiguration (string name)
		{
			var conf = new MonoGameProjectConfiguration (name);
			conf.CopyFrom (base.CreateConfiguration (name));
			return conf;
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

		protected override void PopulateSupportFileList (MonoDevelop.Projects.FileCopySet list, ConfigurationSelector solutionConfiguration)
		{
			base.PopulateSupportFileList (list, solutionConfiguration);

			//HACK: workaround for MD not local-copying package references
			foreach (var projectReference in References) {
				if (projectReference.Package != null && projectReference.Package.Name == "monogame") {
					if (projectReference.ReferenceType == ReferenceType.Gac) {
						foreach (var assem in projectReference.Package.Assemblies) {
							list.Add (assem.Location);
							var cfg = (MonoGameProjectConfiguration)solutionConfiguration.GetConfiguration (this);
							if (cfg.DebugMode) {
								var mdbFile = TargetRuntime.GetAssemblyDebugInfoFile (assem.Location);
								if (System.IO.File.Exists (mdbFile))
									list.Add (mdbFile);
							}
						}
					}
					break;
				}
			}
		}
	}
	
	public class MonoGameProjectBinding : IProjectBinding
	{
		public Project CreateProject (ProjectCreateInformation info, System.Xml.XmlElement projectOptions)
		{
			string lang = projectOptions.GetAttribute ("language");
			return new MonoGameProject (lang, info, projectOptions);
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
			get { return "MonoGame"; }
		}
	}
	
	public class MonoGameProjectConfiguration : DotNetProjectConfiguration
	{
		public MonoGameProjectConfiguration () : base ()
		{
		}
		
		public MonoGameProjectConfiguration (string name) : base (name)
		{
		}
		
		public override void CopyFrom (ItemConfiguration configuration)
		{
			base.CopyFrom (configuration);
		}
	}
}

