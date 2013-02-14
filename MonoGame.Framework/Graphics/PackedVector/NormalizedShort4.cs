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
    public struct NormalizedShort4 : IPackedVector<ulong>, IEquatable<NormalizedShort4>
	{
		private ulong short4Packed;

        public NormalizedShort4(Vector4 vector)
		{
            short4Packed = PackInFour(vector.X, vector.Y, vector.Z, vector.W);
		}

        public NormalizedShort4(float x, float y, float z, float w)
		{
            short4Packed = PackInFour(x, y, z, w);
		}

        public static bool operator !=(NormalizedShort4 a, NormalizedShort4 b)
		{
			return !a.Equals (b);
		}

        public static bool operator ==(NormalizedShort4 a, NormalizedShort4 b)
		{
			return a.Equals (b);
		}

        public ulong PackedValue
        {
			get { 
				return short4Packed; 
			} 
			set { 
				short4Packed = value; 
			} 
		}

        public override bool Equals(object obj)
        {
            return (obj is NormalizedShort4) && Equals((NormalizedShort4)obj);
        }

        public bool Equals(NormalizedShort4 other)
        {
            return short4Packed.Equals(other.short4Packed);
        }

		public override int GetHashCode ()
		{
			return short4Packed.GetHashCode();
		}

		public override string ToString ()
		{
            return short4Packed.ToString("X");
		}

        private static ulong PackInFour(float vectorX, float vectorY, float vectorZ, float vectorW)
		{
			const float maxPos = 0x7FFF;
			const float minNeg = ~(int)maxPos;

			// clamp the value between min and max values
            var word4 = (ulong)((int)MathHelper.Clamp((float)Math.Round(vectorX * maxPos), minNeg, maxPos) & 0xFFFFFFFF);
            var word3 = (ulong)(((int)MathHelper.Clamp((float)Math.Round(vectorY * maxPos), minNeg, maxPos) & 0xFFFFFFFF) << 0x10);
            var word2 = (ulong)(((int)MathHelper.Clamp((float)Math.Round(vectorZ * maxPos), minNeg, maxPos) & 0xFFFFFFFF) << 0x20);
            var word1 = (ulong)(((int)MathHelper.Clamp((float)Math.Round(vectorW * maxPos), minNeg, maxPos) & 0xFFFFFFFF) << 0x30);

			return ( word4 | word3 | word2 | word1 );
		}

		void IPackedVector.PackFromVector4 (Vector4 vector)
		{
            short4Packed = PackInFour(vector.X, vector.Y, vector.Z, vector.W);
		}

		public Vector4 ToVector4 ()
		{
            const float maxVal = 0x7FFF;

			var v4 = new Vector4 ();
            v4.X = ((short)(short4Packed & 0xFFFFFFFF)) / maxVal;
            v4.Y = ((short)(short4Packed >> 0x10) & 0xFFFFFFFF) / maxVal;
            v4.Z = ((short)(short4Packed >> 0x20) & 0xFFFFFFFF) / maxVal;
            v4.W = (short)(short4Packed >> 0x30) / maxVal;
			return v4;
		}
	}
}
