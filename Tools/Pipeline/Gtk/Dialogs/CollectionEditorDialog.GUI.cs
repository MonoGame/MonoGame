using Gtk;

namespace MonoGame.Tools.Pipeline
{
    public partial class CollectionEditorDialog
    {
        private VBox vbox1;
        private HBox hbox1;
        private Button buttonAdd, buttonRemove;
        private ScrolledWindow scrollView1;
        private TreeView treeView1;

        protected virtual void Build ()
        {
            this.Title = "Reference Editor";
            this.WindowPosition = WindowPosition.CenterOnParent;
            this.DefaultWidth = 400;
            this.DefaultHeight = 350;

#if GTK3
            var geom = new Gdk.Geometry();
            geom.MinWidth = this.DefaultWidth;
            geom.MinHeight = 200;
            this.SetGeometryHints(this, geom, Gdk.WindowHints.MinSize);
#endif

            hbox1 = new HBox();

            scrollView1 = new ScrolledWindow();

            treeView1 = new TreeView();
            treeView1.HeadersVisible = false;
            treeView1.Selection.Changed += SelectionChanged;
            scrollView1.Add(treeView1);

            hbox1.PackStart(scrollView1, true, true, 0);

            vbox1 = new VBox();

            buttonAdd = new Button("Add");
            buttonAdd.Clicked += AddFileEvent;
            vbox1.PackStart(buttonAdd, false, true, 0);

            buttonRemove = new Button("Remove");
            buttonRemove.Sensitive = false;
            buttonRemove.Clicked += RemoveFileEvent;
            vbox1.PackStart(buttonRemove, false, true, 0);

            hbox1.PackStart(vbox1, false, true, 1);

#if GTK3
            this.ContentArea.PackStart(hbox1, true, true, 0);
#else
            this.VBox.PackStart(hbox1, true, true, 0);
#endif

            this.AddButton("Ok", ResponseType.Ok);
            this.AddButton("Cancel", ResponseType.Cancel);
            this.DefaultResponse = ResponseType.Ok;

            this.ShowAll ();

            this.Response += this.OnResponse;
        }
    }
}
