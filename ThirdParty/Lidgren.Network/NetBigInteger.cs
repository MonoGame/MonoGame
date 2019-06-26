using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace Lidgren.Network
{
	/// <summary>
	/// Big integer class based on BouncyCastle (http://www.bouncycastle.org) big integer code
	/// </summary>
	internal class NetBigInteger
	{
		private const long IMASK = 0xffffffffL;
		private const ulong UIMASK = (ulong)IMASK;

		private static readonly int[] ZeroMagnitude = new int[0];
		private static readonly byte[] ZeroEncoding = new byte[0];

		public static readonly NetBigInteger Zero = new NetBigInteger(0, ZeroMagnitude, false);
		public static readonly NetBigInteger One = createUValueOf(1);
		public static readonly NetBigInteger Two = createUValueOf(2);
		public static readonly NetBigInteger Three = createUValueOf(3);
		public static readonly NetBigInteger Ten = createUValueOf(10);

		private const int chunk2 = 1;
		private static readonly NetBigInteger radix2 = ValueOf(2);
		private static readonly NetBigInteger radix2E = radix2.Pow(chunk2);

		private const int chunk10 = 19;
		private static readonly NetBigInteger radix10 = ValueOf(10);
		private static readonly NetBigInteger radix10E = radix10.Pow(chunk10);

		private const int chunk16 = 16;
		private static readonly NetBigInteger radix16 = ValueOf(16);
		private static readonly NetBigInteger radix16E = radix16.Pow(chunk16);

		private const int BitsPerByte = 8;
		private const int BitsPerInt = 32;
		private const int BytesPerInt = 4;

		private int m_sign; // -1 means -ve; +1 means +ve; 0 means 0;
		private int[] m_magnitude; // array of ints with [0] being the most significant
		private int m_numBits = -1; // cache BitCount() value
		private int m_numBitLength = -1; // cache calcBitLength() value
		private long m_quote = -1L; // -m^(-1) mod b, b = 2^32 (see Montgomery mult.)

		private static int GetByteLength(
			int nBits)
		{
			return (nBits + BitsPerByte - 1) / BitsPerByte;
		}

		private NetBigInteger()
		{
		}

		private NetBigInteger(
			int signum,
			int[] mag,
			bool checkMag)
		{
			if (checkMag)
			{
				int i = 0;
				while (i < mag.Length && mag[i] == 0)
				{
					++i;
				}

				if (i == mag.Length)
				{
					//					sign = 0;
					m_magnitude = ZeroMagnitude;
				}
				else
				{
					m_sign = signum;

					if (i == 0)
					{
						m_magnitude = mag;
					}
					else
					{
						// strip leading 0 words
						m_magnitude = new int[mag.Length - i];
						Array.Copy(mag, i, m_magnitude, 0, m_magnitude.Length);
					}
				}
			}
			else
			{
				m_sign = signum;
				m_magnitude = mag;
			}
		}

		public NetBigInteger(
			string value)
			: this(value, 10)
		{
		}

		public NetBigInteger(
			string str,
			int radix)
		{
			if (str.Length == 0)
				throw new FormatException("Zero length BigInteger");

			NumberStyles style;
			int chunk;
			NetBigInteger r;
			NetBigInteger rE;

			switch (radix)
			{
				case 2:
					// Is there anyway to restrict to binary digits?
					style = NumberStyles.Integer;
					chunk = chunk2;
					r = radix2;
					rE = radix2E;
					break;
				case 10:
					// This style seems to handle spaces and minus sign already (our processing redundant?)
					style = NumberStyles.Integer;
					chunk = chunk10;
					r = radix10;
					rE = radix10E;
					break;
				case 16:
					// TODO Should this be HexNumber?
					style = NumberStyles.AllowHexSpecifier;
					chunk = chunk16;
					r = radix16;
					rE = radix16E;
					break;
				default:
					throw new FormatException("Only bases 2, 10, or 16 allowed");
			}


			int index = 0;
			m_sign = 1;

			if (str[0] == '-')
			{
				if (str.Length == 1)
					throw new FormatException("Zero length BigInteger");

				m_sign = -1;
				index = 1;
			}

			// strip leading zeros from the string str
			while (index < str.Length && Int32.Parse(str[index].ToString(), style) == 0)
			{
				index++;
			}

			if (index >= str.Length)
			{
				// zero value - we're done
				m_sign = 0;
				m_magnitude = ZeroMagnitude;
				return;
			}

			//////
			// could we work out the max number of ints required to store
			// str.Length digits in the given base, then allocate that
			// storage in one hit?, then Generate the magnitude in one hit too?
			//////

			NetBigInteger b = Zero;


			int next = index + chunk;

			if (next <= str.Length)
			{
				do
				{
					string s = str.Substring(index, chunk);
					ulong i = ulong.Parse(s, style);
					NetBigInteger bi = createUValueOf(i);

					switch (radix)
					{
						case 2:
							if (i > 1)
								throw new FormatException("Bad character in radix 2 string: " + s);

							b = b.ShiftLeft(1);
							break;
						case 16:
							b = b.ShiftLeft(64);
							break;
						default:
							b = b.Multiply(rE);
							break;
					}

					b = b.Add(bi);

					index = next;
					next += chunk;
				}
				while (next <= str.Length);
			}

			if (index < str.Length)
			{
				string s = str.Substring(index);
				ulong i = ulong.Parse(s, style);
				NetBigInteger bi = createUValueOf(i);

				if (b.m_sign > 0)
				{
					if (radix == 2)
					{
						// NB: Can't reach here since we are parsing one char at a time
						Debug.Assert(false);
					}
					else if (radix == 16)
					{
						b = b.ShiftLeft(s.Length << 2);
					}
					else
					{
						b = b.Multiply(r.Pow(s.Length));
					}

					b = b.Add(bi);
				}
				else
				{
					b = bi;
				}
			}

			// Note: This is the previous (slower) algorithm
			//			while (index < value.Length)
			//            {
			//				char c = value[index];
			//				string s = c.ToString();
			//				int i = Int32.Parse(s, style);
			//
			//                b = b.Multiply(r).Add(ValueOf(i));
			//                index++;
			//            }

			m_magnitude = b.m_magnitude;
		}

		public NetBigInteger(
			byte[] bytes)
			: this(bytes, 0, bytes.Length)
		{
		}

		public NetBigInteger(
			byte[] bytes,
			int offset,
			int length)
		{
			if (length == 0)
				throw new FormatException("Zero length BigInteger");
			if ((sbyte)bytes[offset] < 0)
			{
				m_sign = -1;

				int end = offset + length;

				int iBval;
				// strip leading sign bytes
				for (iBval = offset; iBval < end && ((sbyte)bytes[iBval] == -1); iBval++)
				{
				}

				if (iBval >= end)
				{
					m_magnitude = One.m_magnitude;
				}
				else
				{
					int numBytes = end - iBval;
					byte[] inverse = new byte[numBytes];

					int index = 0;
					while (index < numBytes)
					{
						inverse[index++] = (byte)~bytes[iBval++];
					}

					Debug.Assert(iBval == end);

					while (inverse[--index] == byte.MaxValue)
					{
						inverse[index] = byte.MinValue;
					}

					inverse[index]++;

					m_magnitude = MakeMagnitude(inverse, 0, inverse.Length);
				}
			}
			else
			{
				// strip leading zero bytes and return magnitude bytes
				m_magnitude = MakeMagnitude(bytes, offset, length);
				m_sign = m_magnitude.Length > 0 ? 1 : 0;
			}
		}

		private static int[] MakeMagnitude(
			byte[] bytes,
			int offset,
			int length)
		{
			int end = offset + length;

			// strip leading zeros
			int firstSignificant;
			for (firstSignificant = offset; firstSignificant < end
				&& bytes[firstSignificant] == 0; firstSignificant++)
			{
			}

			if (firstSignificant >= end)
			{
				return ZeroMagnitude;
			}

			int nInts = (end - firstSignificant + 3) / BytesPerInt;
			int bCount = (end - firstSignificant) % BytesPerInt;
			if (bCount == 0)
			{
				bCount = BytesPerInt;
			}

			if (nInts < 1)
			{
				return ZeroMagnitude;
			}

			int[] mag = new int[nInts];

			int v = 0;
			int magnitudeIndex = 0;
			for (int i = firstSignificant; i < end; ++i)
			{
				v <<= 8;
				v |= bytes[i] & 0xff;
				bCount--;
				if (bCount <= 0)
				{
					mag[magnitudeIndex] = v;
					magnitudeIndex++;
					bCount = BytesPerInt;
					v = 0;
				}
			}

			if (magnitudeIndex < mag.Length)
			{
				mag[magnitudeIndex] = v;
			}

			return mag;
		}

		public NetBigInteger(
			int sign,
			byte[] bytes)
			: this(sign, bytes, 0, bytes.Length)
		{
		}

		public NetBigInteger(
			int sign,
			byte[] bytes,
			int offset,
			int length)
		{
			if (sign < -1 || sign > 1)
				throw new FormatException("Invalid sign value");

			if (sign == 0)
			{
				//sign = 0;
				m_magnitude = ZeroMagnitude;
			}
			else
			{
				// copy bytes
				m_magnitude = MakeMagnitude(bytes, offset, length);
				m_sign = m_magnitude.Length < 1 ? 0 : sign;
			}
		}

		public NetBigInteger Abs()
		{
			return m_sign >= 0 ? this : Negate();
		}

		// return a = a + b - b preserved.
		private static int[] AddMagnitudes(
			int[] a,
			int[] b)
		{
			int tI = a.Length - 1;
			int vI = b.Length - 1;
			long m = 0;

			while (vI >= 0)
			{
				m += ((long)(uint)a[tI] + (long)(uint)b[vI--]);
				a[tI--] = (int)m;
				m = (long)((ulong)m >> 32);
			}

			if (m != 0)
			{
				while (tI >= 0 && ++a[tI--] == 0)
				{
				}
			}

			return a;
		}

		public NetBigInteger Add(
			NetBigInteger value)
		{
			if (m_sign == 0)
				return value;

			if (m_sign != value.m_sign)
			{
				if (value.m_sign == 0)
					return this;

				if (value.m_sign < 0)
					return Subtract(value.Negate());

				return value.Subtract(Negate());
			}

			return AddToMagnitude(value.m_magnitude);
		}

		private NetBigInteger AddToMagnitude(
			int[] magToAdd)
		{
			int[] big, small;
			if (m_magnitude.Length < magToAdd.Length)
			{
				big = magToAdd;
				small = m_magnitude;
			}
			else
			{
				big = m_magnitude;
				small = magToAdd;
			}

			// Conservatively avoid over-allocation when no overflow possible
			uint limit = uint.MaxValue;
			if (big.Length == small.Length)
				limit -= (uint)small[0];

			bool possibleOverflow = (uint)big[0] >= limit;

			int[] bigCopy;
			if (possibleOverflow)
			{
				bigCopy = new int[big.Length + 1];
				big.CopyTo(bigCopy, 1);
			}
			else
			{
				bigCopy = (int[])big.Clone();
			}

			bigCopy = AddMagnitudes(bigCopy, small);

			return new NetBigInteger(m_sign, bigCopy, possibleOverflow);
		}

		public NetBigInteger And(
			NetBigInteger value)
		{
			if (m_sign == 0 || value.m_sign == 0)
			{
				return Zero;
			}

			int[] aMag = m_sign > 0
				? m_magnitude
				: Add(One).m_magnitude;

			int[] bMag = value.m_sign > 0
				? value.m_magnitude
				: value.Add(One).m_magnitude;

			bool resultNeg = m_sign < 0 && value.m_sign < 0;
			int resultLength = System.Math.Max(aMag.Length, bMag.Length);
			int[] resultMag = new int[resultLength];

			int aStart = resultMag.Length - aMag.Length;
			int bStart = resultMag.Length - bMag.Length;

			for (int i = 0; i < resultMag.Length; ++i)
			{
				int aWord = i >= aStart ? aMag[i - aStart] : 0;
				int bWord = i >= bStart ? bMag[i - bStart] : 0;

				if (m_sign < 0)
				{
					aWord = ~aWord;
				}

				if (value.m_sign < 0)
				{
					bWord = ~bWord;
				}

				resultMag[i] = aWord & bWord;

				if (resultNeg)
				{
					resultMag[i] = ~resultMag[i];
				}
			}

			NetBigInteger result = new NetBigInteger(1, resultMag, true);

			if (resultNeg)
			{
				result = result.Not();
			}

			return result;
		}
	
		private int calcBitLength(
			int indx,
			int[] mag)
		{
			for (; ; )
			{
				if (indx >= mag.Length)
					return 0;

				if (mag[indx] != 0)
					break;

				++indx;
			}

			// bit length for everything after the first int
			int bitLength = 32 * ((mag.Length - indx) - 1);

			// and determine bitlength of first int
			int firstMag = mag[indx];
			bitLength += BitLen(firstMag);

			// Check for negative powers of two
			if (m_sign < 0 && ((firstMag & -firstMag) == firstMag))
			{
				do
				{
					if (++indx >= mag.Length)
					{
						--bitLength;
						break;
					}
				}
				while (mag[indx] == 0);
			}

			return bitLength;
		}

		public int BitLength
		{
			get
			{
				if (m_numBitLength == -1)
				{
					m_numBitLength = m_sign == 0
						? 0
						: calcBitLength(0, m_magnitude);
				}

				return m_numBitLength;
			}
		}

		//
		// BitLen(value) is the number of bits in value.
		//
		private static int BitLen(
			int w)
		{
			// Binary search - decision tree (5 tests, rarely 6)
			return (w < 1 << 15 ? (w < 1 << 7
				? (w < 1 << 3 ? (w < 1 << 1
				? (w < 1 << 0 ? (w < 0 ? 32 : 0) : 1)
				: (w < 1 << 2 ? 2 : 3)) : (w < 1 << 5
				? (w < 1 << 4 ? 4 : 5)
				: (w < 1 << 6 ? 6 : 7)))
				: (w < 1 << 11
				? (w < 1 << 9 ? (w < 1 << 8 ? 8 : 9) : (w < 1 << 10 ? 10 : 11))
				: (w < 1 << 13 ? (w < 1 << 12 ? 12 : 13) : (w < 1 << 14 ? 14 : 15)))) : (w < 1 << 23 ? (w < 1 << 19
				? (w < 1 << 17 ? (w < 1 << 16 ? 16 : 17) : (w < 1 << 18 ? 18 : 19))
				: (w < 1 << 21 ? (w < 1 << 20 ? 20 : 21) : (w < 1 << 22 ? 22 : 23))) : (w < 1 << 27
				? (w < 1 << 25 ? (w < 1 << 24 ? 24 : 25) : (w < 1 << 26 ? 26 : 27))
				: (w < 1 << 29 ? (w < 1 << 28 ? 28 : 29) : (w < 1 << 30 ? 30 : 31)))));
		}

		private bool QuickPow2Check()
		{
			return m_sign > 0 && m_numBits == 1;
		}

		public int CompareTo(
			object obj)
		{
			return CompareTo((NetBigInteger)obj);
		}

		
		// unsigned comparison on two arrays - note the arrays may
		// start with leading zeros.
		private static int CompareTo(
			int xIndx,
			int[] x,
			int yIndx,
			int[] y)
		{
			while (xIndx != x.Length && x[xIndx] == 0)
			{
				xIndx++;
			}

			while (yIndx != y.Length && y[yIndx] == 0)
			{
				yIndx++;
			}

			return CompareNoLeadingZeroes(xIndx, x, yIndx, y);
		}

		private static int CompareNoLeadingZeroes(
			int xIndx,
			int[] x,
			int yIndx,
			int[] y)
		{
			int diff = (x.Length - y.Length) - (xIndx - yIndx);

			if (diff != 0)
			{
				return diff < 0 ? -1 : 1;
			}

			// lengths of magnitudes the same, test the magnitude values

			while (xIndx < x.Length)
			{
				uint v1 = (uint)x[xIndx++];
				uint v2 = (uint)y[yIndx++];

				if (v1 != v2)
					return v1 < v2 ? -1 : 1;
			}

			return 0;
		}

		public int CompareTo(
			NetBigInteger value)
		{
			return m_sign < value.m_sign ? -1
				: m_sign > value.m_sign ? 1
				: m_sign == 0 ? 0
				: m_sign * CompareNoLeadingZeroes(0, m_magnitude, 0, value.m_magnitude);
		}

		// return z = x / y - done in place (z value preserved, x contains the remainder)
		private int[] Divide(
			int[] x,
			int[] y)
		{
			int xStart = 0;
			while (xStart < x.Length && x[xStart] == 0)
			{
				++xStart;
			}

			int yStart = 0;
			while (yStart < y.Length && y[yStart] == 0)
			{
				++yStart;
			}

			Debug.Assert(yStart < y.Length);

			int xyCmp = CompareNoLeadingZeroes(xStart, x, yStart, y);
			int[] count;

			if (xyCmp > 0)
			{
				int yBitLength = calcBitLength(yStart, y);
				int xBitLength = calcBitLength(xStart, x);
				int shift = xBitLength - yBitLength;

				int[] iCount;
				int iCountStart = 0;

				int[] c;
				int cStart = 0;
				int cBitLength = yBitLength;
				if (shift > 0)
				{
					//					iCount = ShiftLeft(One.magnitude, shift);
					iCount = new int[(shift >> 5) + 1];
					iCount[0] = 1 << (shift % 32);

					c = ShiftLeft(y, shift);
					cBitLength += shift;
				}
				else
				{
					iCount = new int[] { 1 };

					int len = y.Length - yStart;
					c = new int[len];
					Array.Copy(y, yStart, c, 0, len);
				}

				count = new int[iCount.Length];

				for (; ; )
				{
					if (cBitLength < xBitLength
						|| CompareNoLeadingZeroes(xStart, x, cStart, c) >= 0)
					{
						Subtract(xStart, x, cStart, c);
						AddMagnitudes(count, iCount);

						while (x[xStart] == 0)
						{
							if (++xStart == x.Length)
								return count;
						}

						//xBitLength = calcBitLength(xStart, x);
						xBitLength = 32 * (x.Length - xStart - 1) + BitLen(x[xStart]);

						if (xBitLength <= yBitLength)
						{
							if (xBitLength < yBitLength)
								return count;

							xyCmp = CompareNoLeadingZeroes(xStart, x, yStart, y);

							if (xyCmp <= 0)
								break;
						}
					}

					shift = cBitLength - xBitLength;

					// NB: The case where c[cStart] is 1-bit is harmless
					if (shift == 1)
					{
						uint firstC = (uint)c[cStart] >> 1;
						uint firstX = (uint)x[xStart];
						if (firstC > firstX)
							++shift;
					}

					if (shift < 2)
					{
						c = ShiftRightOneInPlace(cStart, c);
						--cBitLength;
						iCount = ShiftRightOneInPlace(iCountStart, iCount);
					}
					else
					{
						c = ShiftRightInPlace(cStart, c, shift);
						cBitLength -= shift;
						iCount = ShiftRightInPlace(iCountStart, iCount, shift);
					}

					//cStart = c.Length - ((cBitLength + 31) / 32);
					while (c[cStart] == 0)
					{
						++cStart;
					}

					while (iCount[iCountStart] == 0)
					{
						++iCountStart;
					}
				}
			}
			else
			{
				count = new int[1];
			}

			if (xyCmp == 0)
			{
				AddMagnitudes(count, One.m_magnitude);
				Array.Clear(x, xStart, x.Length - xStart);
			}

			return count;
		}

		public NetBigInteger Divide(
			NetBigInteger val)
		{
			if (val.m_sign == 0)
				throw new ArithmeticException("Division by zero error");

			if (m_sign == 0)
				return Zero;

			if (val.QuickPow2Check()) // val is power of two
			{
				NetBigInteger result = Abs().ShiftRight(val.Abs().BitLength - 1);
				return val.m_sign == m_sign ? result : result.Negate();
			}

			int[] mag = (int[])m_magnitude.Clone();

			return new NetBigInteger(m_sign * val.m_sign, Divide(mag, val.m_magnitude), true);
		}

		public NetBigInteger[] DivideAndRemainder(
			NetBigInteger val)
		{
			if (val.m_sign == 0)
				throw new ArithmeticException("Division by zero error");

			NetBigInteger[] biggies = new NetBigInteger[2];

			if (m_sign == 0)
			{
				biggies[0] = Zero;
				biggies[1] = Zero;
			}
			else if (val.QuickPow2Check()) // val is power of two
			{
				int e = val.Abs().BitLength - 1;
				NetBigInteger quotient = Abs().ShiftRight(e);
				int[] remainder = LastNBits(e);

				biggies[0] = val.m_sign == m_sign ? quotient : quotient.Negate();
				biggies[1] = new NetBigInteger(m_sign, remainder, true);
			}
			else
			{
				int[] remainder = (int[])m_magnitude.Clone();
				int[] quotient = Divide(remainder, val.m_magnitude);

				biggies[0] = new NetBigInteger(m_sign * val.m_sign, quotient, true);
				biggies[1] = new NetBigInteger(m_sign, remainder, true);
			}

			return biggies;
		}

		public override bool Equals(
			object obj)
		{
			if (obj == this)
				return true;

			NetBigInteger biggie = obj as NetBigInteger;
			if (biggie == null)
				return false;

			if (biggie.m_sign != m_sign || biggie.m_magnitude.Length != m_magnitude.Length)
				return false;

			for (int i = 0; i < m_magnitude.Length; i++)
			{
				if (biggie.m_magnitude[i] != m_magnitude[i])
				{
					return false;
				}
			}

			return true;
		}

		public NetBigInteger Gcd(
			NetBigInteger value)
		{
			if (value.m_sign == 0)
				return Abs();

			if (m_sign == 0)
				return value.Abs();

			NetBigInteger r;
			NetBigInteger u = this;
			NetBigInteger v = value;

			while (v.m_sign != 0)
			{
				r = u.Mod(v);
				u = v;
				v = r;
			}

			return u;
		}

		public override int GetHashCode()
		{
			int hc = m_magnitude.Length;
			if (m_magnitude.Length > 0)
			{
				hc ^= m_magnitude[0];

				if (m_magnitude.Length > 1)
				{
					hc ^= m_magnitude[m_magnitude.Length - 1];
				}
			}

			return m_sign < 0 ? ~hc : hc;
		}

		private NetBigInteger Inc()
		{
			if (m_sign == 0)
				return One;

			if (m_sign < 0)
				return new NetBigInteger(-1, doSubBigLil(m_magnitude, One.m_magnitude), true);

			return AddToMagnitude(One.m_magnitude);
		}

		public int IntValue
		{
			get
			{
				return m_sign == 0 ? 0
					: m_sign > 0 ? m_magnitude[m_magnitude.Length - 1]
					: -m_magnitude[m_magnitude.Length - 1];
			}
		}
	
		public NetBigInteger Max(
			NetBigInteger value)
		{
			return CompareTo(value) > 0 ? this : value;
		}

		public NetBigInteger Min(
			NetBigInteger value)
		{
			return CompareTo(value) < 0 ? this : value;
		}

		public NetBigInteger Mod(
			NetBigInteger m)
		{
			if (m.m_sign < 1)
				throw new ArithmeticException("Modulus must be positive");

			NetBigInteger biggie = Remainder(m);

			return (biggie.m_sign >= 0 ? biggie : biggie.Add(m));
		}

		public NetBigInteger ModInverse(
			NetBigInteger m)
		{
			if (m.m_sign < 1)
				throw new ArithmeticException("Modulus must be positive");

			NetBigInteger x = new NetBigInteger();
			NetBigInteger gcd = ExtEuclid(this, m, x, null);

			if (!gcd.Equals(One))
				throw new ArithmeticException("Numbers not relatively prime.");

			if (x.m_sign < 0)
			{
				x.m_sign = 1;
				//x = m.Subtract(x);
				x.m_magnitude = doSubBigLil(m.m_magnitude, x.m_magnitude);
			}

			return x;
		}

		private static NetBigInteger ExtEuclid(
			NetBigInteger a,
			NetBigInteger b,
			NetBigInteger u1Out,
			NetBigInteger u2Out)
		{
			NetBigInteger u1 = One;
			NetBigInteger u3 = a;
			NetBigInteger v1 = Zero;
			NetBigInteger v3 = b;

			while (v3.m_sign > 0)
			{
				NetBigInteger[] q = u3.DivideAndRemainder(v3);

				NetBigInteger tmp = v1.Multiply(q[0]);
				NetBigInteger tn = u1.Subtract(tmp);
				u1 = v1;
				v1 = tn;

				u3 = v3;
				v3 = q[1];
			}

			if (u1Out != null)
			{
				u1Out.m_sign = u1.m_sign;
				u1Out.m_magnitude = u1.m_magnitude;
			}

			if (u2Out != null)
			{
				NetBigInteger tmp = u1.Multiply(a);
				tmp = u3.Subtract(tmp);
				NetBigInteger res = tmp.Divide(b);
				u2Out.m_sign = res.m_sign;
				u2Out.m_magnitude = res.m_magnitude;
			}

			return u3;
		}

		private static void ZeroOut(
			int[] x)
		{
			Array.Clear(x, 0, x.Length);
		}

		public NetBigInteger ModPow(
			NetBigInteger exponent,
			NetBigInteger m)
		{
			if (m.m_sign < 1)
				throw new ArithmeticException("Modulus must be positive");

			if (m.Equals(One))
				return Zero;

			if (exponent.m_sign == 0)
				return One;

			if (m_sign == 0)
				return Zero;

			int[] zVal = null;
			int[] yAccum = null;
			int[] yVal;

			// Montgomery exponentiation is only possible if the modulus is odd,
			// but AFAIK, this is always the case for crypto algo's
			bool useMonty = ((m.m_magnitude[m.m_magnitude.Length - 1] & 1) == 1);
			long mQ = 0;
			if (useMonty)
			{
				mQ = m.GetMQuote();

				// tmp = this * R mod m
				NetBigInteger tmp = ShiftLeft(32 * m.m_magnitude.Length).Mod(m);
				zVal = tmp.m_magnitude;

				useMonty = (zVal.Length <= m.m_magnitude.Length);

				if (useMonty)
				{
					yAccum = new int[m.m_magnitude.Length + 1];
					if (zVal.Length < m.m_magnitude.Length)
					{
						int[] longZ = new int[m.m_magnitude.Length];
						zVal.CopyTo(longZ, longZ.Length - zVal.Length);
						zVal = longZ;
					}
				}
			}

			if (!useMonty)
			{
				if (m_magnitude.Length <= m.m_magnitude.Length)
				{
					//zAccum = new int[m.magnitude.Length * 2];
					zVal = new int[m.m_magnitude.Length];
					m_magnitude.CopyTo(zVal, zVal.Length - m_magnitude.Length);
				}
				else
				{
					//
					// in normal practice we'll never see ..
					//
					NetBigInteger tmp = Remainder(m);

					//zAccum = new int[m.magnitude.Length * 2];
					zVal = new int[m.m_magnitude.Length];
					tmp.m_magnitude.CopyTo(zVal, zVal.Length - tmp.m_magnitude.Length);
				}

				yAccum = new int[m.m_magnitude.Length * 2];
			}

			yVal = new int[m.m_magnitude.Length];

			//
			// from LSW to MSW
			//
			for (int i = 0; i < exponent.m_magnitude.Length; i++)
			{
				int v = exponent.m_magnitude[i];
				int bits = 0;

				if (i == 0)
				{
					while (v > 0)
					{
						v <<= 1;
						bits++;
					}

					//
					// first time in initialise y
					//
					zVal.CopyTo(yVal, 0);

					v <<= 1;
					bits++;
				}

				while (v != 0)
				{
					if (useMonty)
					{
						// Montgomery square algo doesn't exist, and a normal
						// square followed by a Montgomery reduction proved to
						// be almost as heavy as a Montgomery mulitply.
						MultiplyMonty(yAccum, yVal, yVal, m.m_magnitude, mQ);
					}
					else
					{
						Square(yAccum, yVal);
						Remainder(yAccum, m.m_magnitude);
						Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
						ZeroOut(yAccum);
					}
					bits++;

					if (v < 0)
					{
						if (useMonty)
						{
							MultiplyMonty(yAccum, yVal, zVal, m.m_magnitude, mQ);
						}
						else
						{
							Multiply(yAccum, yVal, zVal);
							Remainder(yAccum, m.m_magnitude);
							Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0,
								yVal.Length);
							ZeroOut(yAccum);
						}
					}

					v <<= 1;
				}

				while (bits < 32)
				{
					if (useMonty)
					{
						MultiplyMonty(yAccum, yVal, yVal, m.m_magnitude, mQ);
					}
					else
					{
						Square(yAccum, yVal);
						Remainder(yAccum, m.m_magnitude);
						Array.Copy(yAccum, yAccum.Length - yVal.Length, yVal, 0, yVal.Length);
						ZeroOut(yAccum);
					}
					bits++;
				}
			}

			if (useMonty)
			{
				// Return y * R^(-1) mod m by doing y * 1 * R^(-1) mod m
				ZeroOut(zVal);
				zVal[zVal.Length - 1] = 1;
				MultiplyMonty(yAccum, yVal, zVal, m.m_magnitude, mQ);
			}

			NetBigInteger result = new NetBigInteger(1, yVal, true);

			return exponent.m_sign > 0
				? result
				: result.ModInverse(m);
		}

		// return w with w = x * x - w is assumed to have enough space.
		private static int[] Square(
			int[] w,
			int[] x)
		{
			// Note: this method allows w to be only (2 * x.Length - 1) words if result will fit
			//			if (w.Length != 2 * x.Length)
			//				throw new ArgumentException("no I don't think so...");

			ulong u1, u2, c;

			int wBase = w.Length - 1;

			for (int i = x.Length - 1; i != 0; i--)
			{
				ulong v = (ulong)(uint)x[i];

				u1 = v * v;
				u2 = u1 >> 32;
				u1 = (uint)u1;

				u1 += (ulong)(uint)w[wBase];

				w[wBase] = (int)(uint)u1;
				c = u2 + (u1 >> 32);

				for (int j = i - 1; j >= 0; j--)
				{
					--wBase;
					u1 = v * (ulong)(uint)x[j];
					u2 = u1 >> 31; // multiply by 2!
					u1 = (uint)(u1 << 1); // multiply by 2!
					u1 += c + (ulong)(uint)w[wBase];

					w[wBase] = (int)(uint)u1;
					c = u2 + (u1 >> 32);
				}

				c += (ulong)(uint)w[--wBase];
				w[wBase] = (int)(uint)c;

				if (--wBase >= 0)
				{
					w[wBase] = (int)(uint)(c >> 32);
				}
				else
				{
					Debug.Assert((uint)(c >> 32) == 0);
				}
				wBase += i;
			}

			u1 = (ulong)(uint)x[0];
			u1 = u1 * u1;
			u2 = u1 >> 32;
			u1 = u1 & IMASK;

			u1 += (ulong)(uint)w[wBase];

			w[wBase] = (int)(uint)u1;
			if (--wBase >= 0)
			{
				w[wBase] = (int)(uint)(u2 + (u1 >> 32) + (ulong)(uint)w[wBase]);
			}
			else
			{
				Debug.Assert((uint)(u2 + (u1 >> 32)) == 0);
			}

			return w;
		}

		// return x with x = y * z - x is assumed to have enough space.
		private static int[] Multiply(
			int[] x,
			int[] y,
			int[] z)
		{
			int i = z.Length;

			if (i < 1)
				return x;

			int xBase = x.Length - y.Length;

			for (; ; )
			{
				long a = z[--i] & IMASK;
				long val = 0;

				for (int j = y.Length - 1; j >= 0; j--)
				{
					val += a * (y[j] & IMASK) + (x[xBase + j] & IMASK);

					x[xBase + j] = (int)val;

					val = (long)((ulong)val >> 32);
				}

				--xBase;

				if (i < 1)
				{
					if (xBase >= 0)
					{
						x[xBase] = (int)val;
					}
					else
					{
						Debug.Assert(val == 0);
					}
					break;
				}

				x[xBase] = (int)val;
			}

			return x;
		}

		private static long FastExtEuclid(
			long a,
			long b,
			long[] uOut)
		{
			long u1 = 1;
			long u3 = a;
			long v1 = 0;
			long v3 = b;

			while (v3 > 0)
			{
				long q, tn;

				q = u3 / v3;

				tn = u1 - (v1 * q);
				u1 = v1;
				v1 = tn;

				tn = u3 - (v3 * q);
				u3 = v3;
				v3 = tn;
			}

			uOut[0] = u1;
			uOut[1] = (u3 - (u1 * a)) / b;

			return u3;
		}

		private static long FastModInverse(
			long v,
			long m)
		{
			if (m < 1)
				throw new ArithmeticException("Modulus must be positive");

			long[] x = new long[2];
			long gcd = FastExtEuclid(v, m, x);

			if (gcd != 1)
				throw new ArithmeticException("Numbers not relatively prime.");

			if (x[0] < 0)
			{
				x[0] += m;
			}

			return x[0];
		}

		private long GetMQuote()
		{
			Debug.Assert(m_sign > 0);

			if (m_quote != -1)
			{
				return m_quote; // already calculated
			}

			if (m_magnitude.Length == 0 || (m_magnitude[m_magnitude.Length - 1] & 1) == 0)
			{
				return -1; // not for even numbers
			}

			long v = (((~m_magnitude[m_magnitude.Length - 1]) | 1) & 0xffffffffL);
			m_quote = FastModInverse(v, 0x100000000L);

			return m_quote;
		}

		private static void MultiplyMonty(
			int[] a,
			int[] x,
			int[] y,
			int[] m,
			long mQuote)
		// mQuote = -m^(-1) mod b
		{
			if (m.Length == 1)
			{
				x[0] = (int)MultiplyMontyNIsOne((uint)x[0], (uint)y[0], (uint)m[0], (ulong)mQuote);
				return;
			}

			int n = m.Length;
			int nMinus1 = n - 1;
			long y_0 = y[nMinus1] & IMASK;

			// 1. a = 0 (Notation: a = (a_{n} a_{n-1} ... a_{0})_{b} )
			Array.Clear(a, 0, n + 1);

			// 2. for i from 0 to (n - 1) do the following:
			for (int i = n; i > 0; i--)
			{
				long x_i = x[i - 1] & IMASK;

				// 2.1 u = ((a[0] + (x[i] * y[0]) * mQuote) mod b
				long u = ((((a[n] & IMASK) + ((x_i * y_0) & IMASK)) & IMASK) * mQuote) & IMASK;

				// 2.2 a = (a + x_i * y + u * m) / b
				long prod1 = x_i * y_0;
				long prod2 = u * (m[nMinus1] & IMASK);
				long tmp = (a[n] & IMASK) + (prod1 & IMASK) + (prod2 & IMASK);
				long carry = (long)((ulong)prod1 >> 32) + (long)((ulong)prod2 >> 32) + (long)((ulong)tmp >> 32);
				for (int j = nMinus1; j > 0; j--)
				{
					prod1 = x_i * (y[j - 1] & IMASK);
					prod2 = u * (m[j - 1] & IMASK);
					tmp = (a[j] & IMASK) + (prod1 & IMASK) + (prod2 & IMASK) + (carry & IMASK);
					carry = (long)((ulong)carry >> 32) + (long)((ulong)prod1 >> 32) +
						(long)((ulong)prod2 >> 32) + (long)((ulong)tmp >> 32);
					a[j + 1] = (int)tmp; // division by b
				}
				carry += (a[0] & IMASK);
				a[1] = (int)carry;
				a[0] = (int)((ulong)carry >> 32); // OJO!!!!!
			}

			// 3. if x >= m the x = x - m
			if (CompareTo(0, a, 0, m) >= 0)
			{
				Subtract(0, a, 0, m);
			}

			// put the result in x
			Array.Copy(a, 1, x, 0, n);
		}

		private static uint MultiplyMontyNIsOne(
			uint x,
			uint y,
			uint m,
			ulong mQuote)
		{
			ulong um = m;
			ulong prod1 = (ulong)x * (ulong)y;
			ulong u = (prod1 * mQuote) & UIMASK;
			ulong prod2 = u * um;
			ulong tmp = (prod1 & UIMASK) + (prod2 & UIMASK);
			ulong carry = (prod1 >> 32) + (prod2 >> 32) + (tmp >> 32);

			if (carry > um)
			{
				carry -= um;
			}

			return (uint)(carry & UIMASK);
		}

		public NetBigInteger Modulus(
			NetBigInteger val)
		{
			return Mod(val);
		}

		public NetBigInteger Multiply(
			NetBigInteger val)
		{
			if (m_sign == 0 || val.m_sign == 0)
				return Zero;

			if (val.QuickPow2Check()) // val is power of two
			{
				NetBigInteger result = ShiftLeft(val.Abs().BitLength - 1);
				return val.m_sign > 0 ? result : result.Negate();
			}

			if (QuickPow2Check()) // this is power of two
			{
				NetBigInteger result = val.ShiftLeft(Abs().BitLength - 1);
				return m_sign > 0 ? result : result.Negate();
			}

			int maxBitLength = BitLength + val.BitLength;
			int resLength = (maxBitLength + BitsPerInt - 1) / BitsPerInt;

			int[] res = new int[resLength];

			if (val == this)
			{
				Square(res, m_magnitude);
			}
			else
			{
				Multiply(res, m_magnitude, val.m_magnitude);
			}

			return new NetBigInteger(m_sign * val.m_sign, res, true);
		}

		public NetBigInteger Negate()
		{
			if (m_sign == 0)
				return this;

			return new NetBigInteger(-m_sign, m_magnitude, false);
		}

		public NetBigInteger Not()
		{
			return Inc().Negate();
		}

		public NetBigInteger Pow(int exp)
		{
			if (exp < 0)
			{
				throw new ArithmeticException("Negative exponent");
			}

			if (exp == 0)
			{
				return One;
			}

			if (m_sign == 0 || Equals(One))
			{
				return this;
			}

			NetBigInteger y = One;
			NetBigInteger z = this;

			for (; ; )
			{
				if ((exp & 0x1) == 1)
				{
					y = y.Multiply(z);
				}
				exp >>= 1;
				if (exp == 0) break;
				z = z.Multiply(z);
			}

			return y;
		}
		
		private int Remainder(
			int m)
		{
			Debug.Assert(m > 0);

			long acc = 0;
			for (int pos = 0; pos < m_magnitude.Length; ++pos)
			{
				long posVal = (uint)m_magnitude[pos];
				acc = (acc << 32 | posVal) % m;
			}

			return (int)acc;
		}

		// return x = x % y - done in place (y value preserved)
		private int[] Remainder(
			int[] x,
			int[] y)
		{
			int xStart = 0;
			while (xStart < x.Length && x[xStart] == 0)
			{
				++xStart;
			}

			int yStart = 0;
			while (yStart < y.Length && y[yStart] == 0)
			{
				++yStart;
			}

			Debug.Assert(yStart < y.Length);

			int xyCmp = CompareNoLeadingZeroes(xStart, x, yStart, y);

			if (xyCmp > 0)
			{
				int yBitLength = calcBitLength(yStart, y);
				int xBitLength = calcBitLength(xStart, x);
				int shift = xBitLength - yBitLength;

				int[] c;
				int cStart = 0;
				int cBitLength = yBitLength;
				if (shift > 0)
				{
					c = ShiftLeft(y, shift);
					cBitLength += shift;
					Debug.Assert(c[0] != 0);
				}
				else
				{
					int len = y.Length - yStart;
					c = new int[len];
					Array.Copy(y, yStart, c, 0, len);
				}

				for (; ; )
				{
					if (cBitLength < xBitLength
						|| CompareNoLeadingZeroes(xStart, x, cStart, c) >= 0)
					{
						Subtract(xStart, x, cStart, c);

						while (x[xStart] == 0)
						{
							if (++xStart == x.Length)
								return x;
						}

						//xBitLength = calcBitLength(xStart, x);
						xBitLength = 32 * (x.Length - xStart - 1) + BitLen(x[xStart]);

						if (xBitLength <= yBitLength)
						{
							if (xBitLength < yBitLength)
								return x;

							xyCmp = CompareNoLeadingZeroes(xStart, x, yStart, y);

							if (xyCmp <= 0)
								break;
						}
					}

					shift = cBitLength - xBitLength;

					// NB: The case where c[cStart] is 1-bit is harmless
					if (shift == 1)
					{
						uint firstC = (uint)c[cStart] >> 1;
						uint firstX = (uint)x[xStart];
						if (firstC > firstX)
							++shift;
					}

					if (shift < 2)
					{
						c = ShiftRightOneInPlace(cStart, c);
						--cBitLength;
					}
					else
					{
						c = ShiftRightInPlace(cStart, c, shift);
						cBitLength -= shift;
					}

					//cStart = c.Length - ((cBitLength + 31) / 32);
					while (c[cStart] == 0)
					{
						++cStart;
					}
				}
			}

			if (xyCmp == 0)
			{
				Array.Clear(x, xStart, x.Length - xStart);
			}

			return x;
		}

		public NetBigInteger Remainder(
			NetBigInteger n)
		{
			if (n.m_sign == 0)
				throw new ArithmeticException("Division by zero error");

			if (m_sign == 0)
				return Zero;

			// For small values, use fast remainder method
			if (n.m_magnitude.Length == 1)
			{
				int val = n.m_magnitude[0];

				if (val > 0)
				{
					if (val == 1)
						return Zero;

					int rem = Remainder(val);

					return rem == 0
						? Zero
						: new NetBigInteger(m_sign, new int[] { rem }, false);
				}
			}

			if (CompareNoLeadingZeroes(0, m_magnitude, 0, n.m_magnitude) < 0)
				return this;

			int[] result;
			if (n.QuickPow2Check())  // n is power of two
			{
				result = LastNBits(n.Abs().BitLength - 1);
			}
			else
			{
				result = (int[])m_magnitude.Clone();
				result = Remainder(result, n.m_magnitude);
			}

			return new NetBigInteger(m_sign, result, true);
		}

		private int[] LastNBits(
			int n)
		{
			if (n < 1)
				return ZeroMagnitude;

			int numWords = (n + BitsPerInt - 1) / BitsPerInt;
			numWords = System.Math.Min(numWords, m_magnitude.Length);
			int[] result = new int[numWords];

			Array.Copy(m_magnitude, m_magnitude.Length - numWords, result, 0, numWords);

			int hiBits = n % 32;
			if (hiBits != 0)
			{
				result[0] &= ~(-1 << hiBits);
			}

			return result;
		}


		// do a left shift - this returns a new array.
		private static int[] ShiftLeft(
			int[] mag,
			int n)
		{
			int nInts = (int)((uint)n >> 5);
			int nBits = n & 0x1f;
			int magLen = mag.Length;
			int[] newMag;

			if (nBits == 0)
			{
				newMag = new int[magLen + nInts];
				mag.CopyTo(newMag, 0);
			}
			else
			{
				int i = 0;
				int nBits2 = 32 - nBits;
				int highBits = (int)((uint)mag[0] >> nBits2);

				if (highBits != 0)
				{
					newMag = new int[magLen + nInts + 1];
					newMag[i++] = highBits;
				}
				else
				{
					newMag = new int[magLen + nInts];
				}

				int m = mag[0];
				for (int j = 0; j < magLen - 1; j++)
				{
					int next = mag[j + 1];

					newMag[i++] = (m << nBits) | (int)((uint)next >> nBits2);
					m = next;
				}

				newMag[i] = mag[magLen - 1] << nBits;
			}

			return newMag;
		}

		public NetBigInteger ShiftLeft(
			int n)
		{
			if (m_sign == 0 || m_magnitude.Length == 0)
				return Zero;

			if (n == 0)
				return this;

			if (n < 0)
				return ShiftRight(-n);

			NetBigInteger result = new NetBigInteger(m_sign, ShiftLeft(m_magnitude, n), true);

			if (m_numBits != -1)
			{
				result.m_numBits = m_sign > 0
					? m_numBits
					: m_numBits + n;
			}

			if (m_numBitLength != -1)
			{
				result.m_numBitLength = m_numBitLength + n;
			}

			return result;
		}

		// do a right shift - this does it in place.
		private static int[] ShiftRightInPlace(
			int start,
			int[] mag,
			int n)
		{
			int nInts = (int)((uint)n >> 5) + start;
			int nBits = n & 0x1f;
			int magEnd = mag.Length - 1;

			if (nInts != start)
			{
				int delta = (nInts - start);

				for (int i = magEnd; i >= nInts; i--)
				{
					mag[i] = mag[i - delta];
				}
				for (int i = nInts - 1; i >= start; i--)
				{
					mag[i] = 0;
				}
			}

			if (nBits != 0)
			{
				int nBits2 = 32 - nBits;
				int m = mag[magEnd];

				for (int i = magEnd; i > nInts; --i)
				{
					int next = mag[i - 1];

					mag[i] = (int)((uint)m >> nBits) | (next << nBits2);
					m = next;
				}

				mag[nInts] = (int)((uint)mag[nInts] >> nBits);
			}

			return mag;
		}

		// do a right shift by one - this does it in place.
		private static int[] ShiftRightOneInPlace(
			int start,
			int[] mag)
		{
			int i = mag.Length;
			int m = mag[i - 1];

			while (--i > start)
			{
				int next = mag[i - 1];
				mag[i] = ((int)((uint)m >> 1)) | (next << 31);
				m = next;
			}

			mag[start] = (int)((uint)mag[start] >> 1);

			return mag;
		}

		public NetBigInteger ShiftRight(
			int n)
		{
			if (n == 0)
				return this;

			if (n < 0)
				return ShiftLeft(-n);

			if (n >= BitLength)
				return (m_sign < 0 ? One.Negate() : Zero);

			//			int[] res = (int[]) magnitude.Clone();
			//
			//			res = ShiftRightInPlace(0, res, n);
			//
			//			return new BigInteger(sign, res, true);

			int resultLength = (BitLength - n + 31) >> 5;
			int[] res = new int[resultLength];

			int numInts = n >> 5;
			int numBits = n & 31;

			if (numBits == 0)
			{
				Array.Copy(m_magnitude, 0, res, 0, res.Length);
			}
			else
			{
				int numBits2 = 32 - numBits;

				int magPos = m_magnitude.Length - 1 - numInts;
				for (int i = resultLength - 1; i >= 0; --i)
				{
					res[i] = (int)((uint)m_magnitude[magPos--] >> numBits);

					if (magPos >= 0)
					{
						res[i] |= m_magnitude[magPos] << numBits2;
					}
				}
			}

			Debug.Assert(res[0] != 0);

			return new NetBigInteger(m_sign, res, false);
		}

		public int SignValue
		{
			get { return m_sign; }
		}

		// returns x = x - y - we assume x is >= y
		private static int[] Subtract(
			int xStart,
			int[] x,
			int yStart,
			int[] y)
		{
			Debug.Assert(yStart < y.Length);
			Debug.Assert(x.Length - xStart >= y.Length - yStart);

			int iT = x.Length;
			int iV = y.Length;
			long m;
			int borrow = 0;

			do
			{
				m = (x[--iT] & IMASK) - (y[--iV] & IMASK) + borrow;
				x[iT] = (int)m;

				//				borrow = (m < 0) ? -1 : 0;
				borrow = (int)(m >> 63);
			}
			while (iV > yStart);

			if (borrow != 0)
			{
				while (--x[--iT] == -1)
				{
				}
			}

			return x;
		}

		public NetBigInteger Subtract(
			NetBigInteger n)
		{
			if (n.m_sign == 0)
				return this;

			if (m_sign == 0)
				return n.Negate();

			if (m_sign != n.m_sign)
				return Add(n.Negate());

			int compare = CompareNoLeadingZeroes(0, m_magnitude, 0, n.m_magnitude);
			if (compare == 0)
				return Zero;

			NetBigInteger bigun, lilun;
			if (compare < 0)
			{
				bigun = n;
				lilun = this;
			}
			else
			{
				bigun = this;
				lilun = n;
			}

			return new NetBigInteger(m_sign * compare, doSubBigLil(bigun.m_magnitude, lilun.m_magnitude), true);
		}

		private static int[] doSubBigLil(
			int[] bigMag,
			int[] lilMag)
		{
			int[] res = (int[])bigMag.Clone();

			return Subtract(0, res, 0, lilMag);
		}

		public byte[] ToByteArray()
		{
			return ToByteArray(false);
		}

		public byte[] ToByteArrayUnsigned()
		{
			return ToByteArray(true);
		}

		private byte[] ToByteArray(
			bool unsigned)
		{
			if (m_sign == 0)
				return unsigned ? ZeroEncoding : new byte[1];

			int nBits = (unsigned && m_sign > 0)
				? BitLength
				: BitLength + 1;

			int nBytes = GetByteLength(nBits);
			byte[] bytes = new byte[nBytes];

			int magIndex = m_magnitude.Length;
			int bytesIndex = bytes.Length;

			if (m_sign > 0)
			{
				while (magIndex > 1)
				{
					uint mag = (uint)m_magnitude[--magIndex];
					bytes[--bytesIndex] = (byte)mag;
					bytes[--bytesIndex] = (byte)(mag >> 8);
					bytes[--bytesIndex] = (byte)(mag >> 16);
					bytes[--bytesIndex] = (byte)(mag >> 24);
				}

				uint lastMag = (uint)m_magnitude[0];
				while (lastMag > byte.MaxValue)
				{
					bytes[--bytesIndex] = (byte)lastMag;
					lastMag >>= 8;
				}

				bytes[--bytesIndex] = (byte)lastMag;
			}
			else // sign < 0
			{
				bool carry = true;

				while (magIndex > 1)
				{
					uint mag = ~((uint)m_magnitude[--magIndex]);

					if (carry)
					{
						carry = (++mag == uint.MinValue);
					}

					bytes[--bytesIndex] = (byte)mag;
					bytes[--bytesIndex] = (byte)(mag >> 8);
					bytes[--bytesIndex] = (byte)(mag >> 16);
					bytes[--bytesIndex] = (byte)(mag >> 24);
				}

				uint lastMag = (uint)m_magnitude[0];

				if (carry)
				{
					// Never wraps because magnitude[0] != 0
					--lastMag;
				}

				while (lastMag > byte.MaxValue)
				{
					bytes[--bytesIndex] = (byte)~lastMag;
					lastMag >>= 8;
				}

				bytes[--bytesIndex] = (byte)~lastMag;

				if (bytesIndex > 0)
				{
					bytes[--bytesIndex] = byte.MaxValue;
				}
			}

			return bytes;
		}

		public override string ToString()
		{
			return ToString(10);
		}

		public string ToString(
			int radix)
		{
			switch (radix)
			{
				case 2:
				case 10:
				case 16:
					break;
				default:
					throw new FormatException("Only bases 2, 10, 16 are allowed");
			}

			// NB: Can only happen to internally managed instances
			if (m_magnitude == null)
				return "null";

			if (m_sign == 0)
				return "0";

			Debug.Assert(m_magnitude.Length > 0);

			StringBuilder sb = new StringBuilder();

			if (radix == 16)
			{
				sb.Append(m_magnitude[0].ToString("x"));

				for (int i = 1; i < m_magnitude.Length; i++)
				{
					sb.Append(m_magnitude[i].ToString("x8"));
				}
			}
			else if (radix == 2)
			{
				sb.Append('1');

				for (int i = BitLength - 2; i >= 0; --i)
				{
					sb.Append(TestBit(i) ? '1' : '0');
				}
			}
			else
			{
				// This is algorithm 1a from chapter 4.4 in Seminumerical Algorithms, slow but it works
				Stack S = new Stack();
				NetBigInteger bs = ValueOf(radix);

				NetBigInteger u = Abs();
				NetBigInteger b;

				while (u.m_sign != 0)
				{
					b = u.Mod(bs);
					if (b.m_sign == 0)
					{
						S.Push("0");
					}
					else
					{
						// see how to interact with different bases
						S.Push(b.m_magnitude[0].ToString("d"));
					}
					u = u.Divide(bs);
				}

				// Then pop the stack
				while (S.Count != 0)
				{
					sb.Append((string)S.Pop());
				}
			}

			string s = sb.ToString();

			Debug.Assert(s.Length > 0);

			// Strip leading zeros. (We know this number is not all zeroes though)
			if (s[0] == '0')
			{
				int nonZeroPos = 0;
				while (s[++nonZeroPos] == '0') { }

				s = s.Substring(nonZeroPos);
			}

			if (m_sign == -1)
			{
				s = "-" + s;
			}

			return s;
		}

		private static NetBigInteger createUValueOf(
			ulong value)
		{
			int msw = (int)(value >> 32);
			int lsw = (int)value;

			if (msw != 0)
				return new NetBigInteger(1, new int[] { msw, lsw }, false);

			if (lsw != 0)
			{
				NetBigInteger n = new NetBigInteger(1, new int[] { lsw }, false);
				// Check for a power of two
				if ((lsw & -lsw) == lsw)
				{
					n.m_numBits = 1;
				}
				return n;
			}

			return Zero;
		}

		private static NetBigInteger createValueOf(
			long value)
		{
			if (value < 0)
			{
				if (value == long.MinValue)
					return createValueOf(~value).Not();

				return createValueOf(-value).Negate();
			}

			return createUValueOf((ulong)value);
		}

		public static NetBigInteger ValueOf(
			long value)
		{
			switch (value)
			{
				case 0:
					return Zero;
				case 1:
					return One;
				case 2:
					return Two;
				case 3:
					return Three;
				case 10:
					return Ten;
			}

			return createValueOf(value);
		}

		public int GetLowestSetBit()
		{
			if (m_sign == 0)
				return -1;

			int w = m_magnitude.Length;

			while (--w > 0)
			{
				if (m_magnitude[w] != 0)
					break;
			}

			int word = (int)m_magnitude[w];
			Debug.Assert(word != 0);

			int b = (word & 0x0000FFFF) == 0
				? (word & 0x00FF0000) == 0
					? 7
					: 15
				: (word & 0x000000FF) == 0
					? 23
					: 31;

			while (b > 0)
			{
				if ((word << b) == int.MinValue)
					break;

				b--;
			}

			return ((m_magnitude.Length - w) * 32 - (b + 1));
		}

		public bool TestBit(
			int n)
		{
			if (n < 0)
				throw new ArithmeticException("Bit position must not be negative");

			if (m_sign < 0)
				return !Not().TestBit(n);

			int wordNum = n / 32;
			if (wordNum >= m_magnitude.Length)
				return false;

			int word = m_magnitude[m_magnitude.Length - 1 - wordNum];
			return ((word >> (n % 32)) & 1) > 0;
		}
	}
}