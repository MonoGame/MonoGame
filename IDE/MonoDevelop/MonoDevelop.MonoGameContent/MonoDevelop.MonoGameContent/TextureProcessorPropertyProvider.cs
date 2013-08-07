using System;
using MonoDevelop.DesignerSupport;
using MonoDevelop.Projects;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using MonoDevelop.Core;
using System.ComponentModel;
using System.Collections.Generic;

namespace MonoDevelop.MonoGameContent
{
	public class TextureProcessorPropertyProvider :  IPropertyProvider
	{
		public TextureProcessorPropertyProvider ()
		{
		}

		public bool SupportsObject (object o)
		{
			bool result = false;
			ProjectFile file = o as ProjectFile;
			if (file != null) {
				MonoGameContentProject proj = file.Project as MonoGameContentProject;
				if (proj != null) {
					Type processor = proj.Manager.GetProcessorType ((string)file.ExtendedProperties["Processor"]);
					if (processor != null && processor.Equals(typeof(TextureProcessor))) {
						result = true;
					}
				}
			}
			return result;
		}

		public object CreateProvider (object o)
		{
			return new TextureProcessorFileWrapper ((ProjectFile)o);
		}	
	}

	class BaseFileWrapper : CustomDescriptor {

		internal ProjectFile file;

		public BaseFileWrapper (ProjectFile file)
		{
			this.file = file;
		}
	}

	class TextureProcessorFileWrapper : BaseFileWrapper {

		public TextureProcessorFileWrapper (ProjectFile file) :base(file)
		{
		}

		
		[LocalizedCategory("TextureProcessor")]
		[LocalizedDisplayName("TextureFormat")]
		[LocalizedDescription("The Format of the texture")]
		[TypeConverter(typeof(TextureFormatStringsConverter))]
		public string Name {
			get {
				object result = file.ExtendedProperties["ProcessorParameters_TextureFormat"];
				return result == null ? "" : (string)result;
			}
			set { file.ExtendedProperties["ProcessorParameters_TextureFormat"] = value; }
		}

		[MonoDevelop.Components.PropertyGrid.PropertyEditors.StandardValuesSeparator("--")]
		class TextureFormatStringsConverter : ContentStringsConverter
		{
			List<string> valid = new List<string>();

			public TextureFormatStringsConverter ()
			{
				valid.Add(TextureProcessorOutputFormat.Color.ToString());
				valid.Add(TextureProcessorOutputFormat.Compressed.ToString());
				valid.Add(TextureProcessorOutputFormat.DXTCompressed.ToString());
			}

			#region TypeConverter Overrides

			public override StandardValuesCollection GetStandardValues (ITypeDescriptorContext context)
			{
				BaseFileWrapper wrapper = context != null ? context.Instance as BaseFileWrapper : null;
				if (wrapper != null && wrapper.file != null)
				{
					MonoGameContentProject project = wrapper.file.Project as MonoGameContentProject;
					if (project != null)
						return new StandardValuesCollection(valid);
				}
				return new StandardValuesCollection(null);
			}

			public override bool IsValid (ITypeDescriptorContext context, object value)
			{
				if (!(value is string))
					return false;
				string str = value as string;
				TextureProcessorOutputFormat v;
				return Enum.TryParse<TextureProcessorOutputFormat> (str, out v);
			}

			#endregion TypeConverter Overrides
		}

	}
}

