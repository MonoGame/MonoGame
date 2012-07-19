using System;
#if WINRT
using System.Runtime.Serialization;
#endif

namespace Microsoft.Xna.Framework.Graphics
{
    #if WINRT
    [DataContract]
    #else
    [Serializable]
    #endif
	public struct VertexElementColor
	{
#if WINRT
        [DataMember]
#endif
		byte R;
#if WINRT
        [DataMember]
#endif
		byte G;
#if WINRT
        [DataMember]
#endif
		byte B;
#if WINRT
        [DataMember]
#endif
		byte A;

		public VertexElementColor (Color color)
		{

			R = color.R;
			G = color.G;
			B = color.B;
			A = color.A;
		}

		public Color Color {
			get {
				return new Color (R, G, B, A);
			}

			set {
				R = value.R;
				G = value.G;
				B = value.B;
				A = value.A;
			}
		}

		public override string ToString ()
		{
			return string.Format ("[Color: R={0}, G={1}, B={2}, A={3}]", R, G, B, A);
		}

		public static implicit operator Color (VertexElementColor typ)
		{
			// code to convert from  Color to VertexElementColor
			// and return a Color object.
			Color c = new Color ();
			c.R = typ.R;
			c.G = typ.G;
			c.B = typ.B;
			c.A = typ.A;
			return c;
		}

		public static implicit operator VertexElementColor (Color typ)
		{
			// code to convert from  VertextElementColor to Color
			// and return a VertexElementColor object.
			VertexElementColor c = new VertexElementColor (typ);
			return c;
		}

		public static bool operator == (VertexElementColor left, Color right)
		{
			return ( left.R == right.R && left.G == right.G && left.B == right.B && left.A == right.A);
		}

		public static bool operator != (VertexElementColor left, Color right)
		{
			return !(left == right);
		}

		public override bool Equals (object obj)
		{
			if (obj == null) {
				return false;
			}
			if (obj.GetType () != base.GetType ()) {
				return false;
			}
			return (this == ((VertexElementColor)obj));
		}

        public override int GetHashCode()
        {
            return (int)PackedValue;
        }

        public UInt32 PackedValue
        {
            get
            {

                // ARGB
                uint _packedValue = 0;
                _packedValue = (_packedValue & 0xffffff00) | R;
                _packedValue = (_packedValue & 0xffff00ff) | ((uint)(G << 8));
                _packedValue = (_packedValue & 0xff00ffff) | ((uint)(B << 16));
                _packedValue = (_packedValue & 0x00ffffff) | ((uint)(A << 24));
                return _packedValue;
            }
            set
            {
                R = (byte)value;
                G = (byte)(value >> 8);
                B = (byte)(value >> 16);
                A = (byte)(value >> 24);
            }
        }
	}

}

