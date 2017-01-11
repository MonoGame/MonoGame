using System;
using System.Collections.Generic;
using System.Threading;

namespace Lidgren.Network
{
	/// <summary>
	/// NetRandom base class
	/// </summary>
	public abstract class NetRandom : Random
	{
		/// <summary>
		/// Get global instance of NetRandom (uses MWCRandom)
		/// </summary>
		public static NetRandom Instance = new MWCRandom();

		private const double c_realUnitInt = 1.0 / ((double)int.MaxValue + 1.0);

		/// <summary>
		/// Constructor with randomized seed
		/// </summary>
		public NetRandom()
		{
			Initialize(NetRandomSeed.GetUInt32());
		}

		/// <summary>
		/// Constructor with provided 32 bit seed
		/// </summary>
		public NetRandom(int seed)
		{
			Initialize((uint)seed);
		}

		/// <summary>
		/// (Re)initialize this instance with provided 32 bit seed
		/// </summary>
		[CLSCompliant(false)]
		public virtual void Initialize(uint seed)
		{
			// should be abstract, but non-CLS compliant methods can't be abstract!
			throw new NotImplementedException("Implement this in inherited classes");
		}

		/// <summary>
		/// Generates a random value from UInt32.MinValue to UInt32.MaxValue, inclusively
		/// </summary>
		[CLSCompliant(false)]
		public virtual uint NextUInt32()
		{
			// should be abstract, but non-CLS compliant methods can't be abstract!
			throw new NotImplementedException("Implement this in inherited classes");
		}

		/// <summary>
		/// Generates a random value that is greater or equal than 0 and less than Int32.MaxValue
		/// </summary>
		public override int Next()
		{
			var retval = (int)(0x7FFFFFFF & NextUInt32());
			if (retval == 0x7FFFFFFF)
				return NextInt32();
			return retval;
		}

		/// <summary>
		/// Generates a random value greater or equal than 0 and less or equal than Int32.MaxValue (inclusively)
		/// </summary>
		public int NextInt32()
		{
			return (int)(0x7FFFFFFF & NextUInt32());
		}

		/// <summary>
		/// Returns random value larger or equal to 0.0 and less than 1.0
		/// </summary>
		public override double NextDouble()
		{
			return c_realUnitInt * NextInt32();
		}

		/// <summary>
		/// Returns random value is greater or equal than 0.0 and less than 1.0
		/// </summary>
		protected override double Sample()
		{
			return c_realUnitInt * NextInt32();
		}

		/// <summary>
		/// Returns random value is greater or equal than 0.0f and less than 1.0f
		/// </summary>
		public float NextSingle()
		{
			var retval = (float)(c_realUnitInt * NextInt32());
			if (retval == 1.0f)
				return NextSingle();
			return retval;
		}

		/// <summary>
		/// Returns a random value is greater or equal than 0 and less than maxValue
		/// </summary>
		public override int Next(int maxValue)
		{
			return (int)(NextDouble() * maxValue);
		}

		/// <summary>
		/// Returns a random value is greater or equal than minValue and less than maxValue
		/// </summary>
		public override int Next(int minValue, int maxValue)
		{
            return minValue + (int)(NextDouble() * (double)(maxValue - minValue));
		}
		
		/// <summary>
		/// Generates a random value between UInt64.MinValue to UInt64.MaxValue
		/// </summary>
		[CLSCompliant(false)]
		public ulong NextUInt64()
		{
			ulong retval = NextUInt32();
			retval |= NextUInt32() << 32;
			return retval;
		}

		private uint m_boolValues;
		private int m_nextBoolIndex;

		/// <summary>
		/// Returns true or false, randomly
		/// </summary>
		public bool NextBool()
		{
			if (m_nextBoolIndex >= 32)
			{
				m_boolValues = NextUInt32();
				m_nextBoolIndex = 1;
			}

			var retval = ((m_boolValues >> m_nextBoolIndex) & 1) == 1;
			m_nextBoolIndex++;
			return retval;
		}
		

		/// <summary>
		/// Fills all bytes from offset to offset + length in buffer with random values
		/// </summary>
		public virtual void NextBytes(byte[] buffer, int offset, int length)
		{
			int full = length / 4;
			int ptr = offset;
			for (int i = 0; i < full; i++)
			{
				uint r = NextUInt32();
				buffer[ptr++] = (byte)r;
				buffer[ptr++] = (byte)(r >> 8);
				buffer[ptr++] = (byte)(r >> 16);
				buffer[ptr++] = (byte)(r >> 24);
			}

			int rest = length - (full * 4);
			for (int i = 0; i < rest; i++)
				buffer[ptr++] = (byte)NextUInt32();
		}

		/// <summary>
		/// Fill the specified buffer with random values
		/// </summary>
		public override void NextBytes(byte[] buffer)
		{
			NextBytes(buffer, 0, buffer.Length);
		}
	}
}
