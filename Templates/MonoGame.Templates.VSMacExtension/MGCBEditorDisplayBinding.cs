// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using MonoDevelop.Core;
using MonoDevelop.Ide.Desktop;
using MonoDevelop.Ide.Gui;
using MonoDevelop.Projects;

namespace MonoGame.Templates.VSMacExtension
{
	public class MGCBEditorDisplayBinding : IExternalDisplayBinding
	{
		public DesktopApplication GetApplication(FilePath fileName, string mimeType, Project ownerProject)
			=> new MGCBEditorDesktopApplication(fileName.FullPath);

		public bool CanHandle(FilePath fileName, string mimeType, Project ownerProject)
			=> mimeType == "x-mgcb";

		public bool CanUseAsDefault => true;
	}
}
