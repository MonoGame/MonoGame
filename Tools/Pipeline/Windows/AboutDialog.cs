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
            this.Text = String.Format("About {0}", AssemblyAttributes.AssemblyTitle);
            this.labelProductName.Text = AssemblyAttributes.AssemblyProduct;
            this.labelVersion.Text = String.Format("Version {0}", AssemblyAttributes.AssemblyVersion);
            this.labelCopyright.Text = AssemblyAttributes.AssemblyCopyright;
            this.labelCompanyName.Text = AssemblyAttributes.AssemblyCompany;
            this.textBoxDescription.Text = AssemblyAttributes.AssemblyDescription;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
