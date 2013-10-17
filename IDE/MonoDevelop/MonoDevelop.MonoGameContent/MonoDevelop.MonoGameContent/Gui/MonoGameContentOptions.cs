using System;
using Gtk;
using MonoDevelop.Ide.Gui.Dialogs;

namespace MonoDevelop.MonoGameContent
{
	public class MonoGameContentOptions : MultiConfigItemOptionsPanel
	{
		MonoGameOptions widget;

		public override Widget CreatePanelWidget ()
		{
			AllowMixedConfigurations = false;
			return widget = new MonoGameOptions ();
		}

		public override bool IsVisible ()
		{
			return ConfiguredProject is MonoGameContentProject;
		}

		public override void LoadConfigData ()
		{
			widget.LoadPanelContents ((MonoGameContentProjectConfiguration)CurrentConfiguration);
		}

		public override void ApplyChanges ()
		{
			widget.StorePanelContents ((MonoGameContentProjectConfiguration)CurrentConfiguration);
		}

	}
}

