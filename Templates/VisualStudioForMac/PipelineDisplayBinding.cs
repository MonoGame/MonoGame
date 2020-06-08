using System;
using System.IO;
using System.Diagnostics;
using MonoDevelop.Ide.Gui;

namespace MonoGame.IDE.VisualStudioForMac {
	public class PipelineDisplayBinding : IExternalDisplayBinding {
		#region IExternalDisplayBinding implementation
		public MonoDevelop.Ide.Desktop.DesktopApplication GetApplication (MonoDevelop.Core.FilePath fileName, string mimeType, MonoDevelop.Projects.Project ownerProject)
		{
			return new PipelineDesktopApplication (fileName.FullPath, ownerProject);
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
		private readonly static string appPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Applications/MGCB Editor.app");

		MonoDevelop.Projects.Project project;
		string filename;

		public PipelineDesktopApplication (string filename, MonoDevelop.Projects.Project ownerProject)
			: base ("MGCBEditor", "MGCB Editor", true)
		{
			this.project = ownerProject;
			this.filename = filename;
		}

		public override void Launch (params string [] files)
		{
			var process = new Process ();
			process.StartInfo.FileName = "open";
			process.StartInfo.Arguments = string.Format("\"{0}\"", appPath);
			process.StartInfo.EnvironmentVariables.Add("MONOGAME_PIPELINE_PROJECT", Path.GetFullPath(filename));
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.UseShellExecute = false;

			// Fire off the process.
			process.Start ();
		}
	}
}
