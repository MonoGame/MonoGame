using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
    [System.ComponentModel.ToolboxItem (true)]
    partial class ProjectView : Bin
    {
        public Menu menu, addmenu;
        public string openedProject;

        public string ID_BASE = "0", ID_FOLDER = "1", ID_FILE = "2", ID_REFBASE = "3", ID_REF = "4";

        public Gdk.Pixbuf ICON_BASE = new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.settings.png");
        public Gdk.Pixbuf[] ICON_FOLDER = {
            new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.folder_closed.png"),
            new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.folder_missing.png"),
            new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.folder_ref.png")
        };
        public Gdk.Pixbuf[] ICON_FILE = { 
            new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.blueprint.png"), 
            new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.missing.png"),
            new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.ref.png")
        };

        string basename;
        TreeStore listStore;
        MainWindow window;
        PropertiesView propertiesView;

        MenuItem treeadd, treeaddseparator, treenewitem, treeadditem, treenewfolder, treeaddfolder, treeopenfile, treedelete, treedeleteseparator, treeopenfilelocation, treeaddref, treerebuild;

        public ProjectView ()
        {
            Build();
            basename = "base";

            var column = new TreeViewColumn ();

            var iconCell = new CellRendererPixbuf ();
            var textCell = new CellRendererText ();
            var idCell = new CellRendererText ();
            var dataCell = new CellRendererText ();

            idCell.Visible = false;
            dataCell.Visible = false;

            column.PackStart (iconCell, false);
            column.PackStart (textCell, false);
            column.PackStart (idCell, false);
            column.PackStart (dataCell, false);

            treeview1.AppendColumn (column);

            column.AddAttribute (iconCell,  "pixbuf", 0);
            column.AddAttribute (textCell, "text", 1);
            column.AddAttribute (idCell, "text", 2);
            column.AddAttribute (dataCell, "text", 3);

            listStore = new TreeStore (typeof (Gdk.Pixbuf), typeof (string), typeof (string), typeof (string));

            treeview1.Model = listStore;
            treeview1.Selection.Mode = SelectionMode.Multiple;

            treeview1.ButtonPressEvent += OnTreeview1ButtonPressEvent;
            treeview1.KeyReleaseEvent += HandleKeyReleaseEvent;
        }

        void HandleKeyReleaseEvent (object o, KeyReleaseEventArgs args)
        {
            if (args.Event.Key.ToString () == "Menu") 
                ShowMenu ();
        }

        public void Initalize(MainWindow window, MenuItem treerebuild, PropertiesView propertiesView)
        {
            this.window = window;
            this.treerebuild = treerebuild;
            this.propertiesView = propertiesView;

            menu = new Menu ();
            addmenu = new Menu();

            treeadd = new MenuItem("Add");
            treeadd.Submenu = addmenu;

            treeaddref = new MenuItem("Add Reference");
            treeaddref.Activated += delegate
            {
                window._controller.IncludeReferences();
                window.UpdateMenus();
            };

            treeaddseparator = new SeparatorMenuItem ();

            treenewitem = new MenuItem ("New Item...");
            treenewitem.ButtonPressEvent += window.OnNewItemActionActivated;

            treenewfolder = new MenuItem ("New Folder...");
            treenewfolder.ButtonPressEvent += window.OnNewFolderActionActivated;

            treeadditem = new MenuItem ("Existing Item...");
            treeadditem.ButtonPressEvent += window.OnAddItemActionActivated;

            treeaddfolder = new MenuItem ("Existing Folder...");
            treeaddfolder.ButtonPressEvent += window.OnAddFolderActionActivated;

            treedeleteseparator = new SeparatorMenuItem();

            treedelete = new MenuItem ("Remove");
            treedelete.Activated += window.OnDeleteActionActivated;

            treeopenfile = new MenuItem ("Open");
            treeopenfile.Activated += delegate {
                List<TreeIter> iters;
                List<string> ids;
                GetSelectedTreePath(out iters, out ids);

                if (ids.Count != 1)
                    return;

                if(ids[0] != ID_BASE)
                    Process.Start(window._controller.GetFullPath(GetPathFromIter(iters[0])));
                else
                    Process.Start(openedProject);
            };

            treeopenfilelocation = new MenuItem ("Open Item Directory");
            treeopenfilelocation.Activated += delegate {
                List<TreeIter> iters;
                List<string> ids;
                GetSelectedTreePath(out iters, out ids);

                if(ids[0] == ID_BASE)
                    Process.Start(System.IO.Path.GetDirectoryName(window._controller.GetFullPath("")));
                else if(ids[0] == ID_REF)
                    Process.Start(System.IO.Path.GetDirectoryName(window._controller.GetFullPath(treeview1.Model.GetValue(iters[0], 3).ToString())));
                else
                    Process.Start(System.IO.Path.GetDirectoryName(window._controller.GetFullPath(GetPathFromIter(iters[0]))));
            };

            addmenu.Add (treenewitem);
            addmenu.Add (treenewfolder);
            addmenu.Add (new SeparatorMenuItem ());
            addmenu.Add (treeadditem);
            addmenu.Add (treeaddfolder);

            menu.Add (treeaddref);
            menu.Add (treeopenfile);
            menu.Add (treeadd);
            menu.Add (treeaddseparator);
            menu.Add (treeopenfilelocation);
            menu.Add (treerebuild);
            menu.Add (treedeleteseparator);
            menu.Add (treedelete);
        }

        public void SetBaseIter(string name)
        {
            basename = name;
            TreeIter iter = GetBaseIter ();
            treeview1.Model.SetValue (iter, 1, name);
        }

        public TreeIter GetBaseIter()
        {
            TreeIter iter;
        
            if (!treeview1.Model.GetIterFromString(out iter, "0"))
            {
                iter = listStore.AppendValues(ICON_BASE, basename, ID_BASE, "");
                listStore.AppendValues(iter, ICON_FOLDER[2], "References", ID_REFBASE, "");
            }
            treeview1.ExpandRow(treeview1.Model.GetPath(iter), false);

            return iter;
        }

        public TreeIter GetRefIter()
        {
            TreeIter biter = GetBaseIter();
            TreeIter oiter;

            if(treeview1.Model.IterChildren (out oiter, biter)) {

                if (treeview1.Model.GetValue(oiter, 2).ToString() == ID_REFBASE)
                    return oiter;

                while (treeview1.Model.IterNext (ref oiter)) 
                    if (treeview1.Model.GetValue(oiter, 2).ToString() == ID_REFBASE)
                        return oiter;
            }

            return new TreeIter();
        }

        public void Close()
        {
            listStore.Clear ();
        }

        public void AddItem(TreeIter iter, string path, bool exists, bool folder, bool expand)
        {
            string id = ID_FILE;
            Gdk.Pixbuf icon = ICON_FILE[Convert.ToInt32(!exists)];

            if (path.Contains ("/") || folder) {
                icon = ICON_FOLDER [Convert.ToInt32 (!exists)];
                id = ID_FOLDER;
            }

            string[] split = path.Split ('/');
            TreeIter itr;
            if (!GetIter (iter, split [0], out itr))
                itr = AddAndSort (iter, icon, split [0], id);
            else if(treeview1.Model.GetValue(itr, 0) == ICON_FOLDER[0])
                treeview1.Model.SetValue (itr, 0, ICON_FOLDER [Convert.ToInt32 (!exists)]);

            if(expand)
                treeview1.ExpandRow(treeview1.Model.GetPath(iter), false);

            if (split.Length > 1) {

                string newpath = split [1];
                for(int i = 2;i < split.Length;i++)
                    newpath += "/" + split[i];

                AddItem (itr, newpath, exists, folder,  expand);
            }
        }

        public TreeIter AddAndSort(TreeIter piter, Gdk.Pixbuf icon, string name, string id)
        {
            TreeIter oiter;
            int adde = Convert.ToInt32(piter.Equals(GetBaseIter()));
            List<string> list = new List<string>();

            if(treeview1.Model.IterChildren (out oiter, piter)) {

                if (treeview1.Model.GetValue (oiter, 2).ToString () == id)
                    list.Add (treeview1.Model.GetValue (oiter, 1).ToString ());
                else if (id == ID_FILE)
                    adde++;

                while (treeview1.Model.IterNext (ref oiter)) {
                    if (treeview1.Model.GetValue (oiter, 2).ToString() == id)
                        list.Add (treeview1.Model.GetValue (oiter, 1).ToString ());
                    else if (id == ID_FILE)
                        adde++;
                }
            }

            list.Add (name);
            list.Sort ();

            for (int i = 0; i < list.Count; i++) 
                if (list [i] == name) 
                    return listStore.InsertWithValues (piter, i + adde, icon, name, id);

            return listStore.AppendValues (piter, icon, name, id, "");
        }

        public void AddReference(string reff)
        {
            listStore.AppendValues(GetRefIter(), ICON_FILE[2], System.IO.Path.GetFileNameWithoutExtension(reff), ID_REF, reff);
        }

        public void RemoveItem(TreeIter iter, string path)
        {
            string[] split = path.Split ('/');
            TreeIter itr;

            if (GetIter (iter, split [0], out itr)) {
                if (split.Length > 1) {
                    string newpath = split [1];
                    for (int i = 2; i < split.Length; i++)
                        newpath += "/" + split [i];

                    RemoveItem (itr, newpath);
                } else if (treeview1.Model.GetValue(itr, 0) != ICON_BASE) 
                    listStore.Remove (ref itr);
            }
        }

        public void RemoveReferences(List<string> refs)
        {
            foreach (string reff in refs)
                RemoveReference(reff);
        }

        public void RemoveReference(string reff)
        {
            TreeIter refiter = GetRefIter();
            TreeIter oiter;

            if(treeview1.Model.IterChildren (out oiter, refiter)) {

                if (treeview1.Model.GetValue(oiter, 3).ToString() == reff)
                {
                    listStore.Remove(ref oiter);
                    return;
                }

                while (treeview1.Model.IterNext (ref oiter)) {
                    if (treeview1.Model.GetValue(oiter, 3).ToString() == reff)
                    {
                        listStore.Remove(ref oiter);
                        return;
                    }
                }
            }
        }

        public void RefreshItem(TreeIter iter, string path, bool exists)
        {
            string[] split = path.Split ('/');
            TreeIter itr;
            if (!GetIter (iter, split [0], out itr))
                return;
            else if(!exists)
                treeview1.Model.SetValue (itr, 0, ICON_FOLDER[1]);

            if (split.Length > 1) {
                string newpath = split [1];
                for (int i = 2; i < split.Length; i++)
                    newpath += "/" + split [i];

                RefreshItem (itr, newpath, exists);
            } else {
                Gdk.Pixbuf icon = ICON_FILE [Convert.ToInt32 (!exists)];
                treeview1.Model.SetValue (itr, 0, icon);

                if (exists) 
                    RefreshFolders (iter);
            }
        }

        public void RefreshFolders(TreeIter piter)
        {
            TreeIter oiter;

            if (treeview1.Model.GetValue (piter, 0) != ICON_FOLDER [1])
                return;

            if(treeview1.Model.IterChildren (out oiter, piter)) {
                if (treeview1.Model.GetValue (oiter, 0) == ICON_FILE [1])
                    return;

                while (treeview1.Model.IterNext (ref oiter))
                    if (treeview1.Model.GetValue (oiter, 0) == ICON_FILE [1])
                        return;
            }

            treeview1.Model.SetValue (piter, 0, ICON_FOLDER [0]);
            treeview1.Model.IterParent (out oiter, piter);

            RefreshFolders (oiter);
        }

        public bool GetIter(TreeIter iter, string name, out TreeIter oiter)
        {
            if(treeview1.Model.IterChildren (out oiter, iter)) {
                if (treeview1.Model.GetValue(oiter, 1).ToString() == name && treeview1.Model.GetValue(oiter, 2).ToString() != ID_REFBASE)
                    return true;

                while (treeview1.Model.IterNext(ref oiter))
                    if (treeview1.Model.GetValue(oiter, 1).ToString() == name && treeview1.Model.GetValue(oiter, 2).ToString() != ID_REFBASE)
                        return true;
            }

            return false;
        }

        string GetPathFromIter(TreeIter iter)
        {
            string[] split = treeview1.Model.GetStringFromIter (iter).Split (':');
            string filePath = "";
            TreeIter piter = iter;

            try {
                filePath = treeview1.Model.GetValue (iter, 1).ToString ();
            } catch {
            }

            for (int i = 1; i < split.Length - 1; i++) {
                treeview1.Model.IterParent (out piter, piter);
                filePath = string.Format("{0}/{1}", treeview1.Model.GetValue(piter, 1), filePath);
            }

            return filePath;
        }

        public string[] GetSelectedTreePath(out List<TreeIter> iters, out List<string> ids)
        {
            TreePath[] paths = treeview1.Selection.GetSelectedRows ();

            var filePaths = new List<string>();
            iters = new List<TreeIter> ();
            ids = new List<string> ();

            for (int i = 0; i < paths.Length; i++)
            {
                TreeIter iter;
                if (treeview1.Model.GetIter(out iter, paths[i]))
                {
                    filePaths.Add(GetPathFromIter(iter));
                    iters.Add(iter);
                    ids.Add (treeview1.Model.GetValue(iter, 2).ToString());
                }
            }

            return filePaths.ToArray();
        }

        List<string> GetAllPaths(TreeIter iter)
        {
            TreeIter oiter;
            var paths = new List<string> ();

            if(treeview1.Model.IterChildren (out oiter, iter)) {
                do {
                    if (treeview1.Model.GetValue(oiter, 2).ToString() == ID_FILE)
                        paths.Add (GetPathFromIter (oiter));
                    else
                        paths.AddRange (GetAllPaths (oiter));
                } while (treeview1.Model.IterNext (ref oiter));
            }

            return paths;
        }

        public void Remove()
        {
            List<TreeIter> iter;
            List<string> ids;
            string[] path = GetSelectedTreePath (out iter, out ids);

            var items = new List<ContentItem>();
            var directories = new List<string>();
            var refs = new List<string>();

            for (int i = 0; i < path.Length; i++) {
                if (ids [i] == ID_FILE) {
                    var item = window._controller.GetItem (path [i]) as ContentItem;
                    if(!items.Contains(item))
                        items.Add (item);
                } else if (ids[i] == ID_REFBASE) {
                    //remove all refs?
                } else if (ids[i] == ID_REF) {
                    refs.Add(treeview1.Model.GetValue(iter[i], 3).ToString());
                } else {
                    if (ids[i] == ID_FOLDER)
                        directories.Add(path[i]);

                    List<string> paths = GetAllPaths (iter [i]);
                    foreach (string pth in paths) {
                        var item = window._controller.GetItem (pth) as ContentItem;
                        if(!items.Contains(item))
                            items.Add (item);
                    }

                    TreeIter itr = iter [i];
                }
            }

            if(items.Count > 0 || directories.Count > 0 || refs.Count > 0)
                window._controller.Exclude (items, directories, refs);
        }

        public void Rebuild()
        {
            List<TreeIter> iter;
            List<string> ids;
            string[] path = GetSelectedTreePath (out iter, out ids);

            var items = new List<ContentItem>();

            for (int i = 0; i < path.Length; i++) {
                if (ids [i] == ID_FILE) {
                    var item = window._controller.GetItem (path [i]) as ContentItem;
                    if(!items.Contains(item))
                        items.Add (item);
                } else {
                    List<string> paths = GetAllPaths (iter [i]);
                    foreach (string pth in paths) {
                        var item = window._controller.GetItem (pth) as ContentItem;
                        if(!items.Contains(item))
                            items.Add (item);
                    }

                    TreeIter itr = iter [i];
                }
            }

            if(items.Count > 0)
                window._controller.RebuildItems (items);
        }

        protected void OnTreeview1ButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
        {
            if (!window._controller.ProjectOpen)
                return;

            if (args.Event.Button == 1) 
                ReloadPropertyGrid ();
            else if (args.Event.Button == 3) 
                ShowMenu ();
        }

        [GLib.ConnectBefore]
        protected void OnTreeview1ButtonPressEvent (object o, ButtonPressEventArgs args)
        {
            if (args.Event.Type == Gdk.EventType.TwoButtonPress && args.Event.Button == 1)
            {
                List<TreeIter> iters;
                List<string> ids;
                GetSelectedTreePath(out iters, out ids);

                if (ids.Count != 1)
                    return;

                if(ids[0] == ID_BASE)
                    Process.Start(openedProject);
                else if(ids[0] != ID_FOLDER)
                    Process.Start(window._controller.GetFullPath(GetPathFromIter(iters[0])));
            }

            if (args.Event.Button == 3) {
                var paths = new List<TreePath> ();
                paths.AddRange (treeview1.Selection.GetSelectedRows ());

                TreeViewDropPosition pos; 
                TreePath path;

                treeview1.GetDestRowAtPos ((int)args.Event.X, (int)args.Event.Y, out path, out pos);

                if (paths.Contains (path)) {
                    args.RetVal = true;
                    return;
                }
            }

            args.RetVal = false;
        }

        protected void OnTreeview1CursorChanged (object o, EventArgs args)
        {
            ReloadPropertyGrid ();
            window.UpdateMenus();
        }

        string CombineVariables(string vara, string varb)
        {
            if (vara == "????" || vara == varb)
                return varb;
            return "";
        }

        void ReloadPropertyGrid()
        {
            window._controller.Selection.Clear (window);
            List<TreeIter> iters;
            List<string> ids;
            var paths = GetSelectedTreePath (out iters, out ids);

            if (ids.Contains(ID_REFBASE))
            {
                propertiesView.Clear();
                return;
            }

            var project = (PipelineProject)window._controller.GetItem(openedProject);
            bool ps = false;

            var citems = new List<ContentItem> ();
            var dirpaths = new List<string> ();
            string name = "????";
            string location = "????";

            for(int i = 0;i < paths.Length;i++)
            {
                if (ids[i] == ID_BASE) {
                    ps = true;
                    name = CombineVariables(name, treeview1.Model.GetValue(iters[i], 1).ToString());
                    location = CombineVariables(location, project.Location);
                }
                else if (ids[i] == ID_REF) {
                    name = CombineVariables(name, treeview1.Model.GetValue(iters[i], 1).ToString());
                    location = CombineVariables(location, System.IO.Path.GetDirectoryName(treeview1.Model.GetValue(iters[i], 3).ToString()));
                }
                else {
                    var item = window._controller.GetItem (paths [i]);

                    if (item as ContentItem != null) {
                        citems.Add (item as ContentItem);
                        window._controller.Selection.Add(item, window);
                    } else
                        dirpaths.Add (paths[i]);

                    name = CombineVariables (name, treeview1.Model.GetValue (iters [i], 1).ToString ());
                    TreeIter piter;
                    treeview1.Model.IterParent (out piter, iters [i]);
                    location = CombineVariables (location, treeview1.Model.GetValue (piter, 1).ToString ());
                }
            }

            if (citems.Count > 0 && !ps && dirpaths.Count == 0) {
                var objs = new List<object> ();
                objs.AddRange (citems.ToArray ());
                propertiesView.Load (objs, name, location);
            }
            else if (citems.Count == 0 && ps && dirpaths.Count == 0) {
                var objs = new List<object> ();
                objs.Add (project);
                propertiesView.Load (objs, name, location);
            }
            else
                propertiesView.Load(null, name, location);
        }

        void ShowMenu()
        {
            List<TreeIter> iters;
            List<string> ids;
            var paths = new List<string> ();
            paths.AddRange (GetSelectedTreePath (out iters, out ids));

            if (paths.Count == 0)
                return;
            else if (paths.Count == 1) {
                if (paths[0] != null) {
                    window.UpdateMenus ();

                    menu.ShowAll ();

                    treeadd.Visible = false;
                    treeaddref.Visible = false;

                    if (ids[0] == ID_BASE)
                    {
                        treeadd.Visible = true;
                    }
                    else if (ids[0] == ID_FOLDER)
                    {
                        treeopenfile.Visible = false;
                        treeadd.Visible = true;
                    }
                    else if (ids[0] == ID_REFBASE)
                    {
                        treeopenfile.Visible = false;
                        treeaddref.Visible = true;
                        treeopenfilelocation.Visible = false;
                        treerebuild.Visible = false;
                    }
                    else if (ids[0] == ID_REF)
                    {
                        treerebuild.Visible = false;
                        treeopenfile.Visible = false;
                        treeaddref.Visible = true;
                    }

                    treeaddseparator.Visible = treeadd.Visible || treeopenfile.Visible;
                    treedeleteseparator.Visible = treedelete.Visible;

                    menu.Popup ();
                }
            } else {
                menu.ShowAll ();

                treeaddref.Visible = false;
                treeadd.Visible = false;
                treeopenfile.Visible = false;
                treeaddseparator.Visible = false;
                treeopenfile.Visible = false;
                treeopenfilelocation.Visible = false;

                if (ids.Contains(ID_REFBASE) || ids.Contains(ID_REF))
                {
                    treerebuild.Visible = false;
                    treedeleteseparator.Visible = false;
                }

                menu.Popup ();
            }
        }
    }
}

