using System;

namespace MonoGame.Tools.Pipeline
{
	public partial class AboutDialog : Gtk.Dialog
	{
		public AboutDialog ()
		{
			this.Build ();

			this.Title = String.Format("About {0}", AssemblyAttributes.AssemblyTitle);

			label1.Text = AssemblyAttributes.AssemblyProduct;
			label2.Text = String.Format("Version {0}", AssemblyAttributes.AssemblyVersion);
			label3.Text = AssemblyAttributes.AssemblyCopyright;
			label4.Text = AssemblyAttributes.AssemblyCompany;

			textview1.Buffer.Text = AssemblyAttributes.AssemblyDescription;
		}

		protected void OnResponse(object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}

