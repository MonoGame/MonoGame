using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline.Windows
{
    public partial class NewContentDialog : Form
    {
        public NewContentDialog(IEnumerable<ContentItemTemplate> items)
        {
            InitializeComponent();

            foreach (var i in items)
            {
                var ctrl = new Button()
                    {
                        Text = i.Label,
                        
                    }
                _listBox.Controls.Add(ctrl);
            }
        }
    }
}
