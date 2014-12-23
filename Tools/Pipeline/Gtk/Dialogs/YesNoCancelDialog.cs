using System;

namespace MonoGame.Tools.Pipeline
{
	public partial class YesNoCancelDialog : Gtk.Dialog
	{
		public YesNoCancelDialog (string title, string text)
		{
			this.Build ();
			this.Title = title;
			this.label1.Text = text;
		}

		protected void OnResponse(object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}

