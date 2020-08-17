// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Desktop;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;
using MonoGame.Tools.Pipeline;

namespace MonoGame.IDE.VisualStudioForMac
{
	public class MGCBPadBinding : IExternalDisplayBinding
	{
		public DesktopApplication GetApplication(FilePath fileName, string mimeType, Project ownerProject)
			=> new MGCBPadEditor(fileName.FullPath);

		public bool CanHandle(FilePath fileName, string mimeType, Project ownerProject)
			=> mimeType == "x-mgcb";

		public bool CanUseAsDefault => true;
	}

	class MGCBPadEditor : DesktopApplication
	{
		private string _filename;

		public MGCBPadEditor(string filename) : base("MonoGame.IDE.VisualStudioForMac.MGCBPadEditor", "MGCB Pad", true)
		{
			_filename = filename;
		}

		public override void Launch(params string[] files)
		{
			var pad = IdeApp.Workbench.GetPad<MGPad>() ??
					IdeApp.Workbench.ShowPad(new MGPad(), "MGPad", "MonoGame Content", "Left", "monogame-project-icon-16");

			pad.BringToFront();
			PipelineController.Instance.OpenProject(_filename);
		}
	}
}
