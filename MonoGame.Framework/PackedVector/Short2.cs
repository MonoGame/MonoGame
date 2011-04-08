using System;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public struct Short2 : IPackedVector<uint>, IPackedVector, IEquatable<Short2>
	{
		private uint short2Packed;

		public Short2 (Vector2 vector)
		{
			short2Packed = Short2.PackInTwo (vector.X, vector.Y);
		}

		public Short2 (Single x,Single y)
		{
			short2Packed = Short2.PackInTwo (x, y);
		}

		public static bool operator !=(Short2 a, Short2 b)
		{
			return !a.Equals (b);
		}

		public static bool operator ==(Short2 a, Short2 b)
		{
			return a.Equals (b);
		}

		public uint PackedValue { 
			get { 
				return short2Packed; 
			} 
			set { 
				short2Packed = value; 
			} 
		}

		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}

		public bool Equals (Short2 other)
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			return short2Packed.GetHashCode();
		}

		public override string ToString ()
		{
			// not sure what to return here
			// microsoft returns some funky formatted string
			return string.Format("{0} / {1}", (short)(short2Packed & 0xFFFF), (short)(short2Packed >> 0x10) );
		}

		public Vector2 ToVector2 ()
		{
			Vector2 v2 = new Vector2 ();
			v2.X = (short)(short2Packed & 0xFFFF);
			v2.Y = (short)(short2Packed >> 0x10);
			return v2;
		}

		private static uint PackInTwo (float vectorX, float vectorY)
		{
			float maxPos = 0x7FFF; // Largest two byte positive number 0xFFFF >> 1; 
			float minNeg = ~(int)maxPos; // two's complement

			// clamp the value between min and max values
			uint word2 = (uint)((int)Math.Max (Math.Min (vectorX, maxPos), minNeg) & 0xFFFF);
			uint word1 = (uint)(((int)Math.Max (Math.Min (vectorY, maxPos), minNeg) & 0xFFFF) << 0x10);

			return (word2 | word1);
		}

		void IPackedVector.PackFromVector4 (Vector4 vector)
		{
			short2Packed = Short2.PackInTwo (vector.X, vector.Y);
		}

		Vector4 IPackedVector.ToVector4 ()
		{
			Vector4 v4 = new Vector4 (0,0,0,1);
			v4.X = (short)(short2Packed & 0xFFFF);
			v4.Y = (short)(short2Packed >> 0x10);
			return v4;
		}
	}
}
