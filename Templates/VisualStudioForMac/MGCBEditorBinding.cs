// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using MonoDevelop.Core;
using MonoDevelop.Ide.Desktop;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace MonoGame.IDE.VisualStudioForMac
{
	public class MGCBEditorBinding : IExternalDisplayBinding
	{
		public DesktopApplication GetApplication(FilePath fileName, string mimeType, Project ownerProject)
			=> new MGCBEditor(fileName.FullPath);

		public bool CanHandle(FilePath fileName, string mimeType, Project ownerProject)
			=> mimeType == "x-mgcb" && Directory.Exists(MGCBEditor.AppPath);

		public bool CanUseAsDefault => true;
	}

	class MGCBEditor : DesktopApplication
	{
		public readonly static string AppPath = Path.Combine(Environment.GetEnvironmentVariable("HOME"), "Applications/MGCB Editor.app");

		private string _filename;

		public MGCBEditor(string filename) : base("MonoGame.IDE.VisualStudioForMac.MGCBEditor", "MGCB Editor", true)
		{
			_filename = filename;
		}

		public override void Launch(params string[] files)
		{
			var process = new Process();
			process.StartInfo.FileName = "open";
			process.StartInfo.Arguments = string.Format("-a \"{0}\" \"{1}\"", AppPath, _filename);
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.UseShellExecute = false;

			process.Start();
		}
	}
}
