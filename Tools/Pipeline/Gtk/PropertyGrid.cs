using Gtk;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using MonoGame.Tools.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using TP = Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;

namespace MonoGame.Tools.Pipeline
{
	public class PropertyGrid : Gtk.VBox
	{
		object currentObject;
		ScrolledWindow container;
		PropertyGridTable properties;
		PropertyGridTable processorParams;
		IController controller;

		internal PropertyGrid (IController controler)
		{
			this.controller = controler;
			container = new ScrolledWindow ();
			container.HscrollbarPolicy = PolicyType.Never;
			container.VscrollbarPolicy = PolicyType.Automatic;

			var vbox = new VBox ();
			properties = new PropertyGridTable (this);
			processorParams = new PropertyGridTable (this);

			vbox.PackStart (properties, false, false,0);
			vbox.PackStart (processorParams, false, false, 0);

			container.AddWithViewport (vbox);
			PackStart (container, true, true, 0);
		}

		public object CurrentObject { 
			get { return currentObject; }
			set { 
				if (currentObject != value) {
					currentObject = value;
					Refresh ();
				}
			}
		}


		public void Refresh() {
			if (currentObject == null) {
				properties.Clear ();
				processorParams.Clear ();
				return;
			}

			var objectType = currentObject.GetType ();
			var props = objectType.GetProperties (BindingFlags.Instance | BindingFlags.Public);

			properties.Clear ();
			processorParams.Clear ();
			uint currentLine = 0;
			foreach (var p in props) {

				var attrs = p.GetCustomAttributes(true).Where(x => x is BrowsableAttribute).Cast<BrowsableAttribute>();
				if (attrs.Any (x => !x.Browsable))
					continue;

				if (p.PropertyType == typeof(BuildAction)) {
					continue;
				}
				if (p.PropertyType == typeof(List<string>)) {
					processorParams.AddEntry (currentLine++, p.Name, p.GetValue (currentObject, null),
						PropertyGridTable.EntryType.List);
					continue;
				}
				if (p.PropertyType == typeof(GraphicsProfile)) {
					Dictionary<string, object> data = Enum.GetValues (typeof(GraphicsProfile))
						.Cast<GraphicsProfile> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					processorParams.AddEntry (currentLine++, p.Name, p.GetValue(currentObject,null), 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							var combo = (ComboBox)s;
							p.SetValue(currentObject, data[combo.ActiveText],null);
							controller.OnProjectModified();
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(TP.TargetPlatform)) {
					Dictionary<string, object> data = Enum.GetValues (typeof(TP.TargetPlatform))
						.Cast<TP.TargetPlatform> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					processorParams.AddEntry (currentLine++, p.Name, p.GetValue(currentObject,null), 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							var combo = (ComboBox)s;
							p.SetValue(currentObject, data[combo.ActiveText],null);
							controller.OnProjectModified();
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(string)) {
					if (p.CanWrite)
						properties.AddEntry (currentLine++, p.Name, p.GetValue(currentObject, null), 
							PropertyGridTable.EntryType.Text, (s,e) => { 
								p.SetValue(currentObject, ((TextBuffer)s).Text,null);
							});
					else 
						properties.AddEntry (currentLine++, p.Name, p.GetValue(currentObject,null), 
							PropertyGridTable.EntryType.Readonly);
					continue;
				}
				if (p.PropertyType == typeof(bool)) {
					properties.AddEntry (currentLine++, p.Name, p.GetValue(currentObject, null), 
						PropertyGridTable.EntryType.Check,(s,e) => { 
							p.SetValue(currentObject, ((CheckButton)s).Active,null);
						});
					continue;
				}
				if (p.PropertyType == typeof(ImporterTypeDescription)) {

					var importer = (ImporterTypeDescription)p.GetValue(currentObject, null);
					var data = PipelineTypes.Importers.ToDictionary (item => item.DisplayName, item => (object)item);
					properties.AddEntry (currentLine++, p.Name, importer, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							var combo = (ComboBox)s;
							p.SetValue(currentObject, PipelineTypes.Importers[combo.Active],null);
							controller.OnProjectModified();
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(ProcessorTypeDescription)) {
					var processor = (ProcessorTypeDescription)p.GetValue(currentObject, null);
					var contentItem = (ContentItem)currentObject;
					var data = PipelineTypes.Processors.ToDictionary (item => item.DisplayName, item => (object)item);
					properties.AddEntry (currentLine++, p.Name, processor, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							var combo = (ComboBox)s;
							processor = PipelineTypes.Processors[combo.Active];
							p.SetValue(currentObject, processor,null);
							RefreshProcessorParams (processor, contentItem);
							controller.OnProjectModified();
						}, data);

					RefreshProcessorParams (processor, contentItem);
					continue;
				}

				properties.AddEntry (currentLine++, p.Name, null, PropertyGridTable.EntryType.Unkown);

			}

			properties.QueueDraw ();
			processorParams.QueueDraw ();
			container.QueueDraw ();
		}

		void RefreshProcessorParams(ProcessorTypeDescription processor, ContentItem contentItem) {
			processorParams.Clear ();
			uint currentLine = 0;
			foreach (var p1 in processor.Properties) {
				if (p1.Type == typeof(bool)) {
					processorParams.AddEntry (currentLine++, p1.Name, contentItem.ProcessorParams[p1.Name], 
						PropertyGridTable.EntryType.Check, (s,e) => { 
							contentItem.ProcessorParams[p1.Name] = ((CheckButton)s).Active;
							controller.OnItemModified (contentItem);
						});
					continue;
				}
				if (p1.Type == typeof(string)) {
					processorParams.AddEntry (currentLine++, p1.Name, contentItem.ProcessorParams[p1.Name], 
						PropertyGridTable.EntryType.Text, (s,e) => { 
							contentItem.ProcessorParams[p1.Name] = ((TextBuffer)s).Text;
							controller.OnItemModified (contentItem);
						});
					continue;
				}
				if (p1.Type == typeof(char)) {
					char c = ' ';
					var v = contentItem.ProcessorParams [p1.Name];
					char.TryParse (v.ToString(), out c);
					processorParams.AddEntry (currentLine++, p1.Name, c, 
						PropertyGridTable.EntryType.Text, (s,e) => { 
							var tb = (TextBuffer)s;
							if (!string.IsNullOrEmpty(tb.Text))
								contentItem.ProcessorParams[p1.Name] = tb.Text[0];
							else 
								contentItem.ProcessorParams[p1.Name] = ' '.ToString();
							controller.OnItemModified (contentItem);
						});
					continue;
				}
				if (p1.Type == typeof(ConversionQuality)) {
					var value = contentItem.ProcessorParams [p1.Name];
					Dictionary<string, object> data = Enum.GetValues (typeof(ConversionQuality))
						.Cast<ConversionQuality> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					var defaultValue = (ConversionQuality)p1.DefaultValue;
					processorParams.AddEntry (currentLine++, p1.Name, (object)value ?? (object)defaultValue, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							var combo = (ComboBox)s;;
							contentItem.ProcessorParams[p1.Name] = (ConversionQuality)data[combo.ActiveText];
							controller.OnItemModified (contentItem);
						}, data);
					continue;
				}
				if (p1.Type == typeof(MaterialProcessorDefaultEffect)) {
					var value = contentItem.ProcessorParams [p1.Name];
					Dictionary<string, object> data = Enum.GetValues (typeof(MaterialProcessorDefaultEffect))
						.Cast<MaterialProcessorDefaultEffect> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					var defaultValue = (MaterialProcessorDefaultEffect)p1.DefaultValue;
					processorParams.AddEntry (currentLine++, p1.Name, (object)value ?? (object)defaultValue,
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							var combo = (ComboBox)s;;
							contentItem.ProcessorParams[p1.Name] = (MaterialProcessorDefaultEffect)data[combo.ActiveText];
							controller.OnItemModified (contentItem);
						}, data);
					continue;
				}
				if (p1.Type == typeof(TextureProcessorOutputFormat)) {
					var value = contentItem.ProcessorParams [p1.Name];
					Dictionary<string, object> data = Enum.GetValues (typeof(TextureProcessorOutputFormat))
						.Cast<TextureProcessorOutputFormat> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					var defaultValue = (TextureProcessorOutputFormat)p1.DefaultValue;
					processorParams.AddEntry (currentLine++, p1.Name, (object)value ?? (object)defaultValue,
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							var combo = (ComboBox)s;;
							contentItem.ProcessorParams[p1.Name] = (TextureProcessorOutputFormat)data[combo.ActiveText];
							controller.OnItemModified (contentItem);
						}, data);
					continue;
				}
				if (p1.Type == typeof(Microsoft.Xna.Framework.Color)) {
					processorParams.AddEntry (currentLine++, p1.Name, contentItem.ProcessorParams[p1.Name], 
						PropertyGridTable.EntryType.Color, (s,e) => { 
							var button = s as ColorButton;
							var color = (Microsoft.Xna.Framework.Color)contentItem.ProcessorParams[p1.Name];
							if (button != null) {
								color.R = (byte)(button.Color.Red >> 8);
								color.G = (byte)(button.Color.Green >> 8);
								color.B = (byte)(button.Color.Blue >> 8);
							} 
							var scaleButton = s as ScaleButton;
							if (scaleButton != null) {
								color.A = (byte)scaleButton.Value;
							}
							contentItem.ProcessorParams[p1.Name] = color;
							controller.OnItemModified (contentItem);
						});
					continue;
				}
				if (p1.Type == typeof(Single)) {
					processorParams.AddEntry (currentLine++, p1.Name, contentItem.ProcessorParams[p1.Name].ToString(), 
						PropertyGridTable.EntryType.Text, (s,e) => { 
							contentItem.ProcessorParams[p1.Name] =  Single.Parse (((TextBuffer)s).Text).ToString();
							controller.OnItemModified (contentItem);
						});
					continue;
				}
				processorParams.AddEntry (currentLine++, p1.Name, null, PropertyGridTable.EntryType.Unkown);
			}
		}
	}

	class PropertyGridTable : Gtk.Table {

		PropertyGrid parent;

		internal PropertyGridTable (PropertyGrid propertyGrid) : base(0,2,false)
		{
			parent = propertyGrid;
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