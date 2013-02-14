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
	public struct NormalizedShort2 : IPackedVector<uint>, IEquatable<NormalizedShort2>
	{
		private uint short2Packed;

        public NormalizedShort2(Vector2 vector)
		{
            short2Packed = PackInTwo(vector.X, vector.Y);
		}

        public NormalizedShort2(float x, float y)
		{
            short2Packed = PackInTwo(x, y);
		}

        public static bool operator !=(NormalizedShort2 a, NormalizedShort2 b)
		{
			return !a.Equals (b);
		}

        public static bool operator ==(NormalizedShort2 a, NormalizedShort2 b)
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
            return (obj is NormalizedShort2) && Equals((NormalizedShort2)obj);
		}

        public bool Equals(NormalizedShort2 other)
		{
            return short2Packed.Equals(other.short2Packed);
		}

		public override int GetHashCode ()
		{
			return short2Packed.GetHashCode();
		}

		public override string ToString ()
		{
            return short2Packed.ToString("X");
		}

		public Vector2 ToVector2 ()
		{
            const float maxVal = 0x7FFF;

			var v2 = new Vector2 ();
            v2.X = ((short)(short2Packed & 0xFFFF)) / maxVal;
            v2.Y = (short)(short2Packed >> 0x10) / maxVal;
			return v2;
		}

		private static uint PackInTwo (float vectorX, float vectorY)
		{
			const float maxPos = 0x7FFF;
			const float minNeg = ~(int)maxPos;

			// clamp the value between min and max values
            // Round rather than truncate.
            var word2 = (uint)((int)MathHelper.Clamp((float)Math.Round(vectorX * maxPos), minNeg, maxPos) & 0xFFFF);
            var word1 = (uint)(((int)MathHelper.Clamp((float)Math.Round(vectorY * maxPos), minNeg, maxPos) & 0xFFFF) << 0x10);

			return (word2 | word1);
		}

		void IPackedVector.PackFromVector4 (Vector4 vector)
		{
            short2Packed = PackInTwo(vector.X, vector.Y);
		}

		Vector4 IPackedVector.ToVector4 ()
		{
			var v4 = new Vector4 (0,0,0,1);
			v4.X = (short)(short2Packed & 0xFFFF);
			v4.Y = (short)(short2Packed >> 0x10);
			return v4;
		}
	}
}
