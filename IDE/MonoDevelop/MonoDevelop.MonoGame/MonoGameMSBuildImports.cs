using System;
using System.Xml;
using System.Xml.XPath;
using System.Linq;
using MonoDevelop.Projects.Formats.MSBuild;
using System.Collections.Generic;

namespace MonoDevelop.MonoGame
{
	public class MonoGameMSBuildImports : MSBuildExtension
	{
		const string MonoGameContentBuildTargets = "$(MSBuildExtensionsPath)\\MonoGame\\v3.0\\MonoGame.Content.Builder.targets";
		const string MonoGameCommonProps = "$(MSBuildExtensionsPath)\\MonoGame\\v3.0\\MonoGame.Common.props";

		static bool UpgradeMonoGameProject (MonoDevelop.Core.IProgressMonitor monitor, MonoDevelop.Projects.SolutionEntityItem item, MSBuildProject project)
		{
			bool needsSave = false;
			bool containsMGCB = project.ItemGroups.Any (x => x.Items.Any (i => System.IO.Path.GetExtension (i.Include) == ".mgcb"));
			bool isMonoGame = project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "MonoGamePlatform")) ||
				project.ItemGroups.Any (x => x.Items.Any (i => i.Name == "Reference" && i.Include == "MonoGame.Framework")) ||
				containsMGCB;
			bool isApplication = project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "OutputType" && p.GetValue () == "Exe")) ||
				project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "AndroidApplication" && string.Compare (p.GetValue (), bool.TrueString, true)==0));
			bool isShared = project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "HasSharedItems" && p.GetValue () == "true"));
			var type = item.GetType ().Name;
			var platform = Environment.OSVersion.Platform == PlatformID.Win32NT ? "Windows" : "DesktopGL";
			switch (type) {
			case "XamarinIOSProject":
				platform = "iOS";
				break;
			case "MonoDroidProject":
				platform = "Android";
				break;
			case "MonoGameProject":
				platform = "DesktopGL";
				break;
			}
			if (isMonoGame) {
				var ritems = new List<MSBuildItem> ();
				foreach (var ig in project.ItemGroups) {
					foreach (var i in ig.Items.Where (x => x.Name == "Reference" && x.Include == "MonoGame.Framework")) {
						if (!i.HasMetadata ("HintPath")) {
							monitor.Log.WriteLine ("Fixing {0} to be MonoGameContentReference", i.Include);
							var a = ig.AddNewItem ("Reference", i.Include);
							a.SetMetadata ("HintPath", string.Format (@"$(MonoGameInstallDirectory)\MonoGame\v3.0\Assemblies\{0}\MonoGame.Framework.dll", platform));
							ritems.Add (i);
							needsSave = true;
						}
					}
				}
				foreach (var a in ritems) {
					project.RemoveItem (a);
				}
			}
			if (isMonoGame && containsMGCB && (isApplication || isShared)) {
				if (!project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "MonoGamePlatform")) && !isShared) {
					monitor.Log.WriteLine ("Adding MonoGamePlatform", platform);
					project.PropertyGroups.First ().SetPropertyValue ("MonoGamePlatform", platform, true);
					needsSave = true;
				}
				if (!project.Imports.Any (x => x.Project.StartsWith (MonoGameCommonProps, StringComparison.OrdinalIgnoreCase))&& !isShared) {
					monitor.Log.WriteLine ("Adding MonoGame.Common.props Import");
					var e = project.Document.DocumentElement;
					var manager = new XmlNamespaceManager (new NameTable ());
					var schema = "http://schemas.microsoft.com/developer/msbuild/2003";
					manager.AddNamespace ("tns", schema);
					var import = project.Document.CreateElement ("Import", schema);
					import.SetAttribute ("Project", MonoGameCommonProps);
					import.SetAttribute ("Condition", string.Format ("Exists('{0}')", MonoGameCommonProps));
					project.Document.DocumentElement.InsertBefore (import, project.Document.DocumentElement.FirstChild);
					needsSave = true;
				}
				if (containsMGCB) {
					var ritems = new List<MSBuildItem> ();
					foreach (var ig in project.ItemGroups) {
						foreach (var i in ig.Items.Where (x => System.IO.Path.GetExtension (x.Include) == ".mgcb")) {
							if (i.Name != "MonoGameContentReference" && i.Name == "None") {
								monitor.Log.WriteLine ("Fixing {0} to be MonoGameContentReference", i.Include);
								ig.AddNewItem ("MonoGameContentReference", i.Include);
								ritems.Add (i);
								needsSave = true;
							}
						}
					}
					foreach (var a in ritems) {
						project.RemoveItem (a);
					}
				}
				if (!project.Imports.Any (x => x.Project.StartsWith (MonoGameContentBuildTargets, StringComparison.OrdinalIgnoreCase)) && !isShared) {
					monitor.Log.WriteLine ("Adding MonoGame Content Builder .targets");
					project.AddNewImport (MonoGameContentBuildTargets);
					needsSave = true;
				}
			}
			return needsSave;
		}

		public override void LoadProject (MonoDevelop.Core.IProgressMonitor monitor, MonoDevelop.Projects.SolutionEntityItem item, MSBuildProject project)
		{
			
			if (UpgradeMonoGameProject (monitor, item, project))
				project.Save (project.FileName);
			base.LoadProject (monitor, item, project);
		}

		public override void SaveProject (MonoDevelop.Core.IProgressMonitor monitor, MonoDevelop.Projects.SolutionEntityItem item, MSBuildProject project)
		{
			UpgradeMonoGameProject (monitor, item, project);
			base.SaveProject (monitor, item, project);
		}
	}
}

