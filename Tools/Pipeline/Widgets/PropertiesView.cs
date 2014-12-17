using System;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PropertiesView : Gtk.Bin
	{
		TreeStore listStore;

		public PropertiesView ()
		{
			this.Build ();

			Gtk.TreeViewColumn column1 = new Gtk.TreeViewColumn ();
			column1.Sizing = TreeViewColumnSizing.Fixed;
			column1.FixedWidth = 100;
			column1.Resizable = true;
			column1.Title = "Name";

			Gtk.CellRendererText textCell1 = new Gtk.CellRendererText ();
			textCell1.Underline = Pango.Underline.Single;
			column1.PackStart (textCell1, false);

			Gtk.CellRendererText textCell2 = new Gtk.CellRendererText ();
			column1.PackStart (textCell2, false);

			treeview2.AppendColumn (column1);

			Gtk.TreeViewColumn column2 = new Gtk.TreeViewColumn ();
			column2.Resizable = true;
			column2.Title = "Value";

			Gtk.CellRendererText editTextCell = new Gtk.CellRendererText ();

			editTextCell.EditingStarted += delegate(object o, EditingStartedArgs args) {

			};

			editTextCell.Edited += delegate(object o, EditedArgs args) {
				TreeModel model;
				TreeIter iter;

				if (treeview2.Selection.GetSelected (out model, out iter)) {
					model.SetValue(iter, 4, args.NewText);
				}
			};
			column2.PackStart (editTextCell, false);

			Gtk.CellRendererCombo comboCell = new Gtk.CellRendererCombo ();
			comboCell.TextColumn = 0;
			comboCell.IsExpanded = true;
			comboCell.Editable = true;
			comboCell.HasEntry = false;
			comboCell.Edited += delegate(object o, EditedArgs args) {
				TreeModel model;
				TreeIter iter;

				if (treeview2.Selection.GetSelected (out model, out iter)) {
					model.SetValue(iter, 8, args.NewText);
				}
			};
			column2.PackStart (comboCell , false);

			treeview2.AppendColumn (column2);

			column1.AddAttribute (textCell1, "text", 0);
			column1.AddAttribute (textCell1, "visible", 1);
			column1.AddAttribute (textCell2, "text", 2);
			column1.AddAttribute (textCell2, "visible", 3);
			column2.AddAttribute (editTextCell, "text", 4);
			column2.AddAttribute (editTextCell, "visible", 5);
			column2.AddAttribute (editTextCell, "editable", 6);
			column2.AddAttribute (comboCell, "model", 7);
			column2.AddAttribute (comboCell, "text", 8);
			column2.AddAttribute (comboCell, "editable", 9);
			column2.AddAttribute (comboCell, "visible", 10);

			listStore = new Gtk.TreeStore (typeof (string), typeof(bool), typeof (string), typeof(bool), typeof (string), typeof(bool), typeof(bool), typeof(TreeStore), typeof(string), typeof(bool), typeof(bool));
			treeview2.Model = listStore;
		}

		protected void OnGtkScrolledWindow1SizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			vpaned1.Position = vpaned1.Allocation.Height - 50;
		}
	}
}

