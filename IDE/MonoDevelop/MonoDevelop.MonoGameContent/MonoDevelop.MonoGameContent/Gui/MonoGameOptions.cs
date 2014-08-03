using System;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace MonoDevelop.MonoGameContent
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MonoGameOptions : Gtk.Bin
	{
		public MonoGameOptions ()
		{
			this.Build ();

			cmbPlatforms.AppendText (TargetPlatform.Windows.ToString());
			cmbPlatforms.AppendText (TargetPlatform.Xbox360.ToString());
			cmbPlatforms.AppendText (TargetPlatform.WindowsPhone.ToString());
			cmbPlatforms.AppendText (TargetPlatform.iOS.ToString());
			cmbPlatforms.AppendText (TargetPlatform.Android.ToString());
			cmbPlatforms.AppendText (TargetPlatform.Linux.ToString());
			cmbPlatforms.AppendText (TargetPlatform.MacOSX.ToString());
			cmbPlatforms.AppendText (TargetPlatform.WindowsStoreApp.ToString());
			cmbPlatforms.AppendText (TargetPlatform.NativeClient.ToString());
			cmbPlatforms.AppendText (TargetPlatform.Ouya.ToString());
			cmbPlatforms.AppendText (TargetPlatform.PlayStationMobile.ToString());
			cmbPlatforms.AppendText (TargetPlatform.WindowsPhone8.ToString());
			cmbPlatforms.AppendText (TargetPlatform.RaspberryPi.ToString());

			ShowAll ();

		}

		public void LoadPanelContents (MonoGameContentProjectConfiguration cfg)
		{
			cmbPlatforms.Active = (int)Enum.Parse (typeof(TargetPlatform), cfg.MonoGamePlatform);
			chkCompress.Active = cfg.XnaCompressContent.ToLowerInvariant() == "true";
		}

		public void StorePanelContents (MonoGameContentProjectConfiguration cfg)
		{
			cfg.MonoGamePlatform = ((TargetPlatform)(cmbPlatforms.Active)).ToString();
			cfg.XnaCompressContent = chkCompress.Active.ToString();
		}
	}
}

