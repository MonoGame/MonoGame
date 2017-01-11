using System;
using System.Threading;

namespace Lidgren.Network
{
	/// <summary>
	/// Class for generating random seeds
	/// </summary>
	public static class NetRandomSeed
	{
		private static int m_seedIncrement = -1640531527;

		/// <summary>
		/// Generates a 32 bit random seed
		/// </summary>
		[CLSCompliant(false)]
		public static uint GetUInt32()
		{
			ulong seed = GetUInt64();
			uint low = (uint)seed;
			uint high = (uint)(seed >> 32);
			return low ^ high;
		}

		/// <summary>
		/// Generates a 64 bit random seed
		/// </summary>
		[CLSCompliant(false)]
		public static ulong GetUInt64()
		{
			var guidBytes = Guid.NewGuid().ToByteArray();
			ulong seed =
				((ulong)guidBytes[0] << (8 * 0)) |
				((ulong)guidBytes[1] << (8 * 1)) |
				((ulong)guidBytes[2] << (8 * 2)) |
				((ulong)guidBytes[3] << (8 * 3)) |
				((ulong)guidBytes[4] << (8 * 4)) |
				((ulong)guidBytes[5] << (8 * 5)) |
				((ulong)guidBytes[6] << (8 * 6)) |
				((ulong)guidBytes[7] << (8 * 7));

			return seed ^ NetUtility.GetPlatformSeed(m_seedIncrement);
		}
	}
}
