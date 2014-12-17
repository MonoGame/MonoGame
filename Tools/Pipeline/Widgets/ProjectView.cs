using System.Collections.Generic;
using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
	[System.ComponentModel.ToolboxItem (true)]
	partial class ProjectView : Gtk.Bin
	{
		public Menu menu;

		Gdk.Pixbuf ICON_BASE = new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.settings.png");
		Gdk.Pixbuf ICON_FOLDER = new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.folder_closed.png");
		Gdk.Pixbuf ICON_OTHER = new Gdk.Pixbuf (null, "MonoGame.Tools.Pipeline.Icons.blueprint.png");

		string basename;
		TreeStore listStore;
		MainWindow window;

		MenuItem treenewitem, treeadditem, treeopenfile, treerebuild;

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
		}

		public void Initalize(MainWindow window, MenuItem treerebuild)
		{
			this.window = window;

			menu = new Menu ();

			treenewitem = new MenuItem ("New Item...");
			treenewitem.Activated += window.OnNewItemActionActivated;
			menu.Add (treenewitem);

			treeadditem = new MenuItem ("Add Item...");
			treeadditem.Activated += window.OnAddItemActionActivated;
			menu.Add (treeadditem);

			MenuItem treedelete = new MenuItem ("Delete");
			treedelete.Activated += window.OnDeleteActionActivated;
			menu.Add (treedelete);

			SeparatorMenuItem seperator = new SeparatorMenuItem ();
			menu.Add (seperator);

			treeopenfile = new MenuItem ("Open File");
			menu.Add (treeopenfile);

			MenuItem treeopenfilelocation = new MenuItem ("Open File Location");
			menu.Add (treeopenfilelocation);

			SeparatorMenuItem seperator2 = new SeparatorMenuItem ();
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

		public void AddItem(TreeIter iter, string path)
		{
			Gdk.Pixbuf icon = ICON_OTHER;

			if (path.Contains ("/")) 
				icon = ICON_FOLDER;

			string[] split = path.Split ('/');
			TreeIter itr;
			if (!GetIter (iter, split [0], out itr))
				itr = listStore.AppendValues (iter, icon, split [0]);

			if (split.Length > 1) {
				string newpath = split [1];
				for(int i = 2;i < split.Length;i++)
					newpath += "/" + split[i];

				AddItem (itr, newpath);
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
					listStore.Remove (ref itr);
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

		public string GetSelectedTreePath(out TreeIter iter, out Gdk.Pixbuf icon)
		{
			string filePath = "";
			icon = ICON_BASE;

			if (treeview1.Selection.GetSelected (out iter)) {
				icon = (Gdk.Pixbuf)treeview1.Model.GetValue (iter, 0);
				filePath = GetPathFromIter (iter);
			}

			return filePath;
		}

		private List<string> GetAllPaths(TreeIter iter)
		{
			TreeIter oiter;
			List<string> paths = new List<string> ();

			if(treeview1.Model.IterChildren (out oiter, iter)) {
				do {
					if ((Gdk.Pixbuf)treeview1.Model.GetValue (oiter, 0) == ICON_OTHER)
						paths.Add (GetPathFromIter (oiter));
					else
						paths.AddRange (GetAllPaths (oiter));
				} while (treeview1.Model.IterNext (ref oiter));
			}

			return paths;
		}

		public void Remove()
		{
			TreeIter iter;
			Gdk.Pixbuf icon;
			string path = GetSelectedTreePath (out iter, out icon);

			List<ContentItem> items = new List<ContentItem>();

			if (icon == ICON_OTHER) {
				var item = window._controller.GetItem(path) as ContentItem;
				items.Add (item);
			} else if (icon == ICON_FOLDER) {
				List<string> paths = GetAllPaths (iter);
				foreach(string pth in paths)
				{
					var item = window._controller.GetItem(pth) as ContentItem;
					items.Add (item);
				}

				listStore.Remove (ref iter);
			}

			if(items.Count > 0)
				window._controller.Exclude (items);
		}

		protected void OnTreeview1ButtonReleaseEvent (object o, ButtonReleaseEventArgs args)
		{
			if (args.Event.Button != 3 || !window._controller.ProjectOpen)
				return;

			TreeViewDropPosition pos; 
			TreePath path;
			TreeIter iter;

			treeview1.GetDestRowAtPos ((int)args.Event.X, (int)args.Event.Y, out path, out pos);
			if (path != null) {
				listStore.GetIter (out iter, path);

				Gdk.Pixbuf icon = (Gdk.Pixbuf)treeview1.Model.GetValue (iter, 0);
				window.UpdateMenus ();

				menu.ShowAll ();
				if (icon == ICON_BASE) {
					treenewitem.Visible = true;
					treeadditem.Visible = true;
					treeopenfile.Visible = true;
				} else if (icon == ICON_FOLDER) {
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
		}
	}
}

