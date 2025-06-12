// MonoGame - Copyright (C) MonoGame Foundation, Inc
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities
{
    interface IVector4Converter<T> where T : struct
    {
        Vector4 ToVector4(T value);
        T FromVector4(Vector4 value);
    }

    /// <summary>
    /// Helper class used by PixelBitmapContent.TryCopyFrom and TryCopyTo to convert between non-PackedValue types and Vector4.
    /// </summary>
    class Vector4Converter :
        IVector4Converter<byte>,
        IVector4Converter<short>,
        IVector4Converter<int>,
        IVector4Converter<float>,
        IVector4Converter<Color>,
        IVector4Converter<Vector4>
    {
        Vector4 IVector4Converter<byte>.ToVector4(byte value)
        {
            var f = (float)value / (float)byte.MaxValue;
            return new Vector4(f, 0f, 0f, 1f);
        }

        Vector4 IVector4Converter<short>.ToVector4(short value)
        {
            var f = (float)value / (float)short.MaxValue;
            return new Vector4(f, 0f, 0f, 1f);
        }

        Vector4 IVector4Converter<int>.ToVector4(int value)
        {
            var f = (float)value / (float)int.MaxValue;
            return new Vector4(f, 0f, 0f, 1f);
        }

        Vector4 IVector4Converter<float>.ToVector4(float value)
        {
            return new Vector4(value, 0f, 0f, 1f);
        }

        Vector4 IVector4Converter<Color>.ToVector4(Color value)
        {
            return value.ToVector4();
        }

        Vector4 IVector4Converter<Vector4>.ToVector4(Vector4 value)
        {
            return value;
        }

        byte IVector4Converter<byte>.FromVector4(Vector4 value)
        {
            return (byte)(value.X * (float)byte.MaxValue);
        }

        short IVector4Converter<short>.FromVector4(Vector4 value)
        {
            return (short)(value.X * (float)short.MaxValue);
        }

        int IVector4Converter<int>.FromVector4(Vector4 value)
        {
            return (int)(value.X * (float)int.MaxValue);
        }

        float IVector4Converter<float>.FromVector4(Vector4 value)
        {
            return value.X;
        }

        Color IVector4Converter<Color>.FromVector4(Vector4 value)
        {
            return new Color(value);
        }

        Vector4 IVector4Converter<Vector4>.FromVector4(Vector4 value)
        {
            return value;
        }
    }
}
