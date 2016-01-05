using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class AddFileDialog : Form
    {
        public bool applyforall;
        public CopyAction responce;

        public AddFileDialog(string fileloc, bool exists)
        {
            InitializeComponent();

            label1.Text = label1.Text.Replace("%file", fileloc);
            if (exists)
            {
                radioButtonCopy.Enabled = false;
                radioButtonLink.Checked = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            applyforall = checkBox1.Checked;
            if (radioButtonCopy.Checked)
                responce = CopyAction.Copy;
            else if (radioButtonLink.Checked)
                responce = CopyAction.Link;
            else
                responce = CopyAction.Skip;

            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
