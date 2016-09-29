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
#if !__ANDROID__ && !IOS && !UNITY_WEBPLAYER && !UNITY_ANDROID && !UNITY_IPHONE
			ulong seed = (ulong)System.Diagnostics.Stopwatch.GetTimestamp();
			seed ^= (ulong)Environment.WorkingSet;
			ulong s2 = (ulong)Interlocked.Increment(ref m_seedIncrement);
			s2 |= (((ulong)Guid.NewGuid().GetHashCode()) << 32);
			seed ^= s2;
#else
			ulong seed = (ulong)Environment.TickCount;
			seed |= (((ulong)(new object().GetHashCode())) << 32);
			ulong s2 = (ulong)Guid.NewGuid().GetHashCode();
			s2 |= (((ulong)Interlocked.Increment(ref m_seedIncrement)) << 32);
			seed ^= s2;
#endif
			return seed;
		}
	}
}
