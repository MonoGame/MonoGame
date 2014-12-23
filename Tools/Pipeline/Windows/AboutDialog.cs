using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AttributeAccessors.AssemblyTitle);
            this.labelProductName.Text = AttributeAccessors.AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AttributeAccessors.AssemblyVersion);
            this.labelCopyright.Text = AttributeAccessors.AssemblyCopyright;
            this.labelCompanyName.Text = AttributeAccessors.AssemblyCompany;
            this.textBoxDescription.Text = AttributeAccessors.AssemblyDescription;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
