using System;
using MonoDevelop.Ide.Gui;
using System.IO;
using System.Diagnostics;
using MonoDevelop.Ide.Gui.Content;
using Gtk;

namespace MonoDevelop.MonoGame
{
	#if BUILDINEDITOR
	public class PipelineDisplayBinding: IViewDisplayBinding
	{
		#region IViewDisplayBinding implementation

		public IViewContent CreateContent (MonoDevelop.Core.FilePath fileName, string mimeType, MonoDevelop.Projects.Project ownerProject)
		{
			return new MonoGameContentEditorViewContent (fileName, ownerProject);
		}

		public string Name {
			get {
				return "MonoGame Content Builder";
			}
		}

		#endregion

		#region IDisplayBinding implementation

		public bool CanHandle (MonoDevelop.Core.FilePath fileName, string mimeType, MonoDevelop.Projects.Project ownerProject)
		{
			return mimeType == "text/x-mgcb";
		}

		public bool CanUseAsDefault {
			get {
				return true;
			}
		}

		#endregion
	}
	#else
	public class PipelineDisplayBinding : IExternalDisplayBinding {
		#region IExternalDisplayBinding implementation
		public MonoDevelop.Ide.Desktop.DesktopApplication GetApplication (MonoDevelop.Core.FilePath fileName, string mimeType, MonoDevelop.Projects.Project ownerProject)
		{
			return new PipelineDesktopApplication (fileName.FullPath,ownerProject);
		}
		#endregion
		#region IDisplayBinding implementation
		public bool CanHandle (MonoDevelop.Core.FilePath fileName, string mimeType, MonoDevelop.Projects.Project ownerProject)
		{
			return mimeType == "text/x-mgcb";
		}
		public bool CanUseAsDefault {
			get {
				return true;
			}
		}
		#endregion
		
	}

	class PipelineDesktopApplication : MonoDevelop.Ide.Desktop.DesktopApplication
	{
		MonoDevelop.Projects.Project project;
		string filename;

		public PipelineDesktopApplication (string filename, MonoDevelop.Projects.Project ownerProject)
			: base ("MonoGamePipelineTool", "MonoGame Pipeline Tool", true)
		{
			this.project = ownerProject;
			this.filename = filename;
		}

		public override void Launch (params string[] files)
		{
			var process = new Process ();
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				process.StartInfo.FileName = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86), @"MSBuild\MonoGame\v3.0\Tools", "Pipeline.exe");
				process.StartInfo.Arguments = string.Format ("\"{0}\"", filename);
			} else {
				if (Directory.Exists ("/Applications/Pipeline.app")) {
					process.StartInfo.FileName = "open";
					process.StartInfo.EnvironmentVariables.Add("MONOGAME_PIPELINE_PROJECT", Path.GetFullPath (filename));
					process.StartInfo.Arguments = string.Format ("-b com.monogame.pipeline --args \"{0}\"", Path.GetFullPath (filename));
				} else {
					// figure out linix 
					process.StartInfo.FileName = "monogame-pipeline-tool";
					process.StartInfo.Arguments = string.Format ("\"{0}\"", filename);
				}
			}
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.UseShellExecute = false;
			
			// Fire off the process.
			process.Start();
		}
	}
	#endif
}

