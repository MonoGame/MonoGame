using System.Collections.Generic;
using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
	public partial class NewTemplateDialog : Gtk.Dialog
	{
		public string name;
		public ContentItemTemplate templateFile;

		List<ContentItemTemplate> items;
		TreeStore listStore;

		public NewTemplateDialog (IEnumerator<ContentItemTemplate> enums)
		{
			this.Build ();

			this.Title = "New Item";
			Gtk.TreeViewColumn column = new Gtk.TreeViewColumn ();

			Gtk.CellRendererPixbuf iconCell = new Gtk.CellRendererPixbuf ();
			Gtk.CellRendererText textCell = new Gtk.CellRendererText ();
			Gtk.CellRendererText textCell2 = new Gtk.CellRendererText ();

			column.PackStart (iconCell, false);
			column.PackStart (textCell, false);
			column.PackStart (textCell2, false);

			treeview1.AppendColumn (column);

			column.AddAttribute (iconCell,  "pixbuf", 0);
			column.AddAttribute (textCell, "text", 1);
			column.AddAttribute (textCell, "text", 2);

			listStore = new Gtk.TreeStore (typeof (Gdk.Pixbuf), typeof (string), typeof (string));
			treeview1.Model = listStore;

			items = new List<ContentItemTemplate> ();
			int i = 0;

			while (enums.MoveNext ()) {
				listStore.AppendValues (new Gdk.Pixbuf (System.IO.Path.GetDirectoryName (enums.Current.TemplateFile) + "/" + enums.Current.Icon), enums.Current.Label, i.ToString());
				items.Add (enums.Current);
				i++;
			}
		}

		protected void OnResponse(object sender, EventArgs e)
		{
			this.name = this.entry1.Text;

			TreeIter iter;
			if (treeview1.Selection.GetSelected (out iter)) {
				int tid = Convert.ToInt32 (treeview1.Model.GetValue (iter, 2).ToString ());
				templateFile = items [tid];
			}

			this.Destroy ();
		}

		public void ButtonOkEnabled()
		{
			TreeIter iter;

			if (entry1.Text != "") {
				if (MainWindow.CheckString (entry1.Text, MainWindow.AllowedCharacters)) {
					if (treeview1.Selection.GetSelected (out iter)) {
						buttonOk.Sensitive = true;
						label2.Visible = false;
					} else {
						buttonOk.Sensitive = false;
						label2.Visible = false;
					}
				} else {
					buttonOk.Sensitive = false;
					label2.Visible = true;
				}
			} else {
				buttonOk.Sensitive = false;
				label2.Visible = false;
			}
		}

		protected void OnTreeview1CursorChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			if(treeview1.Selection.GetSelected(out iter))
				ButtonOkEnabled ();
		}

		protected void OnEntry1Changed (object sender, EventArgs e)
		{
			ButtonOkEnabled ();
		}

		protected void OnButtonOkClicked (object sender, EventArgs e)
		{
			if (buttonOk.Sensitive) 
				this.Respond (Gtk.ResponseType.Ok);
		}
	}
}

