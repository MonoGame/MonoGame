using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Design
{
    public class ColorTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Color))
                return true;
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var colour = (Color)value;

            if (destinationType == typeof(Color))
            {                
                return colour;
            }

            if (destinationType == typeof(string))
            {
                var terms = new string[4];
                terms[0] = colour.R.ToString(culture);
                terms[1] = colour.G.ToString(culture);
                terms[2] = colour.B.ToString(culture);
                terms[3] = colour.A.ToString(culture);

                return string.Join(culture.TextInfo.ListSeparator + " ", terms);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }


        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var sourceType = value.GetType();
            var color = Color.Transparent;

            if (sourceType == typeof(string))
            {
                var str = (string)value;
                var words = str.Split(culture.TextInfo.ListSeparator.ToCharArray());

                color.R = byte.Parse(words[0], culture);
                color.G = byte.Parse(words[1], culture);
                color.B = byte.Parse(words[2], culture);
                color.A = byte.Parse(words[3], culture);
                
                return color;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
