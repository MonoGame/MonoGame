using System;
using System.Collections.Generic;

using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class CollectionEditorDialog : Dialog
    {
        private string startLocation;
        private FileFilter dllFilter;
        private TreeStore listStore;
        private PipelineController controller;

        public string text = "";

        public CollectionEditorDialog (Window parrent, string text) : base(Global.GetNewDialog(parrent.Handle))
        {
            Build();

            this.controller = ((PipelineController)((MainWindow)parrent)._controller);

            dllFilter = new FileFilter ();
            dllFilter.Name = "Dll Files (*.dll)";
            dllFilter.AddPattern ("*.dll");

            startLocation = controller.ProjectLocation;

            var column = new TreeViewColumn ();

            var textCell = new CellRendererText ();
            var dataCell = new CellRendererText ();

            dataCell.Visible = false;

            column.PackStart (textCell, false);
            column.PackStart (dataCell, false);

            treeView1.AppendColumn (column);

            column.AddAttribute (textCell, "markup", 0);
            column.AddAttribute (dataCell, "text", 1);

            listStore = new TreeStore (typeof (string), typeof (string));

            treeView1.Model = listStore;
            treeView1.Selection.Mode = SelectionMode.Multiple;

            var refs = text.Split(Environment.NewLine[0]);

            foreach (string reff in refs)
                if (!string.IsNullOrWhiteSpace(reff))
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
            var ret = new List<string>();

            TreeIter iter;
            if (listStore.GetIterFirst(out iter))
            {
                ret.Add(treeView1.Model.GetValue(iter, 1).ToString());

                while (listStore.IterNext(ref iter))
                    ret.Add(treeView1.Model.GetValue(iter, 1).ToString());
            }

            return ret;
        }

        private void SelectionChanged (object sender, EventArgs e)
        {
            buttonRemove.Sensitive = (treeView1.Selection.GetSelectedRows().Length > 0);
        }

        private void AddFileEvent(object sender, EventArgs e)
        {
            var fileNames = new string[0];
            var files = GetFileNames();

            var pl = controller.ProjectLocation;
            if (!pl.EndsWith(System.IO.Path.DirectorySeparatorChar.ToString()))
                pl += System.IO.Path.DirectorySeparatorChar;
            var folderUri = new Uri(pl);

            var fileChooser =
                new FileChooserDialog("Select Reference File",
                    this,
                    FileChooserAction.Open,
                    "Cancel", ResponseType.Cancel,
                    "Open", ResponseType.Ok);

            fileChooser.AddFilter (dllFilter);
            fileChooser.SetCurrentFolder(startLocation);

            if(fileChooser.Run() == (int)ResponseType.Ok)
                fileNames = fileChooser.Filenames;
            
            fileChooser.Destroy();

            if (fileNames.Length > 0)
            {
                foreach (string floc in fileNames)
                {
                    if (System.IO.File.Exists(floc))
                    {
                        var pathUri = new Uri(floc);
                        string fl = Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));

                        if (!files.Contains(fl))
                            AddValue(fl);
                    }
                }
            }
        }

        private void RemoveFileEvent(object sender, EventArgs e)
        {
            var paths = treeView1.Selection.GetSelectedRows ();

            for (int i = paths.Length - 1; i >= 0; i--)
            {
                TreeIter iter;
                if (treeView1.Model.GetIter(out iter, paths[i]))
                    listStore.Remove(ref iter);
            }
        }
    }
}

