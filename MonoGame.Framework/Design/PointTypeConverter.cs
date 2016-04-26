using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Design
{
    public class PointTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(int))
                return true;
            if (destinationType == typeof(Point))
                return true;
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {

            }

            var point = (Point)value;


            if (destinationType == typeof(int))
            {
                return point.X;
            }

            if (destinationType == typeof(Point))
            {
                return point;
            }



            if (destinationType == typeof(string))
            {
                var terms = new string[2];
                terms[0] = point.X.ToString(culture);
                terms[1] = point.Y.ToString(culture);

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
            try
            {
                throw new Exception();
            }
            catch (Exception ex)
            {

            }


            var sourceType = value.GetType();
            var point = Point.Zero;

            if (sourceType == typeof(string))
            {
                var str = (string)value;
                var words = str.Split(culture.TextInfo.ListSeparator.ToCharArray());

                point.X = int.Parse(words[0], culture);
                point.Y = int.Parse(words[1], culture);

                return point;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
