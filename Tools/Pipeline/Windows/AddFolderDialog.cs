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
    public partial class AddFolderDialog : Form
    {
        public CopyAction responce;

        public AddFolderDialog(string folder)
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            responce = (radioButtonCopy.Checked) ? CopyAction.Copy : CopyAction.Link;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
