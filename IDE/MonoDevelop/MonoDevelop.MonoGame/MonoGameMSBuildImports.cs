using System;
using System.Xml;
using System.Xml.XPath;
using System.Linq;
using MonoDevelop.Projects.Formats.MSBuild;
using System.Collections.Generic;
using MonoDevelop.Projects;

namespace MonoDevelop.MonoGame
{
	public class MonoGameMSBuildImports : MSBuildExtension
	{
		const string MonoGameContentBuildTargets = "$(MSBuildExtensionsPath)\\MonoGame\\v3.0\\MonoGame.Content.Builder.targets";
		const string MonoGameCommonProps = "$(MSBuildExtensionsPath)\\MonoGame\\v3.0\\MonoGame.Common.props";
		const string MonoGameExtensionsPath = @"$(MonoGameInstallDirectory)\MonoGame\v3.0\Assemblies\{0}\{1}";
		const string MonoGameExtensionsAbsolutePath = @"/Library/Frameworks/MonoGame.framework/v3.0/Assemblies/{0}/{1}";

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
			monitor.Log.WriteLine ("Found {0}", type);
			var platform = Environment.OSVersion.Platform == PlatformID.Win32NT ? "Windows" : "DesktopGL";
			var path = MonoGameExtensionsPath;
			switch (type) {
			case "XamarinIOSProject":
				platform = "iOS";
				break;
			case "MonoDroidProject":
				platform = "Android";
				break;
			case "XamMac2Project":
			case "MonoGameProject":
				platform = "DesktopGL";
				break;
			case "XamMac":
			case "XamMacProject":
				platform = "DesktopGL";
				// Xamarin.Mac Classic does not use MSBuild so we need to absolute path.
				path = MonoGameExtensionsAbsolutePath;
				break;
			case "MonoMac":
			case "MonoMacProject":
				platform = "MacOSX";
				// Xamarin.Mac Classic does not use MSBuild so we need to absolute path.
				path = MonoGameExtensionsAbsolutePath;
				break;
			}
			monitor.Log.WriteLine ("Platform = {0}", platform);
			monitor.Log.WriteLine ("Path = {0}", path);
			monitor.Log.WriteLine ("isMonoGame {0}", isMonoGame);
			if (isMonoGame) {
				var ritems = new List<MSBuildItem> ();
				foreach (var ig in project.ItemGroups) {
					foreach (var i in ig.Items.Where (x => x.Name == "Reference" && x.Include == "MonoGame.Framework")) {
						if (!i.HasMetadata ("HintPath")) {
							monitor.Log.WriteLine ("Fixing {0} to be MonoGameContentReference", i.Include);
							var a = ig.AddNewItem ("Reference", i.Include);
							a.SetMetadata ("HintPath", string.Format (path, platform, "MonoGame.Framework.dll"));
							ritems.Add (i);
							needsSave = true;
						}
					}
					foreach (var i in ig.Items.Where (x => x.Name == "Reference" && x.Include == "Tao.Sdl")) {
						if (!i.HasMetadata ("HintPath")) {
							monitor.Log.WriteLine ("Fixing {0} to be Tao.Sdl", i.Include);
							var a = ig.AddNewItem ("Reference", i.Include);
							a.SetMetadata ("HintPath", string.Format (path, platform, "Tao.Sdl.dll"));
							ritems.Add (i);
							needsSave = true;
						}
					}
					foreach (var i in ig.Items.Where (x => x.Name == "Reference" && x.Include.StartsWith ("OpenTK") &&
							(platform != "iOS" && platform != "Android"))) {
						if (!i.HasMetadata ("HintPath")) {
							monitor.Log.WriteLine ("Fixing {0} to be OpenTK", i.Include);
							var a = ig.AddNewItem ("Reference", i.Include);
							a.SetMetadata ("HintPath", string.Format (path, platform, "OpenTK.dll"));
							a.SetMetadata ("SpecificVersion", "true");
							ritems.Add (i);
							needsSave = true;
						}
					}
					foreach (var i in ig.Items.Where (x => x.Name == "Reference" && x.Include == "NVorbis")) {
						if (!i.HasMetadata ("HintPath")) {
							monitor.Log.WriteLine ("Fixing {0} to be NVorbis", i.Include);
							var a = ig.AddNewItem ("Reference", i.Include);
							a.SetMetadata ("HintPath", string.Format (path, platform, "NVorbis.dll"));
							ritems.Add (i);
							needsSave = true;
						}
					}
				}
				foreach (var a in ritems) {
					project.RemoveItem (a);
				}
				var dotNetProject = item as DotNetProject;
				if (dotNetProject != null && (type == "MonoMacProject" || type == "XamMacProject" )) {
					var items = new List<ProjectReference> ();
					var newitems = new List<ProjectReference> ();
					foreach (var reference in dotNetProject.References) {
						if (reference.Reference == "MonoGame.Framework" && string.IsNullOrEmpty (reference.HintPath)) {
							items.Add (reference);
							newitems.Add (new ProjectReference (ReferenceType.Assembly, reference.Reference, string.Format (path, platform, "MonoGame.Framework.dll")));
						}
						if (reference.Reference.StartsWith ("OpenTK") && string.IsNullOrEmpty (reference.HintPath)) {
							items.Add (reference);
							newitems.Add (new ProjectReference (ReferenceType.Assembly, reference.Reference, string.Format (path, platform, "OpenTK.dll")));
						}
						if (reference.Reference == "NVorbis" && string.IsNullOrEmpty (reference.HintPath)) {
							items.Add (reference);
							newitems.Add (new ProjectReference (ReferenceType.Assembly, reference.Reference, string.Format (path, platform, "NVorbis.dll")));
						}
						if (reference.Reference == "Tao.Sdl" && string.IsNullOrEmpty (reference.HintPath)) {
							items.Add (reference);
							newitems.Add (new ProjectReference (ReferenceType.Assembly, reference.Reference, string.Format (path, platform, "Tao.Sdl.dll")));
						}
					}
					dotNetProject.References.RemoveRange (items);
					dotNetProject.References.AddRange (newitems);
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
			var changed = (UpgradeMonoGameProject (monitor, item, project));
			base.LoadProject (monitor, item, project);
			if (changed) {
				project.Save (project.FileName);
			}
		}

		public override void SaveProject (MonoDevelop.Core.IProgressMonitor monitor, MonoDevelop.Projects.SolutionEntityItem item, MSBuildProject project)
		{
			var changed = UpgradeMonoGameProject (monitor, item, project);
			base.SaveProject (monitor, item, project);
			if (changed) {
				this.LoadProject (monitor, item, project);
			}
		}
	}
}

