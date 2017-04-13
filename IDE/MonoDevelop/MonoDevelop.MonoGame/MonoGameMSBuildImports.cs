using System;
using System.Xml;
using System.Xml.XPath;
using System.Linq;
using System.Collections.Generic;
using MonoDevelop.Projects.MSBuild;
using MonoDevelop.Projects;

namespace MonoDevelop.MonoGame
{
	public class MonoGameMSBuildImports : DotNetProjectExtension
	{
		const string MonoGameContentBuildTargets = "$(MSBuildExtensionsPath)\\MonoGame\\v3.0\\MonoGame.Content.Builder.targets";
		const string MonoGameCommonProps = "$(MSBuildExtensionsPath)\\MonoGame\\v3.0\\MonoGame.Common.props";
		const string MonoGameExtensionsPath = @"$(MonoGameInstallDirectory)\MonoGame\v3.0\Assemblies\{0}\{1}";
		const string MonoGameExtensionsAbsolutePath = @"/Library/Frameworks/MonoGame.framework/v3.0/Assemblies/{0}/{1}";

		static bool UpgradeMonoGameProject (MonoDevelop.Core.ProgressMonitor monitor, DotNetProjectExtension extension, MSBuildProject project)
		{
			bool needsSave = false;
			bool containsMGCB = project.ItemGroups.Any (x => x.Items.Any (i => System.IO.Path.GetExtension (i.Include) == ".mgcb"));
			bool isMonoGame = project.PropertyGroups.Any (x => x.GetProperties().Any (p => p.Name == "MonoGamePlatform")) ||
				project.ItemGroups.Any (x => x.Items.Any (i => i.Name == "Reference" && i.Include == "MonoGame.Framework")) ||
				containsMGCB;
			bool isDesktopGL = project.ItemGroups.Any (x => x.Items.Any (i => i.Include.EndsWith ("SDL2.dll")));
			bool isApplication = project.PropertyGroups.Any (x => x.GetProperties().Any (p => p.Name == "OutputType" && p.Value == "Exe"))
			                            | project.PropertyGroups.Any (x => x.GetProperties().Any (p => p.Name == "AndroidApplication" && string.Compare (p.Value, bool.TrueString, true)==0));
			bool isShared = project.PropertyGroups.Any (x => x.GetProperties().Any (p => p.Name == "HasSharedItems" && p.Value == "true"));
			bool absoluteReferenes = false;
			var type = extension.Project.GetType ().Name;

			monitor.Log.WriteLine ("Found {0}", type);
			monitor.Log.WriteLine ("Found {0}", project.GetType ().Name);
			var platform = isDesktopGL ? "DesktopGL" : "Windows";
			var path = MonoGameExtensionsPath;
			if (extension.Project.FlavorGuids.Contains ("{FEACFBD2-3405-455C-9665-78FE426C6842}")) {
				platform = "iOS";
			}
			if (extension.Project.FlavorGuids.Contains ("{06FA79CB-D6CD-4721-BB4B-1BD202089C55}")) {
				platform = "tvOS";
			}
			if (extension.Project.FlavorGuids.Contains ("{EFBA0AD7-5A72-4C68-AF49-83D382785DCF}")) {
				platform = "Android";
			}
			if (extension.Project.FlavorGuids.Contains ("{948B3504-5B70-4649-8FE4-BDE1FB46EC69}")) {
				platform = "MacOSX";
				// MonoMac Classic does not use MSBuild so we need to absolute path.
				path = MonoGameExtensionsAbsolutePath;
				absoluteReferenes = true;
			}
			if (extension.Project.FlavorGuids.Contains ("{42C0BBD9-55CE-4FC1-8D90-A7348ABAFB23}")) {
				platform = "DesktopGL";
				// Xamarin.Mac Classic does not use MSBuild so we need to absolute path.
				path = MonoGameExtensionsAbsolutePath;
				absoluteReferenes = true;
			}
			if (extension.Project.FlavorGuids.Contains ("{A3F8F2AB-B479-4A4A-A458-A89E7DC349F1}")) {
				platform = "DesktopGL";
			}
			monitor.Log.WriteLine ("Platform = {0}", platform);
			monitor.Log.WriteLine ("Path = {0}", path);
			monitor.Log.WriteLine ("isMonoGame {0}", isMonoGame);
			if (isMonoGame) {
				var ritems = new List<MSBuildItem> ();
				foreach (var ig in project.ItemGroups) {
					foreach (var i in ig.Items.Where (x => x.Name == "Reference" && x.Include == "MonoGame.Framework")) {
						var metaData = i.Metadata;
						if (!metaData.HasProperty("HintPath")) {
							monitor.Log.WriteLine ("Fixing {0} to be MonoGameContentReference", i.Include);
							metaData.SetValue ("HintPath", string.Format (path, platform, "MonoGame.Framework.dll"));
							needsSave = true;
						}
					}
					foreach (var i in ig.Items.Where (x => x.Name == "Reference" && x.Include == "Tao.Sdl")) {
						var metaData = i.Metadata;
						if (!metaData.HasProperty("HintPath")) {
							monitor.Log.WriteLine ("Fixing {0} to be Tao.Sdl", i.Include);
							metaData.SetValue ("HintPath", string.Format (path, platform, "Tao.Sdl.dll"));
							needsSave = true;
						}
					}
					foreach (var i in ig.Items.Where (x => x.Name == "Reference" && x.Include.StartsWith ("OpenTK") &&
							(platform != "iOS" && platform != "Android"))) {
						var metaData = i.Metadata;
						if (!metaData.HasProperty ("HintPath")) {
							monitor.Log.WriteLine("Fixing {0} to be OpenTK", i.Include);
							metaData.SetValue ("HintPath", string.Format (path, platform, "OpenTK.dll"));
							metaData.SetValue ("SpecificVersion", "true");
							needsSave = true;
						}
					}
				}
				foreach (var a in ritems) {
					project.RemoveItem (a);
				}
				var dotNetProject = extension.Project;
				if (dotNetProject != null && absoluteReferenes) {
					var items = new List<ProjectReference> ();
					var newitems = new List<ProjectReference> ();
					foreach (var reference in dotNetProject.References) {
						if (reference.Reference == "MonoGame.Framework" && string.IsNullOrEmpty (reference.HintPath)) {
							items.Add (reference);
							newitems.Add (ProjectReference.CreateCustomReference  (ReferenceType.Assembly, reference.Reference, string.Format(path, platform, "MonoGame.Framework.dll")));
						}
						if (reference.Reference.StartsWith ("OpenTK", StringComparison.OrdinalIgnoreCase) && string.IsNullOrEmpty (reference.HintPath)) {
							items.Add (reference);
							newitems.Add (ProjectReference.CreateCustomReference (ReferenceType.Assembly, reference.Reference, string.Format (path, platform, "OpenTK.dll")));
						}
						if (reference.Reference == "Tao.Sdl" && string.IsNullOrEmpty (reference.HintPath)) {
							items.Add (reference);
							newitems.Add (ProjectReference.CreateCustomReference (ReferenceType.Assembly, reference.Reference, string.Format (path, platform, "Tao.Sdl.dll")));
						}
					}
					dotNetProject.References.RemoveRange (items);
					dotNetProject.References.AddRange (newitems);
				}
			}
			if (isMonoGame && containsMGCB && (isApplication || isShared)) {
				if (!project.PropertyGroups.Any (x => x.GetProperties().Any (p => p.Name == "MonoGamePlatform")) && !isShared) {
					monitor.Log.WriteLine ("Adding MonoGamePlatform {0}", platform == "tvOS" ? "iOS" : platform);
					project.PropertyGroups.First ().SetValue ("MonoGamePlatform", platform == "tvOS" ? "iOS" : platform, true);
					needsSave = true;
				}
				if (!project.Imports.Any (x => x.Project.StartsWith (MonoGameCommonProps, StringComparison.OrdinalIgnoreCase))&& !isShared) {
					monitor.Log.WriteLine ("Adding MonoGame.Common.props Import");
					project.AddNewImport(MonoGameCommonProps, string.Format ("Exists('{0}')", MonoGameCommonProps), project.PropertyGroups.First());
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

		protected override void OnWriteProject(MonoDevelop.Core.ProgressMonitor monitor, MSBuildProject msproject)
		{
			base.OnWriteProject(monitor, msproject);
			var changed = UpgradeMonoGameProject (monitor, this, msproject);
			if (changed)
			{
				msproject.Save(msproject.FileName);
				this.Project.NeedsReload = true;
			}
		}

		protected override void OnReadProject(MonoDevelop.Core.ProgressMonitor monitor, MSBuildProject msproject)
		{
			base.OnReadProject (monitor, msproject);
			var changed = (UpgradeMonoGameProject(monitor, this, msproject));
			if (changed)
			{
				msproject.Save (msproject.FileName);
				this.Project.NeedsReload = true;
			}
		}
	}
}

