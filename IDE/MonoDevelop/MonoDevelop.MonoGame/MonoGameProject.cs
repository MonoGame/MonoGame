using System;
using MonoDevelop.Projects.MSBuild;
using MonoDevelop.Core.Assemblies;
using System.Xml;
using MonoDevelop.Projects;

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
			return format.Id == "MSBuild12";
		}

		public override TargetFrameworkMoniker GetDefaultTargetFrameworkForFormat (FileFormat format)
		{
			return new TargetFrameworkMoniker("4.0");
		}

		public override bool SupportsFramework (MonoDevelop.Core.Assemblies.TargetFramework framework)
		{
			if (!framework.CanReferenceAssembliesTargetingFramework (MonoDevelop.Core.Assemblies.TargetFrameworkMoniker.NET_4_0))
				return false;
			else
				return base.SupportsFramework (framework);
		}
	}

	public class MonoGameProjectBinding : IProjectBinding
	{
		public Project CreateProject (ProjectCreateInformation info, XmlElement projectOptions)
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

