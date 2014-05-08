using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Builder.Convertors
{
	public class StringToColorConverter : TypeConverter
	{
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof (string))            
                return base.ConvertTo(context, culture, value, destinationType);

            var color = (Color)value;
            return string.Format("{0},{1},{2},{3}", color.R, color.G, color.B, color.A);
        }

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
                if (values.Length == 4)
                {
                    var r = int.Parse(values[0].Trim());
                    var g = int.Parse(values[1].Trim());
                    var b = int.Parse(values[2].Trim());
                    var a = int.Parse(values[3].Trim());
                    return new Microsoft.Xna.Framework.Color(r, g, b, a);
                }
                else
                {
                    throw new ArgumentException(string.Format("Could not convert from string({0}) to Color, expected format is 'r,g,b,a'", value));                    
                }
			}

			return base.ConvertFrom (context, culture, value);
		}
	}
}
