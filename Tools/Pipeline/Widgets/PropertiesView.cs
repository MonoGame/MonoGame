using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using Gtk;
using System.Reflection;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using TP = Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Audio;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace MonoGame.Tools.Pipeline
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PropertiesView : Gtk.Bin
	{
		object currentObject;
		PropertyGridTable properties;
		PropertyGridTable processorParams;
		internal IController controller { get; set; }

		public PropertiesView ()
		{
			this.Build ();

			var vbox = new VBox ();
			properties = new PropertyGridTable ();
			processorParams = new PropertyGridTable ();

			vbox.PackStart (properties, false, false,0);
			vbox.PackStart (processorParams, false, false, 0);

			scrolledwindow2.AddWithViewport (vbox);
			//PackStart (GtkScrolledWindow, true, true, 0);
		}

		protected void OnVbox1SizeAllocated (object o, SizeAllocatedArgs args)
		{
			vpaned1.Position = vpaned1.Allocation.Height - 50;
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
			scrolledwindow2.QueueDraw ();
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
}

