using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
	[System.ComponentModel.ToolboxItem (true)]
	partial class ProjectView : Gtk.Bin
	{
		public Menu menu;
		public string openedProject;

		public Gdk.Pixbuf ICON_BASE = new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.settings.png");
		public Gdk.Pixbuf ICON_FOLDER = new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.folder_closed.png");
		public Gdk.Pixbuf[] ICON_OTHER = new Gdk.Pixbuf[] { 
			new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.blueprint.png"), 
			new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.missing.png")
		};

		string basename;
		TreeStore listStore;
		MainWindow window;
		PropertiesView propertiesView;

		MenuItem treenewitem, treeadditem, treeopenfile, treedelete, treeopenfilelocation;
		SeparatorMenuItem seperator, seperator2;

		public ProjectView ()
		{
			this.Build ();
			basename = "base";

			Gtk.TreeViewColumn column = new Gtk.TreeViewColumn ();

			Gtk.CellRendererPixbuf iconCell = new Gtk.CellRendererPixbuf ();
			Gtk.CellRendererText textCell = new Gtk.CellRendererText ();

			column.PackStart (iconCell, false);
			column.PackStart (textCell, false);

			treeview1.AppendColumn (column);

			column.AddAttribute (iconCell,  "pixbuf", 0);
			column.AddAttribute (textCell, "text", 1);

			listStore = new Gtk.TreeStore (typeof (Gdk.Pixbuf), typeof (string));
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
			this.propertiesView = propertiesView;

			menu = new Menu ();

			treenewitem = new MenuItem ("New Item...");
			treenewitem.Activated += window.OnNewItemActionActivated;
			menu.Add (treenewitem);

			treeadditem = new MenuItem ("Add Item...");
			treeadditem.Activated += window.OnAddItemActionActivated;
			menu.Add (treeadditem);

			treedelete = new MenuItem ("Delete");
			treedelete.Activated += window.OnDeleteActionActivated;
			menu.Add (treedelete);

			seperator = new SeparatorMenuItem ();
			menu.Add (seperator);

			treeopenfile = new MenuItem ("Open File");
			treeopenfile.Activated += delegate {
				List<TreeIter> iters;
				List<Gdk.Pixbuf> icons;
				GetSelectedTreePath(out iters, out icons);

				if(icons[0] != ICON_BASE)
					Process.Start(window._controller.GetFullPath(GetPathFromIter(iters[0])));
				else
					Process.Start(openedProject);
			};
			menu.Add (treeopenfile);

			treeopenfilelocation = new MenuItem ("Open File Location");
			treeopenfilelocation.Activated += delegate {
				List<TreeIter> iters;
				List<Gdk.Pixbuf> icons;
				GetSelectedTreePath(out iters, out icons);

				if(icons[0] != ICON_BASE)
					Process.Start(System.IO.Path.GetDirectoryName(window._controller.GetFullPath(GetPathFromIter(iters[0]))));
				else
					Process.Start(System.IO.Path.GetDirectoryName(window._controller.GetFullPath("")));
			};
			menu.Add (treeopenfilelocation);

			seperator2 = new SeparatorMenuItem ();
			menu.Add (seperator2);

			menu.Add (treerebuild);
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
		
			if(!treeview1.Model.GetIterFromString (out iter, "0"))
				iter = listStore.AppendValues (ICON_BASE, basename);

			return iter;
		}

		public void Close()
		{
			listStore.Clear ();
		}

		public void AddItem(TreeIter iter, string path, bool exists)
		{
			Gdk.Pixbuf icon = ICON_OTHER[Convert.ToInt32(!exists)];

			if (path.Contains ("/")) 
				icon = ICON_FOLDER;

			string[] split = path.Split ('/');
			TreeIter itr;
			if (!GetIter (iter, split [0], out itr))
				itr = listStore.AppendValues (iter, icon, split [0]);

            treeview1.ExpandRow(treeview1.Model.GetPath(iter), false);
			if (split.Length > 1) {
				string newpath = split [1];
				for(int i = 2;i < split.Length;i++)
					newpath += "/" + split[i];

				AddItem (itr, newpath, exists);
			}
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
				} else 
					RemoveIterAndUneededParents (itr);
			}
		}

		public void RefreshItem(TreeIter iter, string path, bool exists)
		{
			string[] split = path.Split ('/');
			TreeIter itr;
			if (!GetIter (iter, split [0], out itr))
				return;

			if (split.Length > 1) {
				string newpath = split [1];
				for (int i = 2; i < split.Length; i++)
					newpath += "/" + split [i];

				RefreshItem (itr, newpath, exists);
			} else {
				Gdk.Pixbuf icon = ICON_OTHER [Convert.ToInt32 (!exists)];
				treeview1.Model.SetValue (itr, 0, icon);
			}
		}

		public void RemoveIterAndUneededParents(TreeIter iter)
		{
			if ((Gdk.Pixbuf)treeview1.Model.GetValue (iter, 0) != ICON_BASE) {
				TreeIter piter;

				treeview1.Model.IterParent (out piter, iter);
				int nc = treeview1.Model.IterNChildren (piter);
				listStore.Remove (ref iter);

				if (nc <= 1) 
					RemoveIterAndUneededParents (piter);
				
			}
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

		private string GetPathFromIter(TreeIter iter)
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
				filePath = treeview1.Model.GetValue (piter, 1).ToString () + "/" + filePath;
			}

			return filePath;
		}

		public string[] GetSelectedTreePath(out List<TreeIter> iters, out List<Gdk.Pixbuf> icons)
		{
			TreePath[] paths = treeview1.Selection.GetSelectedRows ();

			List<string> filePaths = new List<string>();
			iters = new List<TreeIter> ();
			icons = new List<Gdk.Pixbuf> ();

			foreach (TreePath path in paths) {

				TreeIter iter;
				if (treeview1.Model.GetIter (out iter, path)) {
					filePaths.Add (GetPathFromIter (iter));
					iters.Add (iter);
					icons.Add ((Gdk.Pixbuf)treeview1.Model.GetValue (iter, 0));
				}
			}

			return filePaths.ToArray();
		}

		private List<string> GetAllPaths(TreeIter iter)
		{
			TreeIter oiter;
			List<string> paths = new List<string> ();

			if(treeview1.Model.IterChildren (out oiter, iter)) {
				do {
					if ((Gdk.Pixbuf)treeview1.Model.GetValue (oiter, 0) == ICON_OTHER[0] || (Gdk.Pixbuf)treeview1.Model.GetValue (oiter, 0) == ICON_OTHER[1])
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
			List<Gdk.Pixbuf> icon;
			string[] path = GetSelectedTreePath (out iter, out icon);

			List<ContentItem> items = new List<ContentItem>();

			for (int i = 0; i < path.Length; i++) {
				if (icon [i] == ICON_OTHER[0] || icon [i] == ICON_OTHER[1]) {
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
				window._controller.Exclude (items);
		}

		public void Rebuild()
		{
			List<TreeIter> iter;
			List<Gdk.Pixbuf> icon;
			string[] path = GetSelectedTreePath (out iter, out icon);

			List<ContentItem> items = new List<ContentItem>();

			for (int i = 0; i < path.Length; i++) {
				if (icon [i] == ICON_OTHER[0] || icon [i] == ICON_OTHER[1]) {
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
			if (args.Event.Button == 3) {
				List<TreePath> paths = new List<TreePath> ();
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

		private string CombineVariables(string vara, string varb)
		{
			if (vara == "????" || vara == varb)
				return varb;
			return "";
		}

		private void ReloadPropertyGrid()
		{
			window._controller.Selection.Clear (window);
			List<TreeIter> iters;
			List<Gdk.Pixbuf> icons;
			List<string> paths = new List<string> ();
			paths.AddRange (GetSelectedTreePath (out iters, out icons));

			PipelineProject project = (PipelineProject)window._controller.GetItem(openedProject);
			bool ps = false;

			List<ContentItem> citems = new List<ContentItem> ();
			List<string> dirpaths = new List<string> ();
			string name = "????";
			string location = "????";

			for(int i = 0;i < paths.Count;i++)
			{
				if (icons [i] == ICON_BASE) {
					ps = true;
					name = CombineVariables (name, treeview1.Model.GetValue (iters [i], 1).ToString ());
					location = CombineVariables (location, project.Location);
				}
				else {
					var item = window._controller.GetItem (paths [i]);

					if (item as ContentItem != null) {
						citems.Add (item as ContentItem);
						window._controller.Selection.Add (item as ContentItem, window);
					} else
						dirpaths.Add (paths[i]);

					name = CombineVariables (name, treeview1.Model.GetValue (iters [i], 1).ToString ());
					TreeIter piter;
					treeview1.Model.IterParent (out piter, iters [i]);
					location = CombineVariables (location, treeview1.Model.GetValue (piter, 1).ToString ());
				}
			}

			if (citems.Count > 0 && !ps && dirpaths.Count == 0) {
				List<object> objs = new List<object> ();
				objs.AddRange (citems.ToArray ());
				propertiesView.Load (objs, name, location);
			}
			else if (citems.Count == 0 && ps && dirpaths.Count == 0) {
				List<object> objs = new List<object> ();
				objs.Add (project);
				propertiesView.Load (objs, name, location);
			}
			else
				propertiesView.Load(null, name, location);
		}

		private void ShowMenu()
		{
			List<TreeIter> iters;
			List<Gdk.Pixbuf> icons;
			List<string> paths = new List<string> ();
			paths.AddRange (GetSelectedTreePath (out iters, out icons));

			if (paths.Count == 0)
				return;
			else if (paths.Count == 1) {
				if (paths[0] != null) {
					window.UpdateMenus ();

					menu.ShowAll ();
					if (icons[0] == ICON_BASE) {
						treenewitem.Visible = true;
						treeadditem.Visible = true;
						treeopenfile.Visible = true;
					} else if (icons[0] == ICON_FOLDER) {
						treenewitem.Visible = true;
						treeadditem.Visible = true;
						treeopenfile.Visible = false;
					} else {
						treenewitem.Visible = false;
						treeadditem.Visible = false;
						treeopenfile.Visible = true;
					}

					menu.Popup ();
				}
			} else {
				menu.ShowAll ();

				treenewitem.Visible = false;
				treeadditem.Visible = false;
				treeopenfile.Visible = false;
				seperator.Visible = false;
				treeopenfile.Visible = false;
				treeopenfilelocation.Visible = false;

				menu.Popup ();
			}
		}
	}
}

