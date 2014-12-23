using System;
using System.Collections.Generic;
using Gtk;

namespace MonoGame.Tools.Pipeline
{
	[System.ComponentModel.ToolboxItem (true)]
	public class PropertyGridTable : Gtk.Table {

		internal PropertyGridTable () : base(0,2,false)
		{
		}

		public enum EntryType {
			Text,
			Readonly,
			LongText,
			Color,
			Combo,
			Check,
			Integer,
			List,
			Unkown
		}

		public void Clear() {
			foreach (var widget in Children) {
				Remove (widget);
			}
		}

		public void AddEntry(uint line, string label, object value, EntryType type, EventHandler eventHandler = null, Dictionary<string, object> comboItems = null) {

			if (line > NRows-1)
				Resize (NRows+1, NColumns);

			var lbl = new Label (label) { WidthRequest = 130 };
			lbl.SetAlignment (0f, 0.5f);
			Attach(lbl, 0, 1,line,line+1, xoptions:AttachOptions.Fill, yoptions:AttachOptions.Shrink,xpadding:5,ypadding:5);
			lbl.Show ();
			Widget entry;
			ScrolledWindow textContainer;
			TextView tv;
			switch (type) {
			case EntryType.Text:
				tv = new TextView () { WidthRequest = 210 };
				if (value is string)
					tv.Buffer.Text = (string)value ?? "";
				if (value is char)
					tv.Buffer.Text = value.ToString() ?? "";
				tv.Justification = Justification.Left;
				tv.WrapMode = WrapMode.None;
				textContainer = new ScrolledWindow () { WidthRequest = 220, HeightRequest = 20 };
				textContainer.HscrollbarPolicy = PolicyType.Automatic;
				textContainer.VscrollbarPolicy = PolicyType.Never;
				textContainer.AddWithViewport (tv);
				if (eventHandler != null)
					tv.Buffer.Changed += eventHandler;
				entry = textContainer;
				break;
			case EntryType.Readonly:
				tv = new TextView () {
					Sensitive = false,
					WidthRequest = 210
				};
				tv.Buffer.Text = (string)value ?? "";
				tv.WrapMode = WrapMode.Word;
				tv.Justification = Justification.Left;
				textContainer = new ScrolledWindow () { WidthRequest = 220, HeightRequest = 20 };
				textContainer.HscrollbarPolicy = PolicyType.Automatic;
				textContainer.VscrollbarPolicy = PolicyType.Never;
				textContainer.AddWithViewport (tv);
				entry = textContainer;
				break;
			case EntryType.LongText:
				tv = new TextView () { WidthRequest = 210 };
				tv.Buffer.Text = (string)value ?? "";
				tv.WrapMode = WrapMode.Word;
				tv.Justification = Justification.Left;
				if (eventHandler != null)
					tv.Buffer.Changed += eventHandler;
				textContainer = new ScrolledWindow () { WidthRequest = 220, HeightRequest = 20 };
				textContainer.HscrollbarPolicy = PolicyType.Automatic;
				textContainer.VscrollbarPolicy = PolicyType.Never;
				textContainer.AddWithViewport (tv);
				entry = textContainer;
				break;
			case EntryType.Color:
				var hbox = new HBox ();
				var cb = new ColorButton ();
				var c = (Microsoft.Xna.Framework.Color)value;
				cb.Color = new Gdk.Color (c.R, c.G, c.B);
				if (eventHandler != null)
					cb.ColorSet += eventHandler;

				hbox.PackStart (cb, true, true, 0);

				var slider = new ScaleButton (IconSize.Button, 0, 255, 1, null);
				slider.Value = c.A;
				slider.ValueChanged += (object o, ValueChangedArgs args) => {
					eventHandler(slider, EventArgs.Empty);
				};
				hbox.PackStart (slider, true, true, 0);
				slider.Show ();
				cb.Show ();
				entry = hbox;
				break;
			case EntryType.Combo:

				ListStore model = null;
				TreeIter item = TreeIter.Zero;
				foreach (var v in comboItems) {
					if (model == null)
						model = new ListStore (v.Key.GetType (), v.Value.GetType ());
					var i = model.AppendValues (v.Key, v.Value);
					if (v.Value.Equals (value))
						item = i;
				}
				var cbe = new ComboBox (model) { WidthRequest = 220 };

				CellRendererText text = new CellRendererText (); 
				cbe.PackStart (text, true); 
				cbe.AddAttribute (text, "text", 0); 
				cbe.SetActiveIter (item);
				if (eventHandler != null)
					cbe.Changed += eventHandler;
				entry = cbe;
				break;
			case EntryType.Check:
				var chk = new CheckButton () {
					Active = (bool)value,
				};
				if (eventHandler != null)
					chk.Toggled += eventHandler;
				entry = chk;
				break;
			case EntryType.List:
				var list = new Gtk.FileChooserButton ("", FileChooserAction.Open) { WidthRequest = 220 };
				entry = list;
				break;
			default:
				// Shouldn't get here
				tv = new TextView ();
				tv.Buffer.Text = "Unknown";
				entry = tv;
				break;
			}

			Attach (entry, 1, 2, line, line + 1, xoptions:AttachOptions.Expand, yoptions:AttachOptions.Expand,xpadding:0,ypadding:0);
			entry.ShowAll ();
		}

	}
}

