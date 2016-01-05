using System;
using System.Security.Cryptography;

namespace Lidgren.Network
{
	/// <summary>
	/// Multiply With Carry random
	/// </summary>
	public class MWCRandom : NetRandom
	{
		/// <summary>
		/// Get global instance of MWCRandom
		/// </summary>
		public static new readonly MWCRandom Instance = new MWCRandom();

		private uint m_w, m_z;

		/// <summary>
		/// Constructor with randomized seed
		/// </summary>
		public MWCRandom()
		{
			Initialize(NetRandomSeed.GetUInt64());
		}

		/// <summary>
		/// (Re)initialize this instance with provided 32 bit seed
		/// </summary>
		[CLSCompliant(false)]
		public override void Initialize(uint seed)
		{
			m_w = seed;
			m_z = seed * 16777619;
		}

		/// <summary>
		/// (Re)initialize this instance with provided 64 bit seed
		/// </summary>
		[CLSCompliant(false)]
		public void Initialize(ulong seed)
		{
			m_w = (uint)seed;
			m_z = (uint)(seed >> 32);
		}

		/// <summary>
		/// Generates a random value from UInt32.MinValue to UInt32.MaxValue, inclusively
		/// </summary>
		[CLSCompliant(false)]
		public override uint NextUInt32()
		{
			m_z = 36969 * (m_z & 65535) + (m_z >> 16);
			m_w = 18000 * (m_w & 65535) + (m_w >> 16);
			return ((m_z << 16) + m_w);
		}
	}

	/// <summary>
	/// Xor Shift based random
	/// </summary>
	public sealed class XorShiftRandom : NetRandom
	{
		/// <summary>
		/// Get global instance of XorShiftRandom
		/// </summary>
		public static new readonly XorShiftRandom Instance = new XorShiftRandom();

		private const uint c_x = 123456789;
		private const uint c_y = 362436069;
		private const uint c_z = 521288629;
		private const uint c_w = 88675123;

		private uint m_x, m_y, m_z, m_w;

		/// <summary>
		/// Constructor with randomized seed
		/// </summary>
		public XorShiftRandom()
		{
			Initialize(NetRandomSeed.GetUInt64());
		}

		/// <summary>
		/// Constructor with provided 64 bit seed
		/// </summary>
		[CLSCompliant(false)]
		public XorShiftRandom(ulong seed)
		{
			Initialize(seed);
		}

		/// <summary>
		/// (Re)initialize this instance with provided 32 bit seed
		/// </summary>
		[CLSCompliant(false)]
		public override void Initialize(uint seed)
		{
			m_x = (uint)seed;
			m_y = c_y;
			m_z = c_z;
			m_w = c_w;
		}

		/// <summary>
		/// (Re)initialize this instance with provided 64 bit seed
		/// </summary>
		[CLSCompliant(false)]
		public void Initialize(ulong seed)
		{
			m_x = (uint)seed;
			m_y = c_y;
			m_z = (uint)(seed << 32);
			m_w = c_w;
		}

		/// <summary>
		/// Generates a random value from UInt32.MinValue to UInt32.MaxValue, inclusively
		/// </summary>
		[CLSCompliant(false)]
		public override uint NextUInt32()
		{
			uint t = (m_x ^ (m_x << 11));
			m_x = m_y; m_y = m_z; m_z = m_w;
			return (m_w = (m_w ^ (m_w >> 19)) ^ (t ^ (t >> 8)));
		}
	}

	/// <summary>
	/// Mersenne Twister based random
	/// </summary>
	public sealed class MersenneTwisterRandom : NetRandom
	{
		/// <summary>
		/// Get global instance of MersenneTwisterRandom
		/// </summary>
		public static new readonly MersenneTwisterRandom Instance = new MersenneTwisterRandom();

		private const int N = 624;
		private const int M = 397;
		private const uint MATRIX_A = 0x9908b0dfU;
		private const uint UPPER_MASK = 0x80000000U;
		private const uint LOWER_MASK = 0x7fffffffU;
		private const uint TEMPER1 = 0x9d2c5680U;
		private const uint TEMPER2 = 0xefc60000U;
		private const int TEMPER3 = 11;
		private const int TEMPER4 = 7;
		private const int TEMPER5 = 15;
		private const int TEMPER6 = 18;

		private UInt32[] mt;
		private int mti;
		private UInt32[] mag01;

		private const double c_realUnitInt = 1.0 / ((double)int.MaxValue + 1.0);

		/// <summary>
		/// Constructor with randomized seed
		/// </summary>
		public MersenneTwisterRandom()
		{
			Initialize(NetRandomSeed.GetUInt32());
		}

		/// <summary>
		/// Constructor with provided 32 bit seed
		/// </summary>
		[CLSCompliant(false)]
		public MersenneTwisterRandom(uint seed)
		{
			Initialize(seed);
		}

		/// <summary>
		/// (Re)initialize this instance with provided 32 bit seed
		/// </summary>
		[CLSCompliant(false)]
		public override void Initialize(uint seed)
		{
			mt = new UInt32[N];
			mti = N + 1;
			mag01 = new UInt32[] { 0x0U, MATRIX_A };
			mt[0] = seed;
			for (int i = 1; i < N; i++)
				mt[i] = (UInt32)(1812433253 * (mt[i - 1] ^ (mt[i - 1] >> 30)) + i);
		}

		/// <summary>
		/// Generates a random value from UInt32.MinValue to UInt32.MaxValue, inclusively
		/// </summary>
		[CLSCompliant(false)]
		public override uint NextUInt32()
		{
			UInt32 y;
			if (mti >= N)
			{
				GenRandAll();
				mti = 0;
			}
			y = mt[mti++];
			y ^= (y >> TEMPER3);
			y ^= (y << TEMPER4) & TEMPER1;
			y ^= (y << TEMPER5) & TEMPER2;
			y ^= (y >> TEMPER6);
			return y;
		}

		private void GenRandAll()
		{
			int kk = 1;
			UInt32 y;
			UInt32 p;
			y = mt[0] & UPPER_MASK;
			do
			{
				p = mt[kk];
				mt[kk - 1] = mt[kk + (M - 1)] ^ ((y | (p & LOWER_MASK)) >> 1) ^ mag01[p & 1];
				y = p & UPPER_MASK;
			} while (++kk < N - M + 1);
			do
			{
				p = mt[kk];
				mt[kk - 1] = mt[kk + (M - N - 1)] ^ ((y | (p & LOWER_MASK)) >> 1) ^ mag01[p & 1];
				y = p & UPPER_MASK;
			} while (++kk < N);
			p = mt[0];
			mt[N - 1] = mt[M - 1] ^ ((y | (p & LOWER_MASK)) >> 1) ^ mag01[p & 1];
		}
	}

	/// <summary>
	/// RNGCryptoServiceProvider based random; very slow but cryptographically safe
	/// </summary>
	public class CryptoRandom : NetRandom
	{
		/// <summary>
		/// Global instance of CryptoRandom
		/// </summary>
		public static new readonly CryptoRandom Instance = new CryptoRandom();

		private RandomNumberGenerator m_rnd = new RNGCryptoServiceProvider();

		/// <summary>
		/// Seed in CryptoRandom does not create deterministic sequences
		/// </summary>
		[CLSCompliant(false)]
		public override void Initialize(uint seed)
		{
			byte[] tmp = new byte[seed % 16];
			m_rnd.GetBytes(tmp); // just prime it
		}

		/// <summary>
		/// Generates a random value from UInt32.MinValue to UInt32.MaxValue, inclusively
		/// </summary>
		[CLSCompliant(false)]
		public override uint NextUInt32()
		{
			var bytes = new byte[4];
			m_rnd.GetBytes(bytes);
			return (uint)bytes[0] | (((uint)bytes[1]) << 8) | (((uint)bytes[2]) << 16) | (((uint)bytes[3]) << 24);
		}

		/// <summary>
		/// Fill the specified buffer with random values
		/// </summary>
		public override void NextBytes(byte[] buffer)
		{
			m_rnd.GetBytes(buffer);
		}

		/// <summary>
		/// Fills all bytes from offset to offset + length in buffer with random values
		/// </summary>
		public override void NextBytes(byte[] buffer, int offset, int length)
		{
			var bytes = new byte[length];
			m_rnd.GetBytes(bytes);
			Array.Copy(bytes, 0, buffer, offset, length);
		}
	}
}
