using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class BuildOutput
    {
        TreeView treeview1;
        ScrolledWindow scrollView1, scrollView2;
        TextView textView1;

        public void Build()
        {
            this.ShowTabs = false;

            scrollView1 = new ScrolledWindow();

            treeview1 = new TreeView();
            treeview1.HeadersVisible = false;
            treeview1.CanFocus = true;
            scrollView1.Add(treeview1);

            this.AppendPage(scrollView1, new Label("Output"));

            scrollView2 = new ScrolledWindow();

            textView1 = new TextView();
            textView1.Editable = false;
            scrollView2.Add(textView1);

            this.AppendPage(scrollView2, new Label("Output Log"));
        }
    }
}

