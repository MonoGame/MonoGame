using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Builder.Convertors
{
	class StringToColorConverter : TypeConverter
	{
		public override bool CanConvertFrom (ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof (string))
				return true;

			return base.CanConvertFrom (context, sourceType);
		}

		public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			if (value.GetType () == typeof (string)) {
				string[] values = ((string)value).Split(new char[] {','},StringSplitOptions.None);
				if (values.Length == 4) {
					var r = int.Parse(values[0].Trim());
					var g = int.Parse(values[1].Trim());
					var b =	int.Parse(values[2].Trim());
					var a =	int.Parse (values[3].Trim ());
					return new Microsoft.Xna.Framework.Color (r,g,b,a);
				}
			}

			return base.ConvertFrom (context, culture, value);
		}
	}
}
