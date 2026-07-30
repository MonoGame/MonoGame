using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Microsoft.Xna.Framework.Design
{
    internal static class VectorConversion
    {
        [Pure]
        public static bool CanConvertTo(ITypeDescriptorContext context, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type destinationType)
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

            return false;
        }

        [Pure]
        public static object ConvertToFromVector4(ITypeDescriptorContext context, CultureInfo culture, Vector4 value, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces | DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type destinationType)
        {
            if (destinationType == typeof(float))
                return value.X;
            if (destinationType == typeof(Vector2))
                return new Vector2(value.X, value.Y);
            if (destinationType == typeof(Vector3))
                return new Vector3(value.X, value.Y, value.Z);
            if (destinationType == typeof(Vector4))
                return new Vector4(value.X, value.Y, value.Z, value.W);
            if (destinationType.GetInterface("IPackedVector") != null)
            {
                var packedVec = (IPackedVector)Activator.CreateInstance(destinationType);
                packedVec.PackFromVector4(value);
                return packedVec;
            }            

            return null;
        }         
    }
}
