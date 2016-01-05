using System.Collections.Generic;
using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class CollectionEditorDialog : Gtk.Dialog
    {
        TreeStore listStore;
        PipelineController controller;

        public string text = "";

        public CollectionEditorDialog (Window parrent, string text) : base(Global.GetNewDialog(parrent.Handle))
        {
            Build();

            this.Title = Mono.Unix.Catalog.GetString ("Reference Editor");

            this.AddButton("Ok", ResponseType.Ok);
            this.AddButton("Cancel", ResponseType.Cancel);
            this.DefaultResponse = ResponseType.Ok;

            this.controller = ((PipelineController)((MainWindow)parrent)._controller);

            FileFilter filter = new FileFilter ();
            filter.AddPattern ("*.dll");

            filechooserwidget1.Filter = filter;
            filechooserwidget1.SetCurrentFolder(controller.ProjectLocation);
            filechooserwidget1.SelectMultiple = true;

            var column = new TreeViewColumn ();

            var textCell = new CellRendererText ();
            var dataCell = new CellRendererText ();

            dataCell.Visible = false;

            column.PackStart (textCell, false);
            column.PackStart (dataCell, false);

            treeview1.AppendColumn (column);

            column.AddAttribute (textCell, "markup", 0);
            column.AddAttribute (dataCell, "text", 1);

            listStore = new TreeStore (typeof (string), typeof (string));

            treeview1.Model = listStore;
            treeview1.Selection.Mode = SelectionMode.Multiple;

            string[] refs = text.Replace("\r\n", "~").Split('~');

            foreach (string reff in refs)
                if (reff != "")
                    AddValue(reff);
        }

        private void AddValue(string floc)
        {
            listStore.AppendValues("<b>" + System.IO.Path.GetFileNameWithoutExtension(floc) + "</b>\r\n" + controller.GetFullPath(floc), floc);
        }

        protected void OnResponse(object sender, EventArgs e)
        {
            var fnms = GetFileNames();

            foreach (string f in fnms)
            {
                if (text != "")
                    text += "\r\n";
                text += f;
            }

            Destroy();
        }

        private List<string> GetFileNames()
        {
            List<string> ret = new List<string>();

            TreeIter iter;
            if (listStore.GetIterFirst(out iter))
            {
                ret.Add(treeview1.Model.GetValue(iter, 1).ToString());

                while (listStore.IterNext(ref iter))
                    ret.Add(treeview1.Model.GetValue(iter, 1).ToString());
            }

            return ret;
        }

        protected void AddFileEvent(object sender, EventArgs e)
        {
            List<string> files = GetFileNames();

            string pl = controller.ProjectLocation;
            if (!pl.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                pl += System.IO.Path.DirectorySeparatorChar;
            Uri folderUri = new Uri(pl);

            if (filechooserwidget1.Filenames.Length > 0)
            {
                foreach (string floc in filechooserwidget1.Filenames)
                {
                    if (System.IO.File.Exists(floc))
                    {
                        Uri pathUri = new Uri(floc);
                        string fl = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));

                        if (!files.Contains(fl))
                            AddValue(fl);
                    }
                }
            }
        }

        protected void RemoveFileEvent(object sender, EventArgs e)
        {
            TreePath[] paths = treeview1.Selection.GetSelectedRows ();

            for (int i = paths.Length - 1; i >= 0; i--)
            {
                TreeIter iter;
                if (treeview1.Model.GetIter(out iter, paths[i]))
                    listStore.Remove(ref iter);
            }
        }
    }
}

