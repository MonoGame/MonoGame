// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    public struct NormalizedByte4 : IPackedVector<uint>, IEquatable<NormalizedByte4>
    {
        private uint _packed;

        public NormalizedByte4(Vector4 vector)
        {
            _packed = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        public NormalizedByte4(float x, float y, float z, float w)
        {
            _packed = Pack(x, y, z, w);
        }

        public static bool operator !=(NormalizedByte4 a, NormalizedByte4 b)
        {
            return a._packed != b._packed;
        }

        public static bool operator ==(NormalizedByte4 a, NormalizedByte4 b)
        {
            return a._packed == b._packed;
        }

        [CLSCompliant(false)]
        public uint PackedValue
        {
            get
            {
                return _packed;
            }
            set
            {
                _packed = value;
            }
        }

        public override bool Equals(object obj)
        {
            return  (obj is NormalizedByte4) &&
                    ((NormalizedByte4)obj)._packed == _packed;
        }

        public bool Equals(NormalizedByte4 other)
        {
            return _packed == other._packed;
        }

        public override int GetHashCode()
        {
            return _packed.GetHashCode();
        }

        public override string ToString()
        {
            return _packed.ToString("X");
        }

        private static uint Pack(float x, float y, float z, float w)
        {
            var byte4 = (((uint) Math.Round(MathHelper.Clamp(x, -1.0f, 1.0f) * 127.0f)) & 0xff) << 0;
            var byte3 = (((uint) Math.Round(MathHelper.Clamp(y, -1.0f, 1.0f) * 127.0f)) & 0xff) << 8;
            var byte2 = (((uint) Math.Round(MathHelper.Clamp(z, -1.0f, 1.0f) * 127.0f)) & 0xff) << 16;
            var byte1 = (((uint) Math.Round(MathHelper.Clamp(w, -1.0f, 1.0f) * 127.0f)) & 0xff) << 24;

            return byte4 | byte3 | byte2 | byte1;
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            _packed = Pack(vector.X, vector.Y, vector.Z, vector.W);
        }

        public Vector4 ToVector4()
        {
            return new Vector4(
                ((sbyte) ((_packed >> 0) & 0xFF)) / 127.0f,
                ((sbyte) ((_packed >> 8) & 0xFF)) / 127.0f,
                ((sbyte) ((_packed >> 16) & 0xFF)) / 127.0f,
                ((sbyte) ((_packed >> 24) & 0xFF)) / 127.0f);
        }
    }
}
