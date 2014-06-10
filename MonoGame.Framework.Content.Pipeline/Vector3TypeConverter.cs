// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework
{
    public class Vector3TypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(float))
                return true;
            if (destinationType == typeof(Vector2))            
                return true;
            if (destinationType == typeof(Vector3))
                return true;
            if (destinationType == typeof(Vector4))
                return true;
            if (destinationType.GetInterface("IPackedVector") != null)
                return true;            
            
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var vec3 = (Vector3) value;
            var vec4 = new Vector4(vec3.X, vec3.Y, vec3.Z, 0.0f);

            if (destinationType == typeof(float))
                return vec4.X;
            if (destinationType == typeof(Vector2))
                return new Vector2(vec4.X, vec4.Y);
            if (destinationType == typeof(Vector3))
                return new Vector3(vec4.X, vec4.Y, vec4.Z);
            if (destinationType == typeof(Vector4))
                return new Vector4(vec4.X, vec4.Y, vec4.Z, vec4.W);
            if (destinationType.GetInterface("IPackedVector") != null)
            {
                var packedVec = (IPackedVector)Activator.CreateInstance(destinationType);
                packedVec.PackFromVector4(vec4);
                return packedVec;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
