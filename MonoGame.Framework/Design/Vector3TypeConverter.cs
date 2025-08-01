﻿// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

#pragma warning disable IL2067

namespace Microsoft.Xna.Framework.Design
{
    /// <summary>
    /// Provides a unified way of converting <see cref="Vector3"/> values to other  types, as well as for accessing
    /// standard values and subproperties.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public sealed class Vector3TypeConverter : TypeConverter
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
            var vec = (Vector3)value;

            if (VectorConversion.CanConvertTo(context, destinationType))
            {
                var vec4 = new Vector4(vec.X, vec.Y, vec.Z, 0.0f);
                return VectorConversion.ConvertToFromVector4(context, culture, vec4, destinationType);
            }

            if (destinationType == typeof(string))
            {                
                var terms = new string[3];
                terms[0] = vec.X.ToString("R", culture);
                terms[1] = vec.Y.ToString("R", culture);
                terms[2] = vec.Z.ToString("R", culture);

                return string.Join(culture.TextInfo.ListSeparator + " ", terms);
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
            var vec = Vector3.Zero;

            if (sourceType == typeof(string))
            {
                var str = (string)value;
                var words = str.Split(culture.TextInfo.ListSeparator.ToCharArray());

                vec.X = float.Parse(words[0], culture);
                vec.Y = float.Parse(words[1], culture);
                vec.Z = float.Parse(words[2], culture);

                return vec;
            }

            return base.ConvertFrom(context, culture, value);
        }
    }
}

#pragma warning restore IL2067
