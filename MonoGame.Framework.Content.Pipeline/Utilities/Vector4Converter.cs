using Microsoft.Xna.Framework.Graphics.PackedVector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities
{
    interface IVector4Converter<T> where T : struct
    {
        Vector4 ToVector4(T value);
        T FromVector4(Vector4 value);
    }

    class Vector4Converter :
        IVector4Converter<byte>,
        IVector4Converter<short>,
        IVector4Converter<int>,
        IVector4Converter<float>,
        IVector4Converter<Color>
    {
        Vector4 IVector4Converter<byte>.ToVector4(byte value)
        {
            return new Vector4(value, value, value, value);
        }

        Vector4 IVector4Converter<short>.ToVector4(short value)
        {
            return new Vector4(value, value, value, value);
        }

        Vector4 IVector4Converter<int>.ToVector4(int value)
        {
            return new Vector4(value, value, value, value);
        }

        Vector4 IVector4Converter<float>.ToVector4(float value)
        {
            return new Vector4(value, value, value, value);
        }

        Vector4 IVector4Converter<Color>.ToVector4(Color value)
        {
            return value.ToVector4();
        }

        byte IVector4Converter<byte>.FromVector4(Vector4 value)
        {
            return (byte)value.X;
        }

        short IVector4Converter<short>.FromVector4(Vector4 value)
        {
            return (short)value.X;
        }

        int IVector4Converter<int>.FromVector4(Vector4 value)
        {
            return (int)value.X;
        }

        float IVector4Converter<float>.FromVector4(Vector4 value)
        {
            return value.X;
        }

        Color IVector4Converter<Color>.FromVector4(Vector4 value)
        {
            return new Color(value);
        }
    }
}
