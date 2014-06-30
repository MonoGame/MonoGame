﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Xna.Framework.Design
{
    public class Vector4TypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (VectorConversion.CanConvertTo(context, destinationType))
                return true;
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var vec = (Vector4)value;

            if (VectorConversion.CanConvertTo(context, destinationType))
            {
                return VectorConversion.ConvertToFromVector4(context, culture, vec, destinationType);
            }

            if (destinationType == typeof(string))
            {
                var terms = new string[3];
                terms[0] = vec.X.ToString(culture);
                terms[1] = vec.Y.ToString(culture);
                terms[2] = vec.Z.ToString(culture);
                terms[3] = vec.W.ToString(culture);

                return string.Join(culture.NumberFormat.NumberGroupSeparator, terms);
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
            var vec = Vector4.Zero;

            if (sourceType == typeof(string))
            {
                var str = (string)value;
                var words = str.Split(culture.NumberFormat.NumberGroupSeparator.ToCharArray());

                vec.X = float.Parse(words[0]);
                vec.Y = float.Parse(words[1]);
                vec.Z = float.Parse(words[2]);
                vec.Z = float.Parse(words[3]);

                return vec;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}
    