using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace MonoGame.Tools.Pipeline
{
    public partial class NewContentDialog : Form
    {
        public ContentItemTemplate Selected
        {
            get
            {
                if (_listView.SelectedItems.NullOrEmpty())
                    return null;

                return _listView.SelectedItems[0].Tag as ContentItemTemplate;
            }
        }

        public string NameGiven
        {
            get
            {
                if (string.IsNullOrEmpty(_name.Text))
                {
                    var item = Selected;
                    if (item == null)
                        return "NewContentItem";

                    return string.Concat("New", Path.GetFileNameWithoutExtension(item.TemplateFile));
                }

                return _name.Text;
            }
        }

        public NewContentDialog(IEnumerable<ContentItemTemplate> items, ImageList icons)
        {            
            InitializeComponent();

            _listView.SmallImageList = icons;

            _listView.Items.Clear();
            foreach (var i in items)
            {
                var obj = new ListViewItem(i.Label, i.Icon);
                obj.Tag = i;
                _listView.Items.Add(obj);
            }
        }

        private void OnSelectedValueChanged(object sender, EventArgs e)
        {
            _okBtn.Enabled = _listView.SelectedItems.Count > 0;
        }

        private void OnDoubleClick(object sender, MouseEventArgs e)
        {
            if (Selected != null)
            {
                _okBtn.PerformClick();
            }
        }
    }
}
