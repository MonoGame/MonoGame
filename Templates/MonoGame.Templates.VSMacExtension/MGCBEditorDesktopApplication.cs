// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.Diagnostics;
using MonoDevelop.Ide.Desktop;

namespace MonoGame.Templates.VSMacExtension
{
	public class MGCBEditorDesktopApplication : DesktopApplication
	{
		private string _filename;

		public MGCBEditorDesktopApplication(string filename) : base("MonoGame.Templates.VSMacExtension.MGCBEditor", "MGCB Editor", true)
		{
			_filename = filename;
		}

		public override void Launch(params string[] files)
		{
			var process = new Process();
			process.StartInfo.FileName = "dotnet";
			process.StartInfo.Arguments = $"mgcb-editor \"{_filename}\"";
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(_filename);
            process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.StartInfo.UseShellExecute = false;

			process.Start();
		}
	}
}
