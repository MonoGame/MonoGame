using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using Gtk;
using System.Reflection;
using System.ComponentModel;
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
		internal IController controller { get; set; }

		public PropertiesView ()
		{
			this.Build ();
		}

		public void Initalize(Gtk.Window window)
		{
			propertygridtable1.Initalize (window);
		}

		protected void OnTextview1SizeAllocated (object o, SizeAllocatedArgs args)
		{
			vpaned1.Position = vpaned1.Allocation.Height; //add -50 for correct size of description box
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
				propertygridtable1.Clear ();
				return;
			}

			var objectType = currentObject.GetType ();
			var props = objectType.GetProperties (BindingFlags.Instance | BindingFlags.Public);

			propertygridtable1.Clear ();
			uint currentLine = 0;
			foreach (var p in props) {

				var attrs = p.GetCustomAttributes(true).Where(x => x is BrowsableAttribute).Cast<BrowsableAttribute>();
				if (attrs.Any (x => !x.Browsable))
					continue;

				if (p.PropertyType == typeof(BuildAction)) {
					Dictionary<string, object> data = Enum.GetValues (typeof(BuildAction))
						.Cast<BuildAction> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					propertygridtable1.AddEntry (currentLine++, p.Name, p.GetValue(currentObject, null), 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							p.SetValue(currentObject, data[((string)((FalseWidget)s).newvalue)], null);
							controller.OnProjectModified();
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(List<string>)) {
					propertygridtable1.AddEntry (currentLine++, p.Name, p.GetValue (currentObject, null),
						PropertyGridTable.EntryType.List ,(s,e) => { 
							List<string> lines = new List<string>();

							lines.AddRange(((string)((FalseWidget)s).newvalue).Replace("\r\n", "~").Split('~'));

							p.SetValue(currentObject, lines, null);
							controller.OnProjectModified();
						});

					continue;
				}
				if (p.PropertyType == typeof(GraphicsProfile)) {
					Dictionary<string, object> data = Enum.GetValues (typeof(GraphicsProfile))
						.Cast<GraphicsProfile> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					propertygridtable1.AddEntry (currentLine++, p.Name, p.GetValue(currentObject, null), 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							p.SetValue(currentObject, data[((string)((FalseWidget)s).newvalue)], null);
							controller.OnProjectModified();
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(TP.TargetPlatform)) {
					Dictionary<string, object> data = Enum.GetValues (typeof(TP.TargetPlatform))
						.Cast<TP.TargetPlatform> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					propertygridtable1.AddEntry (currentLine++, p.Name, p.GetValue(currentObject,null), 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							p.SetValue(currentObject, data[((string)((FalseWidget)s).newvalue)],null);
							controller.OnProjectModified();
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(string)) {
					if (p.CanWrite)
						propertygridtable1.AddEntry (currentLine++, p.Name, p.GetValue(currentObject, null), 
							PropertyGridTable.EntryType.Text, (s,e) => { 
								p.SetValue(currentObject, ((string)((FalseWidget)s).newvalue), null);
								controller.OnProjectModified();
							});
					else 
						propertygridtable1.AddEntry (currentLine++, p.Name, p.GetValue(currentObject, null), 
							PropertyGridTable.EntryType.Readonly);
					continue;
				}
				if (p.PropertyType == typeof(bool)) {
					propertygridtable1.AddEntry (currentLine++, p.Name, p.GetValue(currentObject, null), 
						PropertyGridTable.EntryType.Check,(s,e) => { 
							p.SetValue(currentObject, Convert.ToBoolean(((string)((FalseWidget)s).newvalue)), null);
							controller.OnProjectModified();
						});
					continue;
				}
				if (p.PropertyType == typeof(ImporterTypeDescription)) {

					var importer = (ImporterTypeDescription)p.GetValue(currentObject, null);
					var data = PipelineTypes.Importers.ToDictionary (item => item.DisplayName, item => (object)item);
					propertygridtable1.AddEntry (currentLine++, p.Name, importer, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach(ImporterTypeDescription itd in PipelineTypes.Importers)
							{
								if(itd.DisplayName == (string)((FalseWidget)s).newvalue)
								{
									p.SetValue(currentObject, itd, null);
									controller.OnProjectModified();
									Refresh ();
									controller.OnProjectModified();
									break;
								}
							}
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(ProcessorTypeDescription)) {
					var processor = (ProcessorTypeDescription)p.GetValue(currentObject, null);
					var contentItem = (ContentItem)currentObject;
					var data = PipelineTypes.Processors.ToDictionary (item => item.DisplayName, item => (object)item);
					propertygridtable1.AddEntry (currentLine++, p.Name, processor, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach(ProcessorTypeDescription ptd in PipelineTypes.Processors)
							{
								if(ptd.DisplayName == (string)((FalseWidget)s).newvalue)
								{
									processor = ptd;
									p.SetValue(currentObject, processor, null);
									controller.OnProjectModified();
									Refresh ();
									controller.OnProjectModified();
									break;
								}
							}
						}, data);

					RefreshProcessorParams (processor, contentItem);
					continue;
				}

				propertygridtable1.AddEntry (currentLine++, p.Name, null, PropertyGridTable.EntryType.Unkown);

			}

			propertygridtable1.Refresh ();
		}

		void RefreshProcessorParams(ProcessorTypeDescription processor, ContentItem contentItem) {
			uint currentLine = 0;
			foreach (var p1 in processor.Properties) {
				if (p1.Type == typeof(bool)) {
					propertygridtable1.AddProcEntry (currentLine++, p1.Name, contentItem.ProcessorParams[p1.Name], 
						PropertyGridTable.EntryType.Check, (s,e) => { 
							contentItem.ProcessorParams[p1.Name] = Convert.ToBoolean((string)((FalseWidget)s).newvalue);
							controller.OnItemModified (contentItem);
						});
					continue;
				}
				if (p1.Type == typeof(string)) {
					propertygridtable1.AddProcEntry (currentLine++, p1.Name, contentItem.ProcessorParams[p1.Name], 
						PropertyGridTable.EntryType.Text, (s,e) => { 
							contentItem.ProcessorParams[p1.Name] = (string)((FalseWidget)s).newvalue;
							controller.OnItemModified (contentItem);
						});
					continue;
				}
				if (p1.Type == typeof(char)) {
					char c = ' ';
					var v = contentItem.ProcessorParams [p1.Name];
					char.TryParse (v.ToString(), out c);

					propertygridtable1.AddProcEntry (currentLine++, p1.Name, c, 
						PropertyGridTable.EntryType.Text, (s,e) => { 
							if (!string.IsNullOrEmpty((string)((FalseWidget)s).newvalue))
								contentItem.ProcessorParams[p1.Name] = ((string)((FalseWidget)s).newvalue)[0];
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
					propertygridtable1.AddProcEntry (currentLine++, p1.Name, (object)value ?? (object)defaultValue, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							contentItem.ProcessorParams[p1.Name] = (ConversionQuality)data[(string)((FalseWidget)s).newvalue];
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
					propertygridtable1.AddProcEntry (currentLine++, p1.Name, (object)value ?? (object)defaultValue,
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							contentItem.ProcessorParams[p1.Name] = (MaterialProcessorDefaultEffect)data[(string)((FalseWidget)s).newvalue];
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
					propertygridtable1.AddProcEntry (currentLine++, p1.Name, (object)value ?? (object)defaultValue,
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							contentItem.ProcessorParams[p1.Name] = (TextureProcessorOutputFormat)data[(string)((FalseWidget)s).newvalue];
							controller.OnItemModified (contentItem);
						}, data);
					continue;
				}
				if (p1.Type == typeof(Microsoft.Xna.Framework.Color)) {
					propertygridtable1.AddProcEntry (currentLine++, p1.Name, contentItem.ProcessorParams[p1.Name], 
						PropertyGridTable.EntryType.Color, (s,e) => { 

							try {
								string[] cvalues = ((FalseWidget)s).newvalue.ToString().Replace (":", " ").Replace("}", " ").Split (' ');
								Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color();

								color.R = (byte)Convert.ToInt16 (cvalues [1]);
								color.G = (byte)Convert.ToInt16 (cvalues [3]);
								color.B = (byte)Convert.ToInt16 (cvalues [5]);
								color.A = (byte)Convert.ToInt16 (cvalues [7]);

								contentItem.ProcessorParams[p1.Name] = color;
								controller.OnItemModified (contentItem);
							}
							catch { }
						});
					continue;
				}
				if (p1.Type == typeof(Single)) {
					propertygridtable1.AddProcEntry (currentLine++, p1.Name, contentItem.ProcessorParams[p1.Name].ToString(), 
						PropertyGridTable.EntryType.Text, (s,e) => { 
							contentItem.ProcessorParams[p1.Name] =  Single.Parse ((string)((FalseWidget)s).newvalue).ToString();
							controller.OnItemModified (contentItem);
						});
					continue;
				}
				propertygridtable1.AddProcEntry (currentLine++, p1.Name, null, PropertyGridTable.EntryType.Unkown);
			}
		}

		protected void OnButton15Clicked (object sender, EventArgs e)
		{
			propertygridtable1.sortgroup = true;
			propertygridtable1.Refresh ();
		}

		protected void OnButton2Clicked (object sender, EventArgs e)
		{
			propertygridtable1.sortgroup = false;
			propertygridtable1.Refresh ();
		}
	}
}

