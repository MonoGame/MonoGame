using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace MonoGame.Tools.Pipeline
{
    class SortedEnumTypeConverter : EnumConverter
    {
        public SortedEnumTypeConverter(Type type) : 
            base(type)
        {
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            var values = new ArrayList(base.GetStandardValues(context));
            values.Sort();
            return new StandardValuesCollection(values);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string) || sourceType == EnumType)
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value != null)
            {
                if (value.GetType() == EnumType)
                    return value;

                if (value is string)
                    return Enum.Parse(EnumType, value as string, true);
            }

            return base.ConvertFrom(context, culture, value);
        }


        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) && value != null)
            {
                if (value is string)
                    return value;

                return Enum.GetName(EnumType, value);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
