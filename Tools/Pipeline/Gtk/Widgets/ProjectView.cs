using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gtk;
using System.Collections;

namespace MonoGame.Tools.Pipeline
{
    [System.ComponentModel.ToolboxItem (true)]
    partial class ProjectView : VBox
    {
        public Menu menu, addmenu;
        public string openedProject;

        public string ID_BASE = "0", ID_FOLDER = "1", ID_FILE = "2";

        public Gdk.Pixbuf ICON_BASE = new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.settings.png");
        public Gdk.Pixbuf[] ICON_FOLDER = {
            IconCache.GetFolderIcon(),
            new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.folder_missing.png")
        };
        public Gdk.Pixbuf[] ICON_FILE = { 
            new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.blueprint.png"), 
            new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.missing.png")
        };

        string basename;
        TreeStore listStore;
        MainWindow window;
        PropertiesView propertiesView;

        MenuItem treeadd, treeaddseperator, treenewitem, treeadditem, treenewfolder, treeaddfolder, treeopenfile, treerename, treedelete, treeopenfilelocation;

        public ProjectView ()
        {
            Build();

            basename = "base";

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

            treeview1.Reorderable = true;
            treeview1.Model = listStore;
            treeview1.Selection.Mode = SelectionMode.Multiple;

            treeview1.ButtonPressEvent += OnTreeview1ButtonPressEvent;
            treeview1.KeyReleaseEvent += HandleKeyReleaseEvent;
            treeview1.ButtonReleaseEvent += OnTreeview1ButtonReleaseEvent;
            treeview1.CursorChanged += OnTreeview1CursorChanged;

            Drag.SourceSet (this.treeview1, 0, null, Gdk.DragAction.Move);
            Drag.DestSet (this.treeview1, 0, null, 0);

            treeview1.DragBegin += Treeview1_DragBegin;
            treeview1.DragMotion += Treeview1_DragMotion;
            treeview1.DragDrop += Treeview1_DragDrop;
            treeview1.DragDataReceived += Treeview1_DragDataReceived;
            treeview1.DragEnd += Treeview1_DragEnd;
        }

        void HandleKeyReleaseEvent (object o, KeyReleaseEventArgs args)
        {
            if (args.Event.Key.ToString () == "Menu") 
                ShowMenu ();
        }

        public void Initalize(MainWindow window, MenuItem treerebuild, PropertiesView propertiesView)
        {
            this.window = window;
            this.propertiesView = propertiesView;

            menu = new Menu ();
            addmenu = new Menu();

            treeadd = new MenuItem("Add");
            treeadd.Submenu = addmenu;

            treeaddseperator = new SeparatorMenuItem ();

            treenewitem = new MenuItem ("New Item...");
            treenewitem.ButtonPressEvent += delegate(object o, ButtonPressEventArgs args)
                { menu.Popdown(); window.OnNewItemActionActivated(o, args); };

            treenewfolder = new MenuItem ("New Folder...");
            treenewfolder.ButtonPressEvent += delegate(object o, ButtonPressEventArgs args)
                { menu.Popdown(); window.OnNewFolderActionActivated(o, args); };

            treeadditem = new MenuItem ("Existing Item...");
            treeadditem.ButtonPressEvent += window.OnAddItemActionActivated;

            treeaddfolder = new MenuItem ("Existing Folder...");
            treeaddfolder.ButtonPressEvent += window.OnAddFolderActionActivated;
            
            treerename = new MenuItem ("Rename");
            treerename.Activated += window.OnRenameActionActivated;

            treedelete = new MenuItem ("Delete");
            treedelete.Activated += window.OnDeleteActionActivated;

            treeopenfile = new MenuItem ("Open");
            treeopenfile.Activated += delegate {
                List<TreeIter> iters;
                List<string> ids;
                GetSelectedTreePath(out iters, out ids);

                if (ids.Count != 1)
                    return;

                string start = openedProject;

                if(ids[0] != ID_BASE)
                    start = window._controller.GetFullPath(GetPathFromIter(iters[0]));

                #if LINUX
                Process.Start("xdg-open", start);
                #else
                Process.Start(start);
                #endif
            };

            treeopenfilelocation = new MenuItem ("Open Item Directory");
            treeopenfilelocation.Activated += delegate {
                List<TreeIter> iters;
                List<string> ids;
                GetSelectedTreePath(out iters, out ids);

                if(ids[0] != ID_BASE)
                    Process.Start(System.IO.Path.GetDirectoryName(window._controller.GetFullPath(GetPathFromIter(iters[0]))));
                else
                    Process.Start(System.IO.Path.GetDirectoryName(window._controller.GetFullPath("")));
            };

            addmenu.Add (treenewitem);
            addmenu.Add (treenewfolder);
            addmenu.Add (new SeparatorMenuItem ());
            addmenu.Add (treeadditem);
            addmenu.Add (treeaddfolder);

            menu.Add (treeopenfile);
            menu.Add (treeadd);
            menu.Add (treeaddseperator);
            menu.Add (treeopenfilelocation);
            menu.Add (treerebuild);
            menu.Add (new SeparatorMenuItem ());
            menu.Add (treerename);
            menu.Add (treedelete);
        }

        public void SetBaseIter(string name)
        {
            basename = name;
            TreeIter iter = GetBaseIter ();
            treeview1.Model.SetValue (iter, 1, name);
            window.ReloadTitle();
        }

        public TreeIter GetBaseIter()
        {
            TreeIter iter;
        
            if(!treeview1.Model.GetIterFromString (out iter, "0"))
                iter = listStore.AppendValues (ICON_BASE, basename, ID_BASE);

            return iter;
        }

        public void ExpandBase()
        {
            treeview1.ExpandRow(treeview1.Model.GetPath(GetBaseIter()), false);
        }

        public void Close()
        {
            listStore.Clear ();
        }

        public void AddItem(TreeIter iter, string path, bool exists, bool folder, bool expand, string fullpath)
        {
            string id = ID_FILE;
            Gdk.Pixbuf icon = ICON_FILE[Convert.ToInt32(!exists)];

            if (path.Contains ("/") || folder) {
                icon = ICON_FOLDER [Convert.ToInt32 (!exists)];
                id = ID_FOLDER;
            }
            else if(exists)
                icon = IconCache.GetIcon(window._controller.GetFullPath(fullpath));

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

                AddItem (itr, newpath, exists, folder, expand, fullpath);
            }
        }

        public TreeIter AddAndSort(TreeIter piter, Gdk.Pixbuf icon, string name, string id)
        {
            TreeIter oiter;
            int adde = 0;
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

            return listStore.AppendValues (piter, icon, name, id);
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

        public void RefreshItem(TreeIter iter, string path, bool exists, string fullpath)
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

                RefreshItem (itr, newpath, exists, fullpath);
            } else {
                Gdk.Pixbuf icon = ICON_FILE [Convert.ToInt32 (!exists)];

                if (exists)
                    icon = IconCache.GetIcon(window._controller.GetFullPath(fullpath));

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

                if (treeview1.Model.GetValue (oiter, 1).ToString() == name)
                    return true;

                while (treeview1.Model.IterNext (ref oiter)) 
                    if (treeview1.Model.GetValue (oiter, 1).ToString() == name)
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

        public void Rename()
        {
            List<TreeIter> iter;
            List<string> ids;
            string[] path = GetSelectedTreePath (out iter, out ids);

            if (path.Length != 1)
                return;

            FileType type = FileType.Base;

            if (ids [0] == ID_FILE) 
                type = FileType.File;
            else if (ids [0] == ID_FOLDER) 
                type = FileType.Folder;

            TextEditorDialog dialog = new TextEditorDialog (window, "Rename", "New Name:", treeview1.Model.GetValue (iter [0], 1).ToString(), true);

            if (dialog.Run() == (int)ResponseType.Ok)
            {
                string newpath = System.IO.Path.GetDirectoryName(path[0]) + System.IO.Path.DirectorySeparatorChar + dialog.text;
                window._controller.Move(new[] { path[0] }, new[] { newpath.StartsWith(System.IO.Path.DirectorySeparatorChar.ToString()) ? newpath.Substring(1) : newpath }, new[] { type });
            }
        }
        
        public void Remove()
        {
            List<TreeIter> iter;
            List<string> ids;
            string[] path = GetSelectedTreePath (out iter, out ids);

            var items = new List<ContentItem>();
            var directories = new List<string>();

            for (int i = 0; i < path.Length; i++) {
                if (ids [i] == ID_FILE) {
                    var item = window._controller.GetItem (path [i]) as ContentItem;
                    if(!items.Contains(item))
                        items.Add (item);
                } else {
                    if (ids[i] == ID_FOLDER)
                        directories.Add(path[i]);

                    List<string> paths = GetAllPaths (iter [i]);
                    foreach (string pth in paths) {
                        var item = window._controller.GetItem (pth) as ContentItem;
                        if (item == null && !directories.Contains(pth))
                            directories.Add(pth);
                        else if(!items.Contains(item))
                            items.Add (item);
                    }
                }
            }

            if(items.Count > 0 || directories.Count > 0)
                window._controller.Exclude (items, directories);
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

            window.UpdateMenus();
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

                if (ids[0] == ID_FILE)
                {
                    string start = window._controller.GetFullPath(GetPathFromIter(iters[0]));

                    #if LINUX
                    Process.Start("xdg-open", start);
                    #else
                    Process.Start(start);
                    #endif
                }
                else
                {
                    bool expanded = treeview1.GetRowExpanded(treeview1.Model.GetPath(iters[0]));

                    if (!expanded)
                        treeview1.ExpandRow(treeview1.Model.GetPath(iters[0]), false);
                    else
                        treeview1.CollapseRow(treeview1.Model.GetPath(iters[0]));
                }
            }
            else if (args.Event.Button == 3)
            {
                var paths = new List<TreePath>();
                paths.AddRange(treeview1.Selection.GetSelectedRows());

                TreeViewDropPosition pos; 
                TreePath path;

                treeview1.GetDestRowAtPos((int)args.Event.X, (int)args.Event.Y, out path, out pos);

                if (paths.Contains(path))
                {
                    args.RetVal = true;
                    return;
                }
            }

            args.RetVal = false;
        }

        List<TreeIter> siters = new List<TreeIter>();

        void Treeview1_DragBegin (object o, DragBeginArgs args)
        {
            List<string> ids;
            GetSelectedTreePath(out siters, out ids);

            if (siters.Contains(GetBaseIter()))
                siters = new List<TreeIter>();
        }

        void Treeview1_DragMotion (object o, DragMotionArgs args)
        {
            TreeViewDropPosition pos;
            TreePath path;
            TreeIter iter;

            if (!treeview1.GetDestRowAtPos(args.X, args.Y, out path, out pos))
                return;
            
            if (!listStore.GetIter(out iter, path))
                return;

            if (treeview1.Model.GetValue(iter, 2).ToString() != ID_FILE)
                treeview1.SetDragDestRow(path, pos);
            else if (pos == TreeViewDropPosition.IntoOrBefore || pos == TreeViewDropPosition.Before)
                treeview1.SetDragDestRow(path, TreeViewDropPosition.Before);
            else
                treeview1.SetDragDestRow(path, TreeViewDropPosition.After);

            Gdk.Drag.Status (args.Context, args.Context.SuggestedAction, args.Time); 
            args.RetVal = true;
        }

        void Treeview1_DragEnd (object o, DragEndArgs args)
        {
            siters = new List<TreeIter>();
        }

        void Treeview1_DragDrop (object o, DragDropArgs args)
        {
            args.RetVal = true;

            #if GTK3
            if(args.Context.ListTargets().Length > 0)
                Drag.GetData ((Widget)o, args.Context, args.Context.ListTargets()[0], args.Time);
            #else
            if(args.Context.Targets.Length > 0)
                Drag.GetData ((Widget)o, args.Context, args.Context.Targets[0], args.Time);
            #endif

            var siters = this.siters;
            this.siters = new List<TreeIter>();

            if (siters.Count == 0)
                return;
            
            TreeViewDropPosition pos;
            TreePath path;
            TreeIter iter;

            if (treeview1.GetDestRowAtPos(args.X, args.Y, out path, out pos))
            {
                if (!listStore.GetIter(out iter, path))
                    return;
            }
            else
            {
                iter = GetBaseIter();
                pos = TreeViewDropPosition.IntoOrAfter;
            }

            if (treeview1.Model.GetValue(iter, 2).ToString() == ID_FILE)
            {
                if (pos == TreeViewDropPosition.IntoOrBefore || pos == TreeViewDropPosition.Before)
                    pos = TreeViewDropPosition.Before;
                else
                    pos = TreeViewDropPosition.After;
            }

            if (pos == TreeViewDropPosition.After || pos == TreeViewDropPosition.Before)
                if (!treeview1.Model.IterParent(out iter, iter))
                    return;

            string dest_folder = (iter.Equals(GetBaseIter())) ? "" : GetPathFromIter(iter) + "/";

            List<string> sourcedata = new List<string>();
            List<string> destdata = new List<string>();
            List<FileType> types = new List<FileType>();

            foreach(var si in siters)
            {
                string spath = GetPathFromIter(si);
                string dpath = dest_folder + System.IO.Path.GetFileName(spath);

                sourcedata.Add(spath);
                destdata.Add(dpath);

                types.Add((treeview1.Model.GetValue(si, 2).ToString() == ID_FILE) ? FileType.File : FileType.Folder);
            }

            window._controller.Move(sourcedata.ToArray(), destdata.ToArray(), types.ToArray());
            window.UpdateMenus();
        }

        void Treeview1_DragDataReceived (object o, DragDataReceivedArgs args)
        {
            args.RetVal = true;

            if (args.SelectionData.Length > 0 && args.SelectionData.Format == 8) {

                byte[] data = args.SelectionData.Data;
                string encoded = System.Text.Encoding.UTF8.GetString (data);
                var paths = new List<string> (encoded.Split ('\r', '\n'));

                var files = new List<string>();
                var folders = new List<string>();

                for (int i = 0; i < paths.Count; ++i)
                {
                    if (paths[i].StartsWith("file://"))
                    {
                        string path = paths[i].Substring(7);

                        try
                        {
                            //convert hex to ascii if any
                            for(int ih = 0;ih < path.Length;ih++)
                            {
                                if (path.Substring(ih, 1) == "%")
                                {
                                    string hex = path.Substring(ih + 1, 2);
                                    path = path.Replace("%" + hex, Convert.ToChar(Convert.ToUInt32(hex, 16)).ToString());
                                }
                            }
                        }
                        catch
                        {
                            window.ShowError("Error", "An unknown error has occured.");
                            return;
                        }

                        if (System.IO.File.Exists(path))
                            files.Add(path);
                        else
                            folders.Add(path);
                    }
                }

                TreeViewDropPosition pos;
                TreePath tpath;
                TreeIter iter;

                if (treeview1.GetDestRowAtPos(args.X, args.Y, out tpath, out pos))
                {
                    if (!listStore.GetIter(out iter, tpath))
                        return;
                }
                else
                {
                    iter = GetBaseIter();
                    pos = TreeViewDropPosition.IntoOrAfter;
                }

                if (treeview1.Model.GetValue(iter, 2).ToString() == ID_FILE)
                {
                    if (pos == TreeViewDropPosition.IntoOrBefore || pos == TreeViewDropPosition.Before)
                        pos = TreeViewDropPosition.Before;
                    else
                        pos = TreeViewDropPosition.After;
                }

                if (pos == TreeViewDropPosition.After || pos == TreeViewDropPosition.Before)
                if (!treeview1.Model.IterParent(out iter, iter))
                    return;

                string initDir = (iter.Equals(GetBaseIter())) ? window._controller.GetFullPath ("") : GetPathFromIter(iter);
                window._controller.DragDrop(initDir, folders.ToArray(), files.ToArray());
                window.UpdateMenus();
            }
        }

        protected void OnTreeview1CursorChanged (object o, EventArgs args)
        {
            ReloadPropertyGrid ();
        }

        string CombineVariables(string vara, string varb)
        {
            if (vara == "????" || vara == varb)
                return varb;
            return "";
        }

        void ReloadPropertyGrid()
        {
            string name = "????";
            string location = "????";

            if (!window._controller.ProjectOpen)
            {
                propertiesView.Load(null, name, location);
                return;
            }

            window._controller.Selection.Clear (window);
            List<TreeIter> iters;
            List<string> ids;
            var paths = GetSelectedTreePath (out iters, out ids);

            var project = (PipelineProject)window._controller.GetItem(openedProject);
            bool ps = false;

            var citems = new List<ContentItem> ();
            var dirpaths = new List<string> ();

            for(int i = 0;i < paths.Length;i++)
            {
                if (ids [i] == ID_BASE) {
                    ps = true;
                    name = CombineVariables (name, treeview1.Model.GetValue (iters [i], 1).ToString ());
                    location = CombineVariables (location, project.Location);
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
                    if (ids[0] == ID_BASE) {
                        treeadd.Visible = true;
                        treeopenfile.Visible = true;
                    } else if (ids[0] == ID_FOLDER) {
                        treeadd.Visible = true;
                        treeopenfile.Visible = false;
                    } else {
                        treeadd.Visible = false;
                        treeopenfile.Visible = true;
					}
                    treerename.Visible = true;
                    treeaddseperator.Visible = treeadd.Visible || treeopenfile.Visible;

                    menu.Popup ();
                }
            } else {
                menu.ShowAll ();

                treeadd.Visible = false;
                treeopenfile.Visible = false;
                treeaddseperator.Visible = false;
                treeopenfile.Visible = false;
                treeopenfilelocation.Visible = false;
                treerename.Visible = false;

                menu.Popup ();
            }
        }
    }
}

