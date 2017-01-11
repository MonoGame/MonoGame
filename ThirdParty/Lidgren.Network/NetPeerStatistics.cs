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

// Uncomment the line below to get statistics in RELEASE builds
//#define USE_RELEASE_STATISTICS

using System;
using System.Text;
using System.Diagnostics;

namespace Lidgren.Network
{
	/// <summary>
	/// Statistics for a NetPeer instance
	/// </summary>
	public sealed class NetPeerStatistics
	{
		private readonly NetPeer m_peer;

		internal int m_sentPackets;
		internal int m_receivedPackets;

		internal int m_sentMessages;
		internal int m_receivedMessages;
		internal int m_receivedFragments;

		internal int m_sentBytes;
		internal int m_receivedBytes;

		internal long m_bytesAllocated;

		internal NetPeerStatistics(NetPeer peer)
		{
			m_peer = peer;
			Reset();
		}

		internal void Reset()
		{
			m_sentPackets = 0;
			m_receivedPackets = 0;

			m_sentMessages = 0;
			m_receivedMessages = 0;
			m_receivedFragments = 0;

			m_sentBytes = 0;
			m_receivedBytes = 0;

			m_bytesAllocated = 0;
		}

		/// <summary>
		/// Gets the number of sent packets since the NetPeer was initialized
		/// </summary>
		public int SentPackets { get { return m_sentPackets; } }

		/// <summary>
		/// Gets the number of received packets since the NetPeer was initialized
		/// </summary>
		public int ReceivedPackets { get { return m_receivedPackets; } }

		/// <summary>
		/// Gets the number of sent messages since the NetPeer was initialized
		/// </summary>
		public int SentMessages { get { return m_sentMessages; } }

		/// <summary>
		/// Gets the number of received messages since the NetPeer was initialized
		/// </summary>
		public int ReceivedMessages { get { return m_receivedMessages; } }

		/// <summary>
		/// Gets the number of sent bytes since the NetPeer was initialized
		/// </summary>
		public int SentBytes { get { return m_sentBytes; } }

		/// <summary>
		/// Gets the number of received bytes since the NetPeer was initialized
		/// </summary>
		public int ReceivedBytes { get { return m_receivedBytes; } }

		/// <summary>
		/// Gets the number of bytes allocated (and possibly garbage collected) for message storage
		/// </summary>
		public long StorageBytesAllocated { get { return m_bytesAllocated; } }

		/// <summary>
		/// Gets the number of bytes in the recycled pool
		/// </summary>
		public int BytesInRecyclePool
		{
			get
			{
				lock (m_peer.m_storagePool)
					return m_peer.m_storagePoolBytes;
			}
		}

#if !USE_RELEASE_STATISTICS
		[Conditional("DEBUG")]
#endif
		internal void PacketSent(int numBytes, int numMessages)
		{
			m_sentPackets++;
			m_sentBytes += numBytes;
			m_sentMessages += numMessages;
		}

#if !USE_RELEASE_STATISTICS
		[Conditional("DEBUG")]
#endif
		internal void PacketReceived(int numBytes, int numMessages, int numFragments)
		{
			m_receivedPackets++;
			m_receivedBytes += numBytes;
			m_receivedMessages += numMessages;
			m_receivedFragments += numFragments;
		}

		/// <summary>
		/// Returns a string that represents this object
		/// </summary>
		public override string ToString()
		{
			StringBuilder bdr = new StringBuilder();
			bdr.AppendLine(m_peer.ConnectionsCount.ToString() + " connections");
#if DEBUG || USE_RELEASE_STATISTICS
			bdr.AppendLine("Sent " + m_sentBytes + " bytes in " + m_sentMessages + " messages in " + m_sentPackets + " packets");
			bdr.AppendLine("Received " + m_receivedBytes + " bytes in " + m_receivedMessages + " messages (of which " + m_receivedFragments + " fragments) in " + m_receivedPackets + " packets");
#else
			bdr.AppendLine("Sent (n/a) bytes in (n/a) messages in (n/a) packets");
			bdr.AppendLine("Received (n/a) bytes in (n/a) messages in (n/a) packets");
#endif
			bdr.AppendLine("Storage allocated " + m_bytesAllocated + " bytes");
			if (m_peer.m_storagePool != null)
				bdr.AppendLine("Recycled pool " + m_peer.m_storagePoolBytes + " bytes (" + m_peer.m_storageSlotsUsedCount + " entries)");
			return bdr.ToString();
		}
	}
}
