// #region License
// /*
// Microsoft Public License (Ms-PL)
// MonoGame - Copyright © 2009 The MonoGame Team
// 
// All rights reserved.
// 
// This license governs use of the accompanying software. If you use the software, you accept this license. If you do not
// accept the license, do not use the software.
// 
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under 
// U.S. copyright law.
// 
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
// 
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, 
// each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
// 
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, 
// your patent license from such contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution 
// notices that are present in the software.
// (D) If you distribute any portion of the software in source code form, you may do so only under this license by including 
// a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object 
// code form, you may only do so under a license that complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees
// or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent
// permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular
// purpose and non-infringement.
// */
// #endregion License
//
// Author: Kenneth James Pouncey

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
			var word2 = (uint)((int)Math.Max (Math.Min (vectorX, maxPos), minNeg) & 0xFFFF);
			var word1 = (uint)(((int)Math.Max (Math.Min (vectorY, maxPos), minNeg) & 0xFFFF) << 0x10);

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
