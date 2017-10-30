using System;
using System.Linq;
using MonoDevelop.Projects;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;

namespace MonoDevelop.MonoGame
{
	public class MonoGameBuildExtension : ProjectServiceExtension
	{
		/// <summary>
		/// List of ProjectTypes which map to the Platform Enum in MonoGame
		/// This is used to pass the correct /platform parameter for the auto 
		/// build of content.
		/// </summary>
		static Dictionary<string,string> supportedProjectTypes = new Dictionary<string,string>() { 
			{"MonoMac", "MacOSX"}, 
			{"XamMac","DesktopGL"}, 
		};

		string platform;

		protected override BuildResult Build (MonoDevelop.Core.IProgressMonitor monitor, SolutionEntityItem item, ConfigurationSelector configuration)
		{
			#if DEBUG			
			monitor.Log.WriteLine("MonoGame Extension Build Called");	
			#endif			
			try
			{
				var proj = item as Project;
				if (proj == null)
				{
					#if DEBUG	
					monitor.Log.WriteLine("MonoGame Extension Null Project");	
					#endif
					return base.Build (monitor, item, configuration);
				}
				#if DEBUG	
				foreach(var p in proj.GetProjectTypes()) {
					monitor.Log.WriteLine("MonoGame Extension Project Type {0}", p);	
				}
				#endif
				if (!proj.GetProjectTypes().Any(x => supportedProjectTypes.ContainsKey(x)))
					return base.Build (monitor, item, configuration);

				var files = proj.Items.Where(x => x is ProjectFile).Cast<ProjectFile>();
				foreach(var file in files.Where(f => f.BuildAction == "MonoGameContentReference")) {
					monitor.Log.WriteLine ("Found MonoGame Content Builder Response File : {0}", file.FilePath);
					platform = proj.GetProjectTypes().FirstOrDefault(x => supportedProjectTypes.ContainsKey(x));
					if (!string.IsNullOrEmpty (platform)) {
						try {
							RunMonoGameContentBuilder(file.FilePath.ToString(), supportedProjectTypes[platform], monitor);
						} catch (Exception ex) {
							monitor.ReportWarning(ex.ToString());
						}
					}
				}
				return base.Build (monitor, item, configuration);
			}
			finally
			{
				#if DEBUG				
				monitor.Log.WriteLine("MonoGame Extension Build Ended");	
				#endif				
			}
		}

		protected override BuildResult Compile (MonoDevelop.Core.IProgressMonitor monitor, SolutionEntityItem item, BuildData buildData)
		{
			var proj = item as Project;
			if (proj == null)
				return base.Compile (monitor, item, buildData);
			if (!proj.GetProjectTypes().Any(x => supportedProjectTypes.ContainsKey(x)))
				return base.Compile (monitor, item, buildData);
			var files = buildData.Items.Where(x => x is ProjectFile).Cast<ProjectFile>().ToArray();
			foreach (var file in files.Where(f => f.BuildAction == "MonoGameContentReference")) {
				var path = System.IO.Path.Combine (Path.GetDirectoryName (file.FilePath.ToString ()), "bin", supportedProjectTypes[platform]);
				monitor.Log.WriteLine("Processing {0}", path);	
				if (!Directory.Exists (path))
					continue;
				foreach (var output in Directory.GetFiles (path, "*.*", SearchOption.AllDirectories)) {
					var link = string.Format ("Content{0}",  output.Replace (path, ""));
					if (proj.Files.FirstOrDefault (x => Path.GetFileName (x.FilePath.ToString ()) == Path.GetFileName (output)) == null) {
						monitor.Log.WriteLine ("Auto Including Content {0}", output);
						proj.Files.Add (new ProjectFile (output, BuildAction.BundleResource) {
							Link = new MonoDevelop.Core.FilePath (link),
							Flags = ProjectItemFlags.DontPersist | ProjectItemFlags.Hidden,
							Visible = false,
						});
					}
				}
			}
			return base.Compile (monitor, item, buildData);
		}

		void RunMonoGameContentBuilder(string responseFile, string platform, MonoDevelop.Core.IProgressMonitor monitor) {
			var process = new Process ();
			var location = Path.Combine (Path.GetDirectoryName (typeof(MonoGameBuildExtension).Assembly.Location), "MGCB.exe");
			if (!File.Exists (location)) {
				switch (Environment.OSVersion.Platform) {
					case PlatformID.Win32NT:
						location = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86), @"MSBuild\MonoGame\v3.0\Tools", "MGCB.exe");
						break;
					case PlatformID.Unix:
						if (Directory.Exists ("/Applications") &&
							Directory.Exists ("/Users")) {
							location = Path.Combine ("/Applications/Pipeline.app/Contents/MonoBundle", "MGCB.exe");
						} else {
							location = Path.Combine ("/bin", "mgcb");
						}
						break;
					case PlatformID.MacOSX:
						location = Path.Combine ("/Applications/Pipeline.app/Contents/MonoBundle", "MGCB.exe");
						break;
				}
			}
			if (!File.Exists (location)) {
				monitor.Log.WriteLine ("MGCB.exe not found");
				return;
			}
			process.StartInfo.WorkingDirectory = Path.GetDirectoryName (responseFile);
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				process.StartInfo.FileName = location;
				process.StartInfo.Arguments = string.Format ("/@:\"{1}\" /platform:{0}", platform, responseFile);
			} else if (Directory.Exists ("/Applications") &&
					Directory.Exists ("/Users")) {
				process.StartInfo.FileName = "mono";
				process.StartInfo.Arguments = string.Format ("\"{0}\" /@:\"{2}\" /platform:{1}", location, platform, responseFile);
			} else {
				process.StartInfo.FileName = location;
				process.StartInfo.Arguments = string.Format ("/@:\"{1}\" /platform:{0}", platform, responseFile);
			}
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.RedirectStandardOutput = true;
			process.OutputDataReceived += (sender, args) => monitor.Log.WriteLine(args.Data);

			monitor.Log.WriteLine ("{0} {1}", process.StartInfo.FileName, process.StartInfo.Arguments);

			// Fire off the process.
			process.Start();
			process.BeginOutputReadLine();
			process.WaitForExit();
		}
	}
}
