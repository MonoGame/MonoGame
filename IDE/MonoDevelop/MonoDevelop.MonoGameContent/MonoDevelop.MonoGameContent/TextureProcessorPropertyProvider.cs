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
		[DefaultValue("Compressed")]
		[TypeConverter(typeof(TextureFormatStringsConverter))]
		public string Name {
			get {
				object result = file.ExtendedProperties["ProcessorParameters_TextureFormat"];
				return result == null ? "Compressed" : (string)result;
			}
			set { file.ExtendedProperties["ProcessorParameters_TextureFormat"] = value; }
		}

		[LocalizedCategory("TextureProcessor")]
		[LocalizedDisplayName("PremultiplyAlpha")]
		[LocalizedDescription("Premultiply the Alpha channel for the texture")]
		[DefaultValue(true)]
		public bool PremultiplyAlpha {
			get {
				object result = file.ExtendedProperties["ProcessorParameters_PremultiplyAlpha"];
				return result == null ? true : (bool)result;
			}
			set { file.ExtendedProperties["ProcessorParameters_PremultiplyAlpha"] = value; }
		}

		[LocalizedCategory("TextureProcessor")]
		[LocalizedDisplayName("GenerateMipmaps")]
		[LocalizedDescription("Generate Mipmaps for this texture")]
		public bool GenerateMipmaps {
			get {
				object result = file.ExtendedProperties["ProcessorParameters_GenerateMipmaps"];
				return result == null ? false : (bool)result;
			}
			set { file.ExtendedProperties["ProcessorParameters_GenerateMipmaps"] = value; }
		}

		[LocalizedCategory("TextureProcessor")]
		[LocalizedDisplayName("ResizeToPowerOfTwo")]
		[LocalizedDescription("Resize the texture to a power of 2")]
		public bool ResizeToPowerOfTwo {
			get {
				object result = file.ExtendedProperties["ProcessorParameters_ResizeToPowerOfTwo"];
				return result == null ? false : (bool)result;
			}
			set { file.ExtendedProperties["ProcessorParameters_ResizeToPowerOfTwo"] = value; }
		}

		[LocalizedCategory("TextureProcessor")]
		[LocalizedDisplayName("ColorKeyEnabled")]
		[LocalizedDescription("Enable color keying for this texture")]
		public bool ColorKeyEnabled {
			get {
				object result = file.ExtendedProperties["ProcessorParameters_ColorKeyEnabled"];
				return result == null ? false : (bool)result;
			}
			set { 
				file.ExtendedProperties["ProcessorParameters_ColorKeyEnabled"] = value; 
			}
		}

		[LocalizedCategory("TextureProcessor")]
		[LocalizedDisplayName("ColorKeyColor")]
		[LocalizedDescription("The key color for this texture")]
		[DefaultValue("255, 0, 255, 255")]
		public string ColorKeyColor {
			get {
				object result = file.ExtendedProperties["ProcessorParameters_ColorKeyColor"];
				return result == null ? "255, 0, 255, 255" : (string)result;
			}
			set { file.ExtendedProperties["ProcessorParameters_ColorKeyColor"] = value; }
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

