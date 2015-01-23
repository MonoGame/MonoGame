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
		List<object> currentObjects;
		internal IController controller { get; set; }

		string name;
		string location;

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

		public void Load(List<object> cobjects, string name, string location)
		{
			this.name = name;
			this.location = location;
			if(cobjects != null)
				this.currentObjects = cobjects;
			else
				this.currentObjects = null;

            if (this.name == "????" && this.location == "????")
                propertygridtable1.Clear();
            else
			    Refresh ();
		}

		private object CompareVariables(object a, object b)
		{
			if (a.ToString () == "???" || a.Equals(b))
				return b;

			return null;
		}

		public void Refresh() {
			propertygridtable1.Clear ();
			propertygridtable1.AddEntry ("Name", name, PropertyGridTable.EntryType.Readonly);
			propertygridtable1.AddEntry ("Location", location, PropertyGridTable.EntryType.Readonly);

			if (currentObjects == null) {
				propertygridtable1.Refresh ();
				return;
			}

			var objectType = currentObjects[0].GetType ();
			var props = objectType.GetProperties (BindingFlags.Instance | BindingFlags.Public);

			foreach (var p in props) {

				var attrs = p.GetCustomAttributes(true).Where(x => x is BrowsableAttribute).Cast<BrowsableAttribute>();
				if (attrs.Any (x => !x.Browsable) || p.Name == "Name" || p.Name == "Location")
					continue;

				object value = "???";
				foreach (object o in currentObjects) 
					value = CompareVariables (value, p.GetValue (o, null));

				if (p.PropertyType == typeof(BuildAction)) {
					if (value == null)
						value = "";

					Dictionary<string, object> data = Enum.GetValues (typeof(BuildAction))
						.Cast<BuildAction> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					propertygridtable1.AddEntry (p.Name, value, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach (object o in currentObjects) 
								p.SetValue(o, data[((string)((FalseWidget)s).newvalue)], null);
							controller.OnProjectModified();
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(List<string>)) {
					if (value == null)
						value = new List<string> ();

					propertygridtable1.AddEntry (p.Name, value,
						PropertyGridTable.EntryType.List ,(s,e) => { 
							List<string> lines = new List<string>();

							lines.AddRange(((string)((FalseWidget)s).newvalue).Replace("\r\n", "~").Split('~'));

							foreach (object o in currentObjects) 
								p.SetValue(o, lines, null);
							controller.OnProjectModified();
						});

					continue;
				}
				if (p.PropertyType == typeof(GraphicsProfile)) {
					if (value == null)
						value = "";

					Dictionary<string, object> data = Enum.GetValues (typeof(GraphicsProfile))
						.Cast<GraphicsProfile> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					propertygridtable1.AddEntry (p.Name, value, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach (object o in currentObjects) 
								p.SetValue(o, data[((string)((FalseWidget)s).newvalue)], null);
							controller.OnProjectModified();
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(TP.TargetPlatform)) {
					if (value == null)
						value = "";

					Dictionary<string, object> data = Enum.GetValues (typeof(TP.TargetPlatform))
						.Cast<TP.TargetPlatform> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					propertygridtable1.AddEntry (p.Name, value, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach (object o in currentObjects) 
								p.SetValue(o, data[((string)((FalseWidget)s).newvalue)],null);
							controller.OnProjectModified();
						}, data);
					continue;
				}
				if (p.PropertyType == typeof(string)) {
					if (value == null)
						value = "";

					if (p.CanWrite)
						propertygridtable1.AddEntry (p.Name, value, 
							PropertyGridTable.EntryType.Text, (s,e) => { 
								foreach (object o in currentObjects) 
									p.SetValue(o, ((string)((FalseWidget)s).newvalue), null);
								controller.OnProjectModified();
							});
					else 
						propertygridtable1.AddEntry (p.Name, value, 
							PropertyGridTable.EntryType.Readonly);
					continue;
				}
				if (p.PropertyType == typeof(bool)) {
					if (value == null)
						value = "";

					propertygridtable1.AddEntry (p.Name, value, 
						PropertyGridTable.EntryType.Check,(s,e) => { 
							foreach (object o in currentObjects) 
								p.SetValue(o, Convert.ToBoolean(((string)((FalseWidget)s).newvalue)), null);
							controller.OnProjectModified();
						});
					continue;
				}
				if (p.PropertyType == typeof(ImporterTypeDescription)) {

					if (value == null)
						value = "";

					var data = PipelineTypes.Importers.ToDictionary (item => item.DisplayName, item => (object)item);
					propertygridtable1.AddEntry (p.Name, value, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach(ImporterTypeDescription itd in PipelineTypes.Importers)
							{
								if(itd.DisplayName == (string)((FalseWidget)s).newvalue)
								{
									foreach (object o in currentObjects) 
										p.SetValue(o, itd, null);
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
					if (value == null)
						value = "";

					var contentItem = (ContentItem)currentObjects[0];
					var data = PipelineTypes.Processors.ToDictionary (item => item.DisplayName, item => (object)item);
					propertygridtable1.AddEntry (p.Name, value, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach(ProcessorTypeDescription ptd in PipelineTypes.Processors)
							{
								if(ptd.DisplayName == (string)((FalseWidget)s).newvalue)
								{
									foreach (object o in currentObjects) 
										p.SetValue(o, ptd, null);
									controller.OnProjectModified();
									Refresh ();
									controller.OnProjectModified();
									break;
								}
							}
						}, data);

					if (value.ToString () != "") {

						List<ProcessorTypeDescription> procs = new List<ProcessorTypeDescription> ();

						foreach (object o in currentObjects)
							procs.Add ((ProcessorTypeDescription)p.GetValue (o, null));

						RefreshProcessorParams (procs, currentObjects);
					}
					continue;
				}

				propertygridtable1.AddEntry (p.Name, null, PropertyGridTable.EntryType.Unkown);

			}

			propertygridtable1.Refresh ();
		}

		void RefreshProcessorParams(List<ProcessorTypeDescription> processors, List<object> contentItems) {

			if (processors.Count == 0)
				return;

			foreach (var p1 in processors[0].Properties) {

				object value = "???";
				foreach (object o in contentItems) 
					value = CompareVariables (value, ((ContentItem)o).ProcessorParams[p1.Name]);

				if (p1.Type == typeof(bool)) {
					if (value == null)
						value = "";

					propertygridtable1.AddProcEntry (p1.Name, value, 
						PropertyGridTable.EntryType.Check, (s,e) => { 
							foreach (object o in contentItems) 
							{
								((ContentItem)o).ProcessorParams[p1.Name] = Convert.ToBoolean((string)((FalseWidget)s).newvalue);
								controller.OnItemModified ((ContentItem)o);
							}
						});
					continue;
				}
				if (p1.Type == typeof(string)) {
					if (value == null)
						value = "";

					propertygridtable1.AddProcEntry (p1.Name, value, 
						PropertyGridTable.EntryType.Text, (s,e) => { 
							foreach (object o in contentItems) 
							{
								((ContentItem)o).ProcessorParams[p1.Name] = (string)((FalseWidget)s).newvalue;
								controller.OnItemModified ((ContentItem)o);
							}
						});
					continue;
				}
				if (p1.Type == typeof(char)) {
					if (value == null)
						value = "";

					char c = ' ';
					var v = value;
					char.TryParse (v.ToString(), out c);

					propertygridtable1.AddProcEntry (p1.Name, c, 
						PropertyGridTable.EntryType.Text, (s,e) => { 
							foreach (object o in contentItems) 
							{
								if (!string.IsNullOrEmpty((string)((FalseWidget)s).newvalue))
									((ContentItem)o).ProcessorParams[p1.Name] = ((string)((FalseWidget)s).newvalue)[0];
								else 
									((ContentItem)o).ProcessorParams[p1.Name] = ' '.ToString();

								controller.OnItemModified ((ContentItem)o);
							}
						});
					continue;
				}
				if (p1.Type == typeof(ConversionQuality)) {
					if (value == null)
						value = "";
					Dictionary<string, object> data = Enum.GetValues (typeof(ConversionQuality))
						.Cast<ConversionQuality> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					var defaultValue = (ConversionQuality)p1.DefaultValue;
					propertygridtable1.AddProcEntry (p1.Name, (object)value ?? (object)defaultValue, 
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach (object o in contentItems) 
							{
								((ContentItem)o).ProcessorParams[p1.Name] = (ConversionQuality)data[(string)((FalseWidget)s).newvalue];
								controller.OnItemModified ((ContentItem)o);
							}
						}, data);
					continue;
				}
				if (p1.Type == typeof(MaterialProcessorDefaultEffect)) {
					if (value == null)
						value = "";
					Dictionary<string, object> data = Enum.GetValues (typeof(MaterialProcessorDefaultEffect))
						.Cast<MaterialProcessorDefaultEffect> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					var defaultValue = (MaterialProcessorDefaultEffect)p1.DefaultValue;
					propertygridtable1.AddProcEntry (p1.Name, (object)value ?? (object)defaultValue,
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach (object o in contentItems) 
							{
								((ContentItem)o).ProcessorParams[p1.Name] = (MaterialProcessorDefaultEffect)data[(string)((FalseWidget)s).newvalue];
								controller.OnItemModified ((ContentItem)o);
							}
						}, data);
					continue;
				}
				if (p1.Type == typeof(TextureProcessorOutputFormat)) {
					if (value == null)
						value = "";
					Dictionary<string, object> data = Enum.GetValues (typeof(TextureProcessorOutputFormat))
						.Cast<TextureProcessorOutputFormat> ()
						.ToDictionary (t => t.ToString(), t => (object)t);
					var defaultValue = (TextureProcessorOutputFormat)p1.DefaultValue;
					propertygridtable1.AddProcEntry (p1.Name, (object)value ?? (object)defaultValue,
						PropertyGridTable.EntryType.Combo,(s,e) => { 
							foreach (object o in contentItems) 
							{
								((ContentItem)o).ProcessorParams[p1.Name] = (TextureProcessorOutputFormat)data[(string)((FalseWidget)s).newvalue];
								controller.OnItemModified ((ContentItem)o);
							}
						}, data);
					continue;
				}
				if (p1.Type == typeof(Microsoft.Xna.Framework.Color)) {
					if(value == null)
						value = new Microsoft.Xna.Framework.Color();
					propertygridtable1.AddProcEntry (p1.Name, value, 
						PropertyGridTable.EntryType.Color, (s,e) => { 
							foreach (object o in contentItems) 
							{
								try {
									string[] cvalues = ((FalseWidget)s).newvalue.ToString().Replace (":", " ").Replace("}", " ").Split (' ');
									Microsoft.Xna.Framework.Color color = new Microsoft.Xna.Framework.Color();

									color.R = (byte)Convert.ToInt16 (cvalues [1]);
									color.G = (byte)Convert.ToInt16 (cvalues [3]);
									color.B = (byte)Convert.ToInt16 (cvalues [5]);
									color.A = (byte)Convert.ToInt16 (cvalues [7]);

									((ContentItem)o).ProcessorParams[p1.Name] = color;
									controller.OnItemModified ((ContentItem)o);
								}
								catch { }
							}
						});
					continue;
				}
				if (p1.Type == typeof(Single)) {
					if(value == null)
						value = "";
					propertygridtable1.AddProcEntry (p1.Name, value.ToString(), 
						PropertyGridTable.EntryType.Text, (s,e) => { 
							foreach (object o in contentItems) 
							{
								((ContentItem)o).ProcessorParams[p1.Name] =  Single.Parse ((string)((FalseWidget)s).newvalue).ToString();
								controller.OnItemModified ((ContentItem)o);
							}
						});
					continue;
				}
				propertygridtable1.AddProcEntry (p1.Name, null, PropertyGridTable.EntryType.Unkown);
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
