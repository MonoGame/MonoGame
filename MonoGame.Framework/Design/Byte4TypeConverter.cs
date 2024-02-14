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
    /// Provides a unified way of converting <see cref="Byte4"/> values to other 
    /// types, as well as for accessing standard values and subproperties.
    /// </summary>
    public class Byte4TypeConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this <see cref="Byte4TypeConverter"/> can convert the 
        /// object to the specified type, using the specified context.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="destinationType"> A <see cref="Type" /> that represents the type you want to convert to.</param>
        /// <returns>
        /// <see langword="true"/> if this converter can perform the conversion; 
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (VectorConversion.CanConvertTo(context, destinationType))
                return true;
            if (destinationType == typeof(string))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the 
        /// specified context and culture information.
        /// </summary>
        /// <param name="context">An <see cref="ITypeDescriptorContext" /> that provides a format context.</param>
        /// <param name="culture">
        /// A <see cref="CultureInfo"/>. If <see langword="null"/> is passed, the current culture is assumed.
        /// </param>
        /// <param name="value">The <see cref="object"/> to convert.</param>
        /// <param name="destinationType">The <see cref="Type"/> to covert the <paramref name="value"/> to.</param>
        /// <returns>A <see cref="object"/> that represents the converted value.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="destinationType"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// Thrown if the conversion cannot be performed.
        /// </exception>
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

        /// <summary>
        /// Returns whether this <see cref="Byte4TypeConverter"/> can convert an
        /// object of the given type to <see cref="Byte4"/>, using the specified
        /// context.
        /// </summary>
        /// <param name="context">A <see cref="ITypeDescriptorContext"/> that provides the format context.</param>
        /// <param name="sourceType">A <see cref="Type"/> that represents the type you want to convert from.</param>
        /// <returns>
        /// <see langword="true"/> if this <see cref="Byte4TypeConverter"/> can
        /// perform the conversion; otherwise, <see langword="false"/>.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            
            if (sourceType == typeof(string))
                return true;
            
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given object to <see cref="Byte4"/>, using the specified
        /// context and culture information.
        /// </summary>
        /// <param name="context">A <see cref="ITypeDescriptorContext"/> that provides a format context.</param>
        /// <param name="culture">The <see cref="CultureInfo"/> to use as the current culture.</param>
        /// <param name="value">The <see cref="object"/> to convert.</param>
        /// <returns>A <see cref="object"/> that represents the converted value.</returns>
        /// <exception cref="NotSupportedException">
        /// Thrown if the conversion cannot be performed.
        /// </exception>
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
