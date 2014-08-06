#region license

/*
Copyright (c) 2013, Milosz Krajewski
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided
that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions
  and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions
  and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN
IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#endregion

using System;

/*
NOTE:
	This file is shared between LZ4n and LZ4s.
	If you would like to modify this file please keep in mind that your changes will
	affect both projects.
	Use 'LZ4s' conditional define to differentiate
*/

// ReSharper disable InconsistentNaming

#if LZ4s
namespace LZ4s
#else
namespace LZ4n
#endif
{
	public static partial class LZ4Codec
	{
		#region configuration

		/// <summary>
		/// Memory usage formula : N->2^N Bytes (examples : 10 -> 1KB; 12 -> 4KB ; 16 -> 64KB; 20 -> 1MB; etc.)
		/// Increasing memory usage improves compression ratio
		/// Reduced memory usage can improve speed, due to cache effect
		/// Default value is 14, for 16KB, which nicely fits into Intel x86 L1 cache
		/// </summary>
		private const int MEMORY_USAGE = 14;

		/// <summary>
		/// Decreasing this value will make the algorithm skip faster data segments considered "incompressible"
		/// This may decrease compression ratio dramatically, but will be faster on incompressible data
		/// Increasing this value will make the algorithm search more before declaring a segment "incompressible"
		/// This could improve compression a bit, but will be slower on incompressible data
		/// The default value (6) is recommended
		/// </summary>
		private const int NOTCOMPRESSIBLE_DETECTIONLEVEL = 6;

#if LZ4s

		/// <summary>Buffer length when Buffer.BlockCopy becomes faster than straight loop.
		/// Please note that safe implementation REQUIRES it to be greater (not even equal) than 8.</summary>
		private const int BLOCK_COPY_LIMIT = 16;
#endif

		#endregion

		#region consts

		private const int MINMATCH = 4;
#pragma warning disable 162
		private const int SKIPSTRENGTH = NOTCOMPRESSIBLE_DETECTIONLEVEL > 2 ? NOTCOMPRESSIBLE_DETECTIONLEVEL : 2;
#pragma warning restore 162
		private const int COPYLENGTH = 8;
		private const int LASTLITERALS = 5;
		private const int MFLIMIT = COPYLENGTH + MINMATCH;
		private const int MINLENGTH = MFLIMIT + 1;
		private const int MAXD_LOG = 16;
		private const int MAXD = 1 << MAXD_LOG;
		private const int MAXD_MASK = MAXD - 1;
		private const int MAX_DISTANCE = (1 << MAXD_LOG) - 1;
		private const int ML_BITS = 4;
		private const int ML_MASK = (1 << ML_BITS) - 1;
		private const int RUN_BITS = 8 - ML_BITS;
		private const int RUN_MASK = (1 << RUN_BITS) - 1;
		private const int STEPSIZE_64 = 8;
		private const int STEPSIZE_32 = 4;

		private const int LZ4_64KLIMIT = (1 << 16) + (MFLIMIT - 1);

		private const int HASH_LOG = MEMORY_USAGE - 2;
		private const int HASH_TABLESIZE = 1 << HASH_LOG;
		private const int HASH_ADJUST = (MINMATCH * 8) - HASH_LOG;

		private const int HASH64K_LOG = HASH_LOG + 1;
		private const int HASH64K_TABLESIZE = 1 << HASH64K_LOG;
		private const int HASH64K_ADJUST = (MINMATCH * 8) - HASH64K_LOG;

		private const int HASHHC_LOG = MAXD_LOG - 1;
		private const int HASHHC_TABLESIZE = 1 << HASHHC_LOG;
		private const int HASHHC_ADJUST = (MINMATCH * 8) - HASHHC_LOG;
		//private const int HASHHC_MASK = HASHHC_TABLESIZE - 1;

		private static readonly int[] DECODER_TABLE_32 = new[] { 0, 3, 2, 3, 0, 0, 0, 0 };
		private static readonly int[] DECODER_TABLE_64 = new[] { 0, 0, 0, -1, 0, 1, 2, 3 };

		private static readonly int[] DEBRUIJN_TABLE_32 = new[] {
			0, 0, 3, 0, 3, 1, 3, 0, 3, 2, 2, 1, 3, 2, 0, 1,
			3, 3, 1, 2, 2, 2, 2, 0, 3, 1, 2, 0, 1, 0, 1, 1
		};

		private static readonly int[] DEBRUIJN_TABLE_64 = new[] {
			0, 0, 0, 0, 0, 1, 1, 2, 0, 3, 1, 3, 1, 4, 2, 7,
			0, 2, 3, 6, 1, 5, 3, 5, 1, 3, 4, 4, 2, 5, 6, 7,
			7, 0, 1, 2, 3, 3, 4, 6, 2, 6, 5, 5, 3, 4, 5, 6,
			7, 1, 2, 4, 6, 4, 4, 5, 7, 2, 6, 5, 7, 6, 7, 7
		};

		private const int MAX_NB_ATTEMPTS = 256;
		private const int OPTIMAL_ML = (ML_MASK - 1) + MINMATCH;

		#endregion

		#region public interface (common)

		/// <summary>Gets maximum the length of the output.</summary>
		/// <param name="inputLength">Length of the input.</param>
		/// <returns>Maximum number of bytes needed for compressed buffer.</returns>
		public static int MaximumOutputLength(int inputLength)
		{
			return inputLength + (inputLength / 255) + 16;
		}

		#endregion

		#region internal interface (common)

		internal static void CheckArguments(
			byte[] input, int inputOffset, ref int inputLength,
			byte[] output, int outputOffset, ref int outputLength)
		{
			if (inputLength < 0) inputLength = input.Length - inputOffset;
			if (inputLength == 0)
			{
				outputLength = 0;
				return;
			}

			if (input == null) throw new ArgumentNullException("input");
			if (inputOffset < 0 || inputOffset + inputLength > input.Length)
				throw new ArgumentException("inputOffset and inputLength are invalid for given input");

			if (outputLength < 0) outputLength = output.Length - outputOffset;
			if (output == null) throw new ArgumentNullException("output");
			if (outputOffset < 0 || outputOffset + outputLength > output.Length)
				throw new ArgumentException("outputOffset and outputLength are invalid for given output");
		}

		#endregion
	}
}

// ReSharper restore InconsistentNaming