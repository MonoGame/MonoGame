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

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities.LZ4
{
	/// <summary>Unsafe LZ4 codec.</summary>
	internal static partial class LZ4Codec
	{
		/// <summary>Copies block of memory.</summary>
		/// <param name="src">The source.</param>
		/// <param name="dst">The destination.</param>
		/// <param name="len">The length (in bytes).</param>
		private static unsafe void BlockCopy(byte* src, byte* dst, int len)
		{
			while (len >= 8)
			{
				*(ulong*)dst = *(ulong*)src;
				dst += 8;
				src += 8;
				len -= 8;
			}
			if (len >= 4)
			{
				*(uint*)dst = *(uint*)src;
				dst += 4;
				src += 4;
				len -= 4;
			}
			if (len >= 2)
			{
				*(ushort*)dst = *(ushort*)src;
				dst += 2;
				src += 2;
				len -= 2;
			}
			if (len >= 1)
			{
				*dst = *src; /* d++; s++; l--; */
			}
		}

		/// <summary>Copies block of memory.</summary>
		/// <param name="dst">The destination.</param>
		/// <param name="len">The length (in bytes).</param>
		/// <param name="val">The value.</param>
		private static unsafe void BlockFill(byte* dst, int len, byte val)
		{
			if (len >= 8)
			{
				ulong mask = val;
				mask |= mask << 8;
				mask |= mask << 16;
				mask |= mask << 32;
				do
				{
					*(ulong*)dst = mask;
					dst += 8;
					len -= 8;
				} while (len >= 8);
			}

			while (len-- > 0) *dst++ = val;
		}

		#region Encode32

		/// <summary>Encodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="output">The output.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <returns>Number of bytes written.</returns>
		public static unsafe int Encode32(
			byte* input,
			byte* output,
			int inputLength,
			int outputLength)
		{
			if (inputLength < LZ4_64KLIMIT)
			{
				var hashTable = new ushort[HASH64K_TABLESIZE];
				fixed (ushort* h = &hashTable[0])
				{
					return LZ4_compress64kCtx_32(h, input, output, inputLength, outputLength);
				}
			}
			else
			{
				var hashTable = new byte*[HASH_TABLESIZE];
				fixed (byte** h = &hashTable[0])
				{
					return LZ4_compressCtx_32(h, input, output, inputLength, outputLength);
				}
			}
		}

		/// <summary>Encodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="output">The output.</param>
		/// <param name="outputOffset">The output offset.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <returns>Number of bytes written.</returns>
		public static unsafe int Encode32(
			byte[] input,
			int inputOffset,
			int inputLength,
			byte[] output,
			int outputOffset,
			int outputLength)
		{
			CheckArguments(
				input, inputOffset, ref inputLength,
				output, outputOffset, ref outputLength);

			if (outputLength == 0) return 0;

			fixed (byte* inputPtr = &input[inputOffset])
			fixed (byte* outputPtr = &output[outputOffset])
			{
				return Encode32(inputPtr, outputPtr, inputLength, outputLength);
			}
		}

		/// <summary>Encodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <returns>Compressed buffer.</returns>
		public static byte[] Encode32(byte[] input, int inputOffset, int inputLength)
		{
			if (inputLength < 0) inputLength = input.Length - inputOffset;

			if (input == null) throw new ArgumentNullException("input");
			if (inputOffset < 0 || inputOffset + inputLength > input.Length)
				throw new ArgumentException("inputOffset and inputLength are invalid for given input");

			var result = new byte[MaximumOutputLength(inputLength)];
			var length = Encode32(input, inputOffset, inputLength, result, 0, result.Length);

			if (length != result.Length)
			{
				if (length < 0)
					throw new InvalidOperationException("Compression has been corrupted");
				var buffer = new byte[length];
				Buffer.BlockCopy(result, 0, buffer, 0, length);
				return buffer;
			}
			return result;
		}

		#endregion

		#region Decode32

		/// <summary>Decodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="output">The output.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <param name="knownOutputLength">Set it to <c>true</c> if output length is known.</param>
		/// <returns>Number of bytes written.</returns>
		public static unsafe int Decode32(
			byte* input,
			int inputLength,
			byte* output,
			int outputLength,
			bool knownOutputLength)
		{
			if (knownOutputLength)
			{
				var length = LZ4_uncompress_32(input, output, outputLength);
				if (length != inputLength)
					throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
				return outputLength;
			}
			else
			{
				var length = LZ4_uncompress_unknownOutputSize_32(input, output, inputLength, outputLength);
				if (length < 0)
					throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
				return length;
			}
		}

		/// <summary>Decodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="output">The output.</param>
		/// <param name="outputOffset">The output offset.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <param name="knownOutputLength">Set it to <c>true</c> if output length is known.</param>
		/// <returns>Number of bytes written.</returns>
		public static unsafe int Decode32(
			byte[] input,
			int inputOffset,
			int inputLength,
			byte[] output,
			int outputOffset,
			int outputLength,
			bool knownOutputLength)
		{
			CheckArguments(
				input, inputOffset, ref inputLength,
				output, outputOffset, ref outputLength);

			if (outputLength == 0) return 0;

			fixed (byte* inputPtr = &input[inputOffset])
			fixed (byte* outputPtr = &output[outputOffset])
			{
				return Decode32(inputPtr, inputLength, outputPtr, outputLength, knownOutputLength);
			}
		}

		/// <summary>Decodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <returns>Decompressed buffer.</returns>
		public static byte[] Decode32(byte[] input, int inputOffset, int inputLength, int outputLength)
		{
			if (inputLength < 0) inputLength = input.Length - inputOffset;

			if (input == null) throw new ArgumentNullException("input");
			if (inputOffset < 0 || inputOffset + inputLength > input.Length)
				throw new ArgumentException("inputOffset and inputLength are invalid for given input");

			var result = new byte[outputLength];
			var length = Decode32(input, inputOffset, inputLength, result, 0, outputLength, true);
			if (length != outputLength)
				throw new ArgumentException("outputLength is not valid");
			return result;
		}

		#endregion

		#region Encode64

		/// <summary>Encodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="output">The output.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <returns>Number of bytes written.</returns>
		public static unsafe int Encode64(
			byte* input,
			byte* output,
			int inputLength,
			int outputLength)
		{
			if (inputLength < LZ4_64KLIMIT)
			{
				var hashTable = new ushort[HASH64K_TABLESIZE];
				fixed (ushort* h = &hashTable[0])
				{
					return LZ4_compress64kCtx_64(h, input, output, inputLength, outputLength);
				}
			}
			else
			{
				var hashTable = new uint[HASH_TABLESIZE];
				fixed (uint* h = &hashTable[0])
				{
					return LZ4_compressCtx_64(h, input, output, inputLength, outputLength);
				}
			}
		}

		/// <summary>Encodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="output">The output.</param>
		/// <param name="outputOffset">The output offset.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <returns>Number of bytes written.</returns>
		public static unsafe int Encode64(
			byte[] input,
			int inputOffset,
			int inputLength,
			byte[] output,
			int outputOffset,
			int outputLength)
		{
			CheckArguments(
				input, inputOffset, ref inputLength,
				output, outputOffset, ref outputLength);

			if (outputLength == 0) return 0;

			fixed (byte* inputPtr = &input[inputOffset])
			fixed (byte* outputPtr = &output[outputOffset])
			{
				return Encode64(inputPtr, outputPtr, inputLength, outputLength);
			}
		}

		/// <summary>Encodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <returns>Compressed buffer.</returns>
		public static byte[] Encode64(byte[] input, int inputOffset, int inputLength)
		{
			if (inputLength < 0) inputLength = input.Length - inputOffset;

			if (input == null) throw new ArgumentNullException("input");
			if (inputOffset < 0 || inputOffset + inputLength > input.Length)
				throw new ArgumentException("inputOffset and inputLength are invalid for given input");

			var result = new byte[MaximumOutputLength(inputLength)];
			var length = Encode64(input, inputOffset, inputLength, result, 0, result.Length);

			if (length != result.Length)
			{
				if (length < 0)
					throw new InvalidOperationException("Compression has been corrupted");
				var buffer = new byte[length];
				Buffer.BlockCopy(result, 0, buffer, 0, length);
				return buffer;
			}
			return result;
		}

		#endregion

		#region Decode64

		/// <summary>Decodes the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="output">The output.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <param name="knownOutputLength">Set it to <c>true</c> if output length is known.</param>
		/// <returns>Number of bytes written.</returns>
		public static unsafe int Decode64(
			byte* input,
			int inputLength,
			byte* output,
			int outputLength,
			bool knownOutputLength)
		{
			if (knownOutputLength)
			{
				var length = LZ4_uncompress_64(input, output, outputLength);
				if (length != inputLength)
					throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
				return outputLength;
			}
			else
			{
				var length = LZ4_uncompress_unknownOutputSize_64(input, output, inputLength, outputLength);
				if (length < 0)
					throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
				return length;
			}
		}

		/// <summary>Decode64s the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="output">The output.</param>
		/// <param name="outputOffset">The output offset.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <param name="knownOutputLength">Set it to <c>true</c> if output length is known.</param>
		/// <returns>Number of bytes written.</returns>
		public static unsafe int Decode64(
			byte[] input,
			int inputOffset,
			int inputLength,
			byte[] output,
			int outputOffset,
			int outputLength,
			bool knownOutputLength)
		{
			CheckArguments(
				input, inputOffset, ref inputLength,
				output, outputOffset, ref outputLength);

			if (outputLength == 0) return 0;

			fixed (byte* inputPtr = &input[inputOffset])
			fixed (byte* outputPtr = &output[outputOffset])
			{
				return Decode64(inputPtr, inputLength, outputPtr, outputLength, knownOutputLength);
			}
		}

		/// <summary>Decode64s the specified input.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <returns>Decompressed buffer.</returns>
		public static byte[] Decode64(byte[] input, int inputOffset, int inputLength, int outputLength)
		{
			if (inputLength < 0) inputLength = input.Length - inputOffset;

			if (input == null) throw new ArgumentNullException("input");
			if (inputOffset < 0 || inputOffset + inputLength > input.Length)
				throw new ArgumentException("inputOffset and inputLength are invalid for given input");

			var result = new byte[outputLength];
			var length = Decode64(input, inputOffset, inputLength, result, 0, outputLength, true);
			if (length != outputLength)
				throw new ArgumentException("outputLength is not valid");
			return result;
		}

		#endregion

		#region HC utilities

		// ReSharper disable InconsistentNaming

		private unsafe class LZ4HC_Data_Structure
		{
			public byte* src_base;
			public byte* nextToUpdate;
			public int[] hashTable;
			public ushort[] chainTable;
		};

		// ReSharper restore InconsistentNaming


		private static unsafe LZ4HC_Data_Structure LZ4HC_Create(byte* src)
		{
			var hc4 = new LZ4HC_Data_Structure {
				hashTable = new int[HASHHC_TABLESIZE],
				chainTable = new ushort[MAXD]
			};

			fixed (ushort* ct = &hc4.chainTable[0])
			{
				BlockFill((byte*)ct, MAXD * sizeof(ushort), 0xFF);
			}

			hc4.src_base = src;
			hc4.nextToUpdate = src + 1;

			return hc4;
		}

		#endregion

		#region Encode32HC

		private static unsafe int LZ4_compressHC_32(byte* input, byte* output, int inputLength, int outputLength)
		{
			return LZ4_compressHCCtx_32(LZ4HC_Create(input), input, output, inputLength, outputLength);
		}

		/// <summary>Encodes the specified input using HC codec.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="output">The output.</param>
		/// <param name="outputOffset">The output offset.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <returns>Number of bytes written. NOTE: when output buffer is too small it returns negative value.</returns>
		public static unsafe int Encode32HC(
			byte[] input,
			int inputOffset,
			int inputLength,
			byte[] output,
			int outputOffset,
			int outputLength)
		{
			if (inputLength == 0) return 0;

			CheckArguments(
				input, inputOffset, ref inputLength,
				output, outputOffset, ref outputLength);

			fixed (byte* inputPtr = &input[inputOffset])
			fixed (byte* outputPtr = &output[outputOffset])
			{
				var length = LZ4_compressHC_32(inputPtr, outputPtr, inputLength, outputLength);
				return length <= 0 ? -1 : length;
			}
		}

		/// <summary>Encodes the specified input using HC codec.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <returns>Buffer with compressed data (NOTE: it can be bigger than input).</returns>
		public static byte[] Encode32HC(
			byte[] input, int inputOffset, int inputLength)
		{
			if (inputLength == 0) return new byte[0];
			var outputLength = MaximumOutputLength(inputLength);
			var result = new byte[outputLength];
			var length = Encode32HC(input, inputOffset, inputLength, result, 0, outputLength);

			if (length < 0)
				throw new ArgumentException("Provided data seems to be corrupted.");

			if (length != outputLength)
			{
				var buffer = new byte[length];
				Buffer.BlockCopy(result, 0, buffer, 0, length);
				result = buffer;
			}

			return result;
		}

		#endregion

		#region Encode64HC

		private static unsafe int LZ4_compressHC_64(byte* input, byte* output, int inputLength, int outputLength)
		{
			return LZ4_compressHCCtx_64(LZ4HC_Create(input), input, output, inputLength, outputLength);
		}

		/// <summary>Encodes the specified input using HC codec.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <param name="output">The output.</param>
		/// <param name="outputOffset">The output offset.</param>
		/// <param name="outputLength">Length of the output.</param>
		/// <returns>Number of bytes written. NOTE: when output buffer is too small it returns negative value.</returns>
		public static unsafe int Encode64HC(
			byte[] input,
			int inputOffset,
			int inputLength,
			byte[] output,
			int outputOffset,
			int outputLength)
		{
			if (inputLength == 0) return 0;

			CheckArguments(
				input, inputOffset, ref inputLength,
				output, outputOffset, ref outputLength);

			fixed (byte* inputPtr = &input[inputOffset])
			fixed (byte* outputPtr = &output[outputOffset])
			{
				var length = LZ4_compressHC_64(inputPtr, outputPtr, inputLength, outputLength);
				return length <= 0 ? -1 : length;
			}
		}

		/// <summary>Encodes the specified input using HC codec.</summary>
		/// <param name="input">The input.</param>
		/// <param name="inputOffset">The input offset.</param>
		/// <param name="inputLength">Length of the input.</param>
		/// <returns>Buffer with compressed data (NOTE: it can be bigger than input).</returns>
		public static byte[] Encode64HC(
			byte[] input, int inputOffset, int inputLength)
		{
			if (inputLength == 0) return new byte[0];
			var outputLength = MaximumOutputLength(inputLength);
			var result = new byte[outputLength];
			var length = Encode64HC(input, inputOffset, inputLength, result, 0, outputLength);

			if (length < 0)
				throw new ArgumentException("Provided data seems to be corrupted.");

			if (length != outputLength)
			{
				var buffer = new byte[length];
				Buffer.BlockCopy(result, 0, buffer, 0, length);
				result = buffer;
			}

			return result;
		}

		#endregion
	}
}
