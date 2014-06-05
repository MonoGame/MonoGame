using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class NewContentDialog : Form
    {
        public ContentItemTemplate SelectedTemplate
        {
            get { return _listBox.SelectedItem as ContentItemTemplate; }
        }

        public string ContentName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_name.Text))
                    return _name.Text;

                var item = _listBox.SelectedItem as ContentItemTemplate;
                return Path.GetFileNameWithoutExtension(item.TemplateFile);
            }
        }
                
        public NewContentDialog(IEnumerable<ContentItemTemplate> items)
        {
            InitializeComponent();

            _listBox.Items.Clear();
            foreach (var i in items)
                _listBox.Items.Add(i);
        }

        private void OnListBoxSelectedValueChanged(object sender, EventArgs e)
        {
            _okBtn.Enabled = _listBox.SelectedItem != null;
        }
    }
}
