using System;

namespace MonoGame.Tools.Pipeline
{
	public partial class ColorPickerDialog : Gtk.Dialog
	{
		public string data;

		public ColorPickerDialog (string data)
		{
			this.Build ();
			this.data = data;

			try
			{
				string[] cvalues = data.Replace (":", " ").Replace("}", " ").Split (' ');

				byte red = (byte)Convert.ToInt16 (cvalues [1]);
				byte green = (byte)Convert.ToInt16 (cvalues [3]);
				byte blue = (byte)Convert.ToInt16 (cvalues [5]);
				int alpha = Convert.ToInt32(cvalues [7]);

				colorselection1.CurrentColor = new Gdk.Color (red, green, blue);
				colorselection1.CurrentAlpha = (ushort)(alpha * 257);
			}
			catch { }
		}

		protected void OnResponse(object sender, EventArgs e)
		{
			this.Destroy ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color ();

			color.R = (byte)Convert.ToInt32(colorselection1.CurrentColor.Red);
			color.G = (byte)Convert.ToInt32(colorselection1.CurrentColor.Green);
			color.B = (byte)Convert.ToInt32(colorselection1.CurrentColor.Blue);
			color.A = (byte)(colorselection1.CurrentAlpha / 257);

			data = color.ToString ();
			this.Respond (Gtk.ResponseType.Ok);
		}
	}
}

