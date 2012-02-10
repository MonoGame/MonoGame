using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
    public struct HalfSingle : IPackedVector<UInt16>, IEquatable<HalfSingle>, IPackedVector
    {
        UInt16 packedValue;

        public HalfSingle(float single)
        {
            packedValue = HalfTypeHelper.convert(single);
        }

        public ushort PackedValue
        {
            get
            {
                return this.packedValue;
            }
            set
            {
                this.packedValue = value;
            }
        }

        public float ToSingle()
        {
            return HalfTypeHelper.convert(this.packedValue);
        }

        void IPackedVector.PackFromVector4(Vector4 vector)
        {
            this.packedValue = HalfTypeHelper.convert(vector.X);
        }

        Vector4 IPackedVector.ToVector4()
        {
            return new Vector4(this.ToSingle(), 0f, 0f, 1f);
        }

        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == this.GetType())
            {
                return this == (HalfSingle)obj;
            }

            return false;
        }

        public bool Equals(HalfSingle other)
        {
            return this.packedValue == other.packedValue;
        }

        public override string ToString()
        {
            return this.ToSingle().ToString();
        }

        public override int GetHashCode()
        {
            return this.packedValue.GetHashCode();
        }

        public static bool operator ==(HalfSingle lhs, HalfSingle rhs)
        {
            return lhs.packedValue == rhs.packedValue;
        }

        public static bool operator !=(HalfSingle lhs, HalfSingle rhs)
        {
            return lhs.packedValue != rhs.packedValue;
        }
    }
}
