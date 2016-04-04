// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.


using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector
{
	public struct Short2 : IPackedVector<uint>, IEquatable<Short2>
	{
		private uint _short2Packed;

		public Short2 (Vector2 vector)
		{
			_short2Packed = PackInTwo (vector.X, vector.Y);
		}

		public Short2 (Single x,Single y)
		{
			_short2Packed = PackInTwo (x, y);
		}

		public static bool operator !=(Short2 a, Short2 b)
		{
			return a.PackedValue != b.PackedValue;
		}

		public static bool operator ==(Short2 a, Short2 b)
		{
			return a.PackedValue == b.PackedValue;
		}

        [CLSCompliant(false)]
		public uint PackedValue
        {
			get
            {
				return _short2Packed;
			}
			set
            {
				_short2Packed = value;
			}
		}

		public override bool Equals (object obj)
		{
            if (obj is Short2)
                return this == (Short2)obj;
            return false;
		}

		public bool Equals (Short2 other)
		{
            return this == other;
		}

		public override int GetHashCode ()
		{
			return _short2Packed.GetHashCode();
		}

		public override string ToString ()
		{
            return _short2Packed.ToString("x8");
		}

		public Vector2 ToVector2 ()
		{
			var v2 = new Vector2 ();
			v2.X = (short)(_short2Packed & 0xFFFF);
			v2.Y = (short)(_short2Packed >> 0x10);
			return v2;
		}

		private static uint PackInTwo (float vectorX, float vectorY)
		{
			const float maxPos = 0x7FFF; // Largest two byte positive number 0xFFFF >> 1;
			const float minNeg = ~(int)maxPos; // two's complement

            // clamp the value between min and max values
            var word2 = ((uint) Math.Round(MathHelper.Clamp(vectorX, minNeg, maxPos)) & 0xFFFF);
            var word1 = (((uint) Math.Round(MathHelper.Clamp(vectorY, minNeg, maxPos)) & 0xFFFF) << 0x10);

            return (word2 | word1);
		}

		void IPackedVector.PackFromVector4 (Vector4 vector)
		{
			_short2Packed = Short2.PackInTwo (vector.X, vector.Y);
		}

		Vector4 IPackedVector.ToVector4 ()
		{
			var v4 = new Vector4 (0,0,0,1);
			v4.X = (short)(_short2Packed & 0xFFFF);
			v4.Y = (short)(_short2Packed >> 0x10);
			return v4;
		}
	}
}
