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

		static bool UpgradeMonoGameProject (MonoDevelop.Core.IProgressMonitor monitor, MonoDevelop.Projects.SolutionEntityItem item, MSBuildProject project)
		{
			bool needsSave = false;
			bool isMonoGame = project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "MonoGamePlatform")) || project.ItemGroups.Any (x => x.Items.Any (i => i.Name == "Reference" && i.Include == "MonoGame.Framework"));
			bool containsMGCB = project.ItemGroups.Any (x => x.Items.Any (i => System.IO.Path.GetExtension (i.Include) == ".mgcb"));
			bool isApplication = project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "OutputType" && p.GetValue () == "Exe")) ||
				project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "AndroidApplication" && string.Compare (p.GetValue (), bool.TrueString, true)==0));
			if (isMonoGame && containsMGCB && isApplication) {
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
				if (!project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "MonoGamePlatform"))) {
					project.PropertyGroups.First ().SetPropertyValue ("MonoGamePlatform", platform, true);
					needsSave = true;
				}
				if (!project.PropertyGroups.Any (x => x.Properties.Any (p => p.Name == "MonoGameInstallDirectory"))) {
					var e = project.Document.DocumentElement;
					var manager = new XmlNamespaceManager (new NameTable ());
					var schema = "http://schemas.microsoft.com/developer/msbuild/2003";
					manager.AddNamespace ("tns", schema);
					var node = e.SelectSingleNode ("tns:PropertyGroup", manager);
					if (node != null) {
						var a = project.Document.CreateElement ("MonoGameInstallDirectory", schema);
						a.InnerText = "$(MSBuildProgramFiles32)";
						a.SetAttribute ("Condition", "'$(OS)' != 'Unix' ");
						node.AppendChild (a);
						var b = project.Document.CreateElement ("MonoGameInstallDirectory", schema);
						b.InnerText = "$(MSBuildExtensionsPath)";
						b.SetAttribute ("Condition", "'$(OS)' == 'Unix' ");
						node.InsertAfter (b, a);
						needsSave = true;
					}
				}
				if (containsMGCB) {
					var ritems = new List<MSBuildItem> ();
					foreach (var ig in project.ItemGroups) {
						foreach (var i in ig.Items.Where (x => System.IO.Path.GetExtension (x.Include) == ".mgcb")) {
							if (i.Name != "MonoGameContentReference" && i.Name != "None") {
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
				if (!project.Imports.Any (x => x.Project.StartsWith (MonoGameContentBuildTargets, StringComparison.OrdinalIgnoreCase))) {
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

