// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Xna.Framework.Design
{
    /// <summary>
    /// Provides a unified way of converting <see cref="Byte4"/> value to other types, as well as for accessing
    /// standard values and subproperties.
    /// </summary>
    public class Byte4TypeConverter : TypeConverter
    {
        /// <inheritdoc />
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (VectorConversion.CanConvertTo(context, destinationType))
                return true;
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        /// <inheritdoc />
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var vec = (Byte4)value;

            if (VectorConversion.CanConvertTo(context, destinationType))
            {
                var vec4 = vec.ToVector4();
                return VectorConversion.ConvertToFromVector4(context, culture, vec4, destinationType);
            }

            if (destinationType == typeof(string))
            {
                return vec.PackedValue.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        /// <inheritdoc />
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            
            if (sourceType == typeof(string))
                return true;
            
            return base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc />
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var sourceType = value.GetType();

            if (sourceType == typeof(string))
            {
                var vecx = (float)Convert.ToByte(value.ToString().Substring(6, 2), 16);
                var vecy = (float)Convert.ToByte(value.ToString().Substring(4, 2), 16);
                var vecz = (float)Convert.ToByte(value.ToString().Substring(2, 2), 16);
                var vecw = (float)Convert.ToByte(value.ToString().Substring(0, 2), 16);

                return new Byte4(vecx, vecy, vecz, vecw);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
