using System;
using System.ComponentModel;
using System.Globalization;

namespace MonoDevelop.MonoGameContent
{
	public class ContentStringsConverter: TypeConverter 
	{
		#region TypeConverter Overrides

		public override bool CanConvertTo (ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType == typeof(string);
		}

		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);	
		}

		public override object ConvertFrom (ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			return value as string;
		}

		public override object ConvertTo (ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			return value as string;
		}

		public override bool GetStandardValuesSupported (ITypeDescriptorContext context)
		{
			return true;
		}

		public override bool GetStandardValuesExclusive (ITypeDescriptorContext context)
		{
			BaseFileWrapper wrapper = context != null ? context.Instance as BaseFileWrapper : null;
			if (wrapper != null && wrapper.file != null)
			{
				MonoGameContentProject project = wrapper.file.Project as MonoGameContentProject;
				return project != null;
			}
			return false;
		}

		#endregion TypeConverter Overrides
	}
}

