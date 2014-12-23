using System;

namespace MonoGame.Tools.Pipeline
{
	public partial class AboutDialog : Gtk.Dialog
	{
		public AboutDialog ()
		{
			this.Build ();

			this.Title = String.Format("About {0}", AttributeAccessors.AssemblyTitle);

			label1.Text = AttributeAccessors.AssemblyProduct;
			label2.Text = String.Format("Version {0}", AttributeAccessors.AssemblyVersion);
			label3.Text = AttributeAccessors.AssemblyCopyright;
			label4.Text = AttributeAccessors.AssemblyCompany;

			textview1.Buffer.Text = AttributeAccessors.AssemblyDescription;
		}

		protected void OnResponse(object sender, EventArgs e)
		{
			this.Destroy ();
		}
	}
}

