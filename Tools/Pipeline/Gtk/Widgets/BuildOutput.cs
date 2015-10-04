using System;
using Gtk;
using Gdk;

namespace MonoGame.Tools.Pipeline
{
    public partial class BuildOutput : Notebook
    {
        Pixbuf icon_begin_end = new Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.build_begin_end.png");
        Pixbuf icon_processing = new Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.build_processing.png");
        Pixbuf icon_fail = new Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.build_fail.png");
        Pixbuf icon_succeed = new Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.build_succeed.png");
        Pixbuf icon_skip = new Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.build_skip.png");
        Pixbuf icon_clean = new Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.build_clean.png");

        TreeIter tmpIter;
        TreeStore listStore;

        Uri folderUri;

        public BuildOutput()
        {
            Build();

            var column = new TreeViewColumn ();

            var iconCell = new CellRendererPixbuf ();
            var textCell = new CellRendererText ();
            var idCell = new CellRendererText ();

            idCell.Visible = false;

            column.PackStart (iconCell, false);
            column.PackStart (textCell, false);
            column.PackStart (idCell, false);

            treeview1.AppendColumn (column);

            column.AddAttribute (iconCell,  "pixbuf", 0);
            column.AddAttribute (textCell, "text", 1);
            column.AddAttribute (idCell, "text", 2);

            listStore = new TreeStore (typeof (Gdk.Pixbuf), typeof (string), typeof (string));

            treeview1.Model = listStore;

            textView1.SizeAllocated += TextView1_SizeAllocated;
        }

        internal void SetBaseFolder(IController controller)
        {
            string pl = ((PipelineController)controller).ProjectLocation;
            if (!pl.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                pl += System.IO.Path.DirectorySeparatorChar;
            folderUri = new Uri(pl);
        }

        private void TextView1_SizeAllocated (object o, SizeAllocatedArgs args)
        {
            textView1.ScrollToIter(textView1.Buffer.EndIter, 0, false, 0, 0);
        }

        public void OutputAppend(string text)
        {
            lock (textView1.Buffer)
            {
                textView1.Buffer.Text += text + "\r\n";
            }

            if (string.IsNullOrWhiteSpace(text))
                return;

            if (text.StartsWith("Build "))
                AddItem(icon_begin_end, text.Remove(text.Length - 1), "");
            else if (text.StartsWith("Time "))
            {
                treeview1.Model.SetValue(tmpIter, 1, treeview1.Model.GetValue(tmpIter, 1) + ", " + text);
            }
            else if (text.StartsWith("Skipping"))
            {
                AddItem(icon_skip, "Skipping " + GetRelativePath(text.Substring(9)), "");
                listStore.AppendValues(tmpIter, null, text.Substring(9), "");
            }
            else if (text.StartsWith("Cleaning"))
            {
                AddItem(icon_clean, "Cleaning " + GetRelativePath(text.Substring(9)), "");
                listStore.AppendValues(tmpIter, null, text.Substring(9), "");
            }
            else if (text.StartsWith("/") && !text.Contains("error"))
            {
                AddItem(icon_processing, "Building " + GetRelativePath(text), "");
                listStore.AppendValues(tmpIter, null, text, "");
            }
            else
            {
                if (text.ToLower().Contains("error") || (text.Contains("System") && text.Contains("Exception")))
                    treeview1.Model.SetValue(tmpIter, 0, icon_fail);

                listStore.AppendValues(tmpIter, null, text, "");
            }
        }

        private void AddItem(Pixbuf icon, string text, string secondarytext)
        {
            if (!tmpIter.Equals(new TreeIter()))
                if (treeview1.Model.GetValue(tmpIter, 0) == icon_processing)
                    treeview1.Model.SetValue(tmpIter, 0, icon_succeed);

            tmpIter = listStore.AppendValues(icon, text, secondarytext);
        }

        private string GetRelativePath(string path)
        {
            var pathUri = new Uri(path);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
        }

        public void ClearOutput()
        {
            lock (textView1.Buffer)
            {
                textView1.Buffer.Text = "";
            }

            listStore.Clear();
        }
    }
}

