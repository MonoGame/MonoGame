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

        OutputParser outputParser;

        Uri folderUri;

        public BuildOutput()
        {
            Build();

            outputParser = new OutputParser();

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

            outputParser.Reset();
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

            outputParser.Parse(text);
            text = text.TrimEnd(new[] { ' ','\n','\r','\t' });

            switch (outputParser.State)
            {
                case OutputState.BuildBegin:
                    AddItem(icon_begin_end, text, "");
                    break;
                case OutputState.Cleaning:
                    AddItem(icon_clean, "Cleaning " + GetRelativePath(outputParser.Filename), "");
                    listStore.AppendValues(tmpIter, null, text, "");
                    break;
                case OutputState.Skipping:
                    AddItem(icon_skip, "Skipping " + GetRelativePath(outputParser.Filename), "");
                    listStore.AppendValues(tmpIter, null, text, "");
                    break;
                case OutputState.BuildAsset:
                    AddItem(icon_processing, "Building " + GetRelativePath(outputParser.Filename), "");
                    listStore.AppendValues(tmpIter, null, text, "");
                    break;
                case OutputState.BuildError:
                    listStore.SetValue(tmpIter, 0, icon_fail);
                    listStore.AppendValues(tmpIter, null, outputParser.ErrorMessage, "");
                    break;
                case OutputState.BuildErrorContinue:
                    listStore.AppendValues(tmpIter, null, outputParser.ErrorMessage, "");
                    break;
                case OutputState.BuildEnd:
                    AddItem(icon_begin_end, text, "");
                    break;
                case OutputState.BuildTime:
                    listStore.SetValue(tmpIter, 1, listStore.GetValue(tmpIter, 1).ToString().TrimEnd(new[] { '.', ' ' }) + ", " + text);
                    break;
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
            try
            {
                var pathUri = new Uri(path);
                return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', System.IO.Path.DirectorySeparatorChar));
            }
            catch
            {
                return path;
            }
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

