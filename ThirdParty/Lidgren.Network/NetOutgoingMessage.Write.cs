/* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Text;

namespace Lidgren.Network
{
	public sealed partial class NetOutgoingMessage
	{
		private const int c_overAllocateAmount = 4;

		private static Dictionary<Type, MethodInfo> s_writeMethods;

		internal byte[] m_data;
		internal int m_bitLength;

		static NetOutgoingMessage()
		{
			s_writeMethods = new Dictionary<Type, MethodInfo>();
			MethodInfo[] methods = typeof(NetOutgoingMessage).GetMethods(BindingFlags.Instance | BindingFlags.Public);
			foreach (MethodInfo mi in methods)
			{
				if (mi.Name.Equals("Write", StringComparison.InvariantCulture))
				{
					ParameterInfo[] pis = mi.GetParameters();
					if (pis.Length == 1)
						s_writeMethods[pis[0].ParameterType] = mi;
				}
			}
		}

		/// <summary>
		/// Returns the internal data buffer, don't modify
		/// </summary>
		public byte[] PeekDataBuffer()
		{
			return m_data;
		}

		/// <summary>
		/// Gets or sets the length of the buffer in bytes
		/// </summary>
		public int LengthBytes
		{
			get { return ((m_bitLength + 7) >> 3); }
			set
			{
				m_bitLength = value * 8;
				InternalEnsureBufferSize(m_bitLength);
			}
		}

		/// <summary>
		/// Gets or sets the length of the buffer in bits
		/// </summary>
		public int LengthBits
		{
			get { return m_bitLength; }
			set
			{
				m_bitLength = value;
				InternalEnsureBufferSize(m_bitLength);
			}
		}

		/// <summary>
		/// Ensures the buffer can hold this number of bits
		/// </summary>
		public void EnsureBufferSize(int numberOfBits)
		{
			int byteLen = ((numberOfBits + 7) >> 3);
			if (m_data == null)
			{
				m_data = new byte[byteLen + c_overAllocateAmount];
				return;
			}
			if (m_data.Length < byteLen)
				Array.Resize<byte>(ref m_data, byteLen + c_overAllocateAmount);
			return;
		}

		/// <summary>
		/// Ensures the buffer can hold this number of bits
		/// </summary>
		public void InternalEnsureBufferSize(int numberOfBits)
		{
			int byteLen = ((numberOfBits + 7) >> 3);
			if (m_data == null)
			{
				m_data = new byte[byteLen];
				return;
			}
			if (m_data.Length < byteLen)
				Array.Resize<byte>(ref m_data, byteLen);
			return;
		}

		/// <summary>
		/// Writes a boolean value using 1 bit
		/// </summary>
		public void Write(bool value)
		{
			EnsureBufferSize(m_bitLength + 1);
			NetBitWriter.WriteByte((value ? (byte)1 : (byte)0), 1, m_data, m_bitLength);
			m_bitLength += 1;
		}

		/// <summary>
		/// Write a byte
		/// </summary>
		public void Write(byte source)
		{
			EnsureBufferSize(m_bitLength + 8);
			NetBitWriter.WriteByte(source, 8, m_data, m_bitLength);
			m_bitLength += 8;
		}

		/// <summary>
		/// Writes a signed byte
		/// </summary>
		[CLSCompliant(false)]
		public void Write(sbyte source)
		{
			EnsureBufferSize(m_bitLength + 8);
			NetBitWriter.WriteByte((byte)source, 8, m_data, m_bitLength);
			m_bitLength += 8;
		}

		/// <summary>
		/// Writes 1 to 8 bits of a byte
		/// </summary>
		public void Write(byte source, int numberOfBits)
		{
			NetException.Assert((numberOfBits > 0 && numberOfBits <= 8), "Write(byte, numberOfBits) can only write between 1 and 8 bits");
			EnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteByte(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		/// <summary>
		/// Writes all bytes in an array
		/// </summary>
		public void Write(byte[] source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			int bits = source.Length * 8;
			EnsureBufferSize(m_bitLength + bits);
			NetBitWriter.WriteBytes(source, 0, source.Length, m_data, m_bitLength);
			m_bitLength += bits;
		}

		/// <summary>
		/// Writes the specified number of bytes from an array
		/// </summary>
		public void Write(byte[] source, int offsetInBytes, int numberOfBytes)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			int bits = numberOfBytes * 8;
			EnsureBufferSize(m_bitLength + bits);
			NetBitWriter.WriteBytes(source, offsetInBytes, numberOfBytes, m_data, m_bitLength);
			m_bitLength += bits;
		}

		/// <summary>
		/// Writes an unsigned 16 bit integer
		/// </summary>
		/// <param name="source"></param>
		[CLSCompliant(false)]
		public void Write(UInt16 source)
		{
			EnsureBufferSize(m_bitLength + 16);
			NetBitWriter.WriteUInt32((uint)source, 16, m_data, m_bitLength);
			m_bitLength += 16;
		}

		/// <summary>
		/// Writes an unsigned integer using 1 to 16 bits
		/// </summary>
		[CLSCompliant(false)]
		public void Write(UInt16 source, int numberOfBits)
		{
			NetException.Assert((numberOfBits > 0 && numberOfBits <= 16), "Write(ushort, numberOfBits) can only write between 1 and 16 bits");
			EnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt32((uint)source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		/// <summary>
		/// Writes a signed 16 bit integer
		/// </summary>
		public void Write(Int16 source)
		{
			EnsureBufferSize(m_bitLength + 16);
			NetBitWriter.WriteUInt32((uint)source, 16, m_data, m_bitLength);
			m_bitLength += 16;
		}

#if UNSAFE
		/// <summary>
		/// Writes a 32 bit signed integer
		/// </summary>
		public unsafe void Write(Int32 source)
		{
			EnsureBufferSize(m_bitLength + 32);

			// can write fast?
			if (m_bitLength % 8 == 0)
			{
				fixed (byte* numRef = &Data[m_bitLength / 8])
				{
					*((int*)numRef) = source;
				}
			}
			else
			{
				NetBitWriter.WriteUInt32((UInt32)source, 32, Data, m_bitLength);
			}
			m_bitLength += 32;
		}
#else
		/// <summary>
		/// Writes a 32 bit signed integer
		/// </summary>
		public void Write(Int32 source)
		{
			EnsureBufferSize(m_bitLength + 32);
			NetBitWriter.WriteUInt32((UInt32)source, 32, m_data, m_bitLength);
			m_bitLength += 32;
		}
#endif

#if UNSAFE
		/// <summary>
		/// Writes a 32 bit unsigned integer
		/// </summary>
		public unsafe void Write(UInt32 source)
		{
			EnsureBufferSize(m_bitLength + 32);

			// can write fast?
			if (m_bitLength % 8 == 0)
			{
				fixed (byte* numRef = &Data[m_bitLength / 8])
				{
					*((uint*)numRef) = source;
				}
			}
			else
			{
				NetBitWriter.WriteUInt32(source, 32, Data, m_bitLength);
			}

			m_bitLength += 32;
		}
#else
		/// <summary>
		/// Writes a 32 bit unsigned integer
		/// </summary>
		[CLSCompliant(false)]
		public void Write(UInt32 source)
		{
			EnsureBufferSize(m_bitLength + 32);
			NetBitWriter.WriteUInt32(source, 32, m_data, m_bitLength);
			m_bitLength += 32;
		}
#endif

		/// <summary>
		/// Writes a 32 bit signed integer
		/// </summary>
		[CLSCompliant(false)]
		public void Write(UInt32 source, int numberOfBits)
		{
			NetException.Assert((numberOfBits > 0 && numberOfBits <= 32), "Write(uint, numberOfBits) can only write between 1 and 32 bits");
			EnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt32(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		/// <summary>
		/// Writes a signed integer using 1 to 32 bits
		/// </summary>
		public void Write(Int32 source, int numberOfBits)
		{
			NetException.Assert((numberOfBits > 0 && numberOfBits <= 32), "Write(int, numberOfBits) can only write between 1 and 32 bits");
			EnsureBufferSize(m_bitLength + numberOfBits);

			if (numberOfBits != 32)
			{
				// make first bit sign
				int signBit = 1 << (numberOfBits - 1);
				if (source < 0)
					source = (-source - 1) | signBit;
				else
					source &= (~signBit);
			}

			NetBitWriter.WriteUInt32((uint)source, numberOfBits, m_data, m_bitLength);

			m_bitLength += numberOfBits;
		}

		/// <summary>
		/// Writes a 64 bit unsigned integer
		/// </summary>
		[CLSCompliant(false)]
		public void Write(UInt64 source)
		{
			EnsureBufferSize(m_bitLength + 64);
			NetBitWriter.WriteUInt64(source, 64, m_data, m_bitLength);
			m_bitLength += 64;
		}

		/// <summary>
		/// Writes an unsigned integer using 1 to 64 bits
		/// </summary>
		[CLSCompliant(false)]
		public void Write(UInt64 source, int numberOfBits)
		{
			EnsureBufferSize(m_bitLength + numberOfBits);
			NetBitWriter.WriteUInt64(source, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		/// <summary>
		/// Writes a 64 bit signed integer
		/// </summary>
		public void Write(Int64 source)
		{
			EnsureBufferSize(m_bitLength + 64);
			ulong usource = (ulong)source;
			NetBitWriter.WriteUInt64(usource, 64, m_data, m_bitLength);
			m_bitLength += 64;
		}

		/// <summary>
		/// Writes a signed integer using 1 to 64 bits
		/// </summary>
		public void Write(Int64 source, int numberOfBits)
		{
			EnsureBufferSize(m_bitLength + numberOfBits);
			ulong usource = (ulong)source;
			NetBitWriter.WriteUInt64(usource, numberOfBits, m_data, m_bitLength);
			m_bitLength += numberOfBits;
		}

		//
		// Floating point
		//
#if UNSAFE
		/// <summary>
		/// Writes a 32 bit floating point value
		/// </summary>
		public unsafe void Write(float source)
		{
			uint val = *((uint*)&source);
#if BIGENDIAN
				val = NetUtility.SwapByteOrder(val);
#endif
			Write(val);
		}
#else
		/// <summary>
		/// Writes a 32 bit floating point value
		/// </summary>
		public void Write(float source)
		{
			byte[] val = BitConverter.GetBytes(source);
#if BIGENDIAN
			// swap byte order
			byte tmp = val[3];
			val[3] = val[0];
			val[0] = tmp;
			tmp = val[2];
			val[2] = val[1];
			val[1] = tmp;
#endif
			Write(val);
		}
#endif

#if UNSAFE
		/// <summary>
		/// Writes a 64 bit floating point value
		/// </summary>
		public unsafe void Write(double source)
		{
			ulong val = *((ulong*)&source);
#if BIGENDIAN
			val = NetUtility.SwapByteOrder(val);
#endif
			Write(val);
		}
#else
		/// <summary>
		/// Writes a 64 bit floating point value
		/// </summary>
		public void Write(double source)
		{
			byte[] val = BitConverter.GetBytes(source);
#if BIGENDIAN
			// 0 1 2 3   4 5 6 7

			// swap byte order
			byte tmp = val[7];
			val[7] = val[0];
			val[0] = tmp;

			tmp = val[6];
			val[6] = val[1];
			val[1] = tmp;

			tmp = val[5];
			val[5] = val[2];
			val[2] = tmp;

			tmp = val[4];
			val[4] = val[3];
			val[3] = tmp;
#endif
			Write(val);
		}
#endif

		//
		// Variable bits
		//

		/// <summary>
		/// Write Base128 encoded variable sized unsigned integer of up to 32 bits
		/// </summary>
		/// <returns>number of bytes written</returns>
		[CLSCompliant(false)]
		public int WriteVariableUInt32(uint value)
		{
			int retval = 1;
			uint num1 = (uint)value;
			while (num1 >= 0x80)
			{
				this.Write((byte)(num1 | 0x80));
				num1 = num1 >> 7;
				retval++;
			}
			this.Write((byte)num1);
			return retval;
		}

		/// <summary>
		/// Write Base128 encoded variable sized signed integer of up to 32 bits
		/// </summary>
		/// <returns>number of bytes written</returns>
		public int WriteVariableInt32(int value)
		{
			uint zigzag = (uint)(value << 1) ^ (uint)(value >> 31);
			return WriteVariableUInt32(zigzag);
		}

		/// <summary>
		/// Write Base128 encoded variable sized signed integer of up to 64 bits
		/// </summary>
		/// <returns>number of bytes written</returns>
		public int WriteVariableInt64(Int64 value)
		{
			ulong zigzag = (ulong)(value << 1) ^ (ulong)(value >> 63);
			return WriteVariableUInt64(zigzag);
		}

		/// <summary>
		/// Write Base128 encoded variable sized unsigned integer of up to 64 bits
		/// </summary>
		/// <returns>number of bytes written</returns>
		[CLSCompliant(false)]
		public int WriteVariableUInt64(UInt64 value)
		{
			int retval = 1;
			UInt64 num1 = (UInt64)value;
			while (num1 >= 0x80)
			{
				this.Write((byte)(num1 | 0x80));
				num1 = num1 >> 7;
				retval++;
			}
			this.Write((byte)num1);
			return retval;
		}

		/// <summary>
		/// Compress (lossy) a float in the range -1..1 using numberOfBits bits
		/// </summary>
		public void WriteSignedSingle(float value, int numberOfBits)
		{
			NetException.Assert(((value >= -1.0) && (value <= 1.0)), " WriteSignedSingle() must be passed a float in the range -1 to 1; val is " + value);

			float unit = (value + 1.0f) * 0.5f;
			int maxVal = (1 << numberOfBits) - 1;
			uint writeVal = (uint)(unit * (float)maxVal);

			Write(writeVal, numberOfBits);
		}

		/// <summary>
		/// Compress (lossy) a float in the range 0..1 using numberOfBits bits
		/// </summary>
		public void WriteUnitSingle(float value, int numberOfBits)
		{
			NetException.Assert(((value >= 0.0) && (value <= 1.0)), " WriteUnitSingle() must be passed a float in the range 0 to 1; val is " + value);

			int maxValue = (1 << numberOfBits) - 1;
			uint writeVal = (uint)(value * (float)maxValue);

			Write(writeVal, numberOfBits);
		}

		/// <summary>
		/// Compress a float within a specified range using a certain number of bits
		/// </summary>
		public void WriteRangedSingle(float value, float min, float max, int numberOfBits)
		{
			NetException.Assert(((value >= min) && (value <= max)), " WriteRangedSingle() must be passed a float in the range MIN to MAX; val is " + value);

			float range = max - min;
			float unit = ((value - min) / range);
			int maxVal = (1 << numberOfBits) - 1;
			Write((UInt32)((float)maxVal * unit), numberOfBits);
		}

		/// <summary>
		/// Writes an integer with the least amount of bits need for the specified range
		/// Returns number of bits written
		/// </summary>
		public int WriteRangedInteger(int min, int max, int value)
		{
			NetException.Assert(value >= min && value <= max, "Value not within min/max range!");

			uint range = (uint)(max - min);
			int numBits = NetUtility.BitsToHoldUInt(range);

			uint rvalue = (uint)(value - min);
			Write(rvalue, numBits);

			return numBits;
		}

		/// <summary>
		/// Write a string
		/// </summary>
		public void Write(string source)
		{
			if (string.IsNullOrEmpty(source))
			{
				EnsureBufferSize(m_bitLength + 8);
				WriteVariableUInt32(0);
				return;
			}

			byte[] bytes = Encoding.UTF8.GetBytes(source);
			EnsureBufferSize(m_bitLength + 8 + (bytes.Length * 8));
			WriteVariableUInt32((uint)bytes.Length);
			Write(bytes);
		}

		/// <summary>
		/// Writes an endpoint description
		/// </summary>
		public void Write(IPEndPoint endPoint)
		{
			byte[] bytes = endPoint.Address.GetAddressBytes();
			Write((byte)bytes.Length);
			Write(bytes);
			Write((ushort)endPoint.Port);
		}

		/// <summary>
		/// Writes the local time to a message; readable (and convertable to local time) by the remote host using ReadTime()
		/// </summary>
		public void WriteTime(double localTime, bool highPrecision)
		{
			if (highPrecision)
				Write(localTime);
			else
				Write((float)localTime);
		}

		/// <summary>
		/// Pads data with enough bits to reach a full byte. Decreases cpu usage for subsequent byte writes.
		/// </summary>
		public void WritePadBits()
		{
			m_bitLength = ((m_bitLength + 7) >> 3) * 8;
			EnsureBufferSize(m_bitLength);
		}

		/// <summary>
		/// Pads data with the specified number of bits.
		/// </summary>
		public void WritePadBits(int numberOfBits)
		{
			m_bitLength += numberOfBits;
			EnsureBufferSize(m_bitLength);
		}

		/// <summary>
		/// Append all the bits of message to this message
		/// </summary>
		public void Write(NetOutgoingMessage message)
		{
			EnsureBufferSize(m_bitLength + (message.LengthBytes * 8));

			Write(message.m_data, 0, message.LengthBytes);

			// did we write excessive bits?
			int bitsInLastByte = (message.m_bitLength % 8);
			if (bitsInLastByte != 0)
			{
				int excessBits = 8 - bitsInLastByte;
				m_bitLength -= excessBits;
			}
		}

		/// <summary>
		/// Append all the bits of message to this message
		/// </summary>
		public void Write(NetIncomingMessage message)
		{
			EnsureBufferSize(m_bitLength + (message.LengthBytes * 8));

			Write(message.m_data, 0, message.LengthBytes);

			// did we write excessive bits?
			int bitsInLastByte = (message.m_bitLength % 8);
			if (bitsInLastByte != 0)
			{
				int excessBits = 8 - bitsInLastByte;
				m_bitLength -= excessBits;
			}
		}
	}
}
