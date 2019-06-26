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
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Lidgren.Network
{
	internal enum MessageResendReason
	{
		Delay,
		HoleInSequence
	}

	/// <summary>
	/// Statistics for a NetConnection instance
	/// </summary>
	public sealed class NetConnectionStatistics
	{
		private readonly NetConnection m_connection;

		internal int m_sentPackets;
		internal int m_receivedPackets;

		internal int m_sentMessages;
		internal int m_receivedMessages;
		internal int m_receivedFragments;

		internal int m_sentBytes;
		internal int m_receivedBytes;

		internal int m_resentMessagesDueToDelay;
		internal int m_resentMessagesDueToHole;

		internal NetConnectionStatistics(NetConnection conn)
		{
			m_connection = conn;
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
			m_resentMessagesDueToDelay = 0;
			m_resentMessagesDueToHole = 0;
		}

		/// <summary>
		/// Gets the number of sent packets for this connection
		/// </summary>
		public int SentPackets { get { return m_sentPackets; } }

		/// <summary>
		/// Gets the number of received packets for this connection
		/// </summary>
		public int ReceivedPackets { get { return m_receivedPackets; } }

		/// <summary>
		/// Gets the number of sent bytes for this connection
		/// </summary>
		public int SentBytes { get { return m_sentBytes; } }

		/// <summary>
		/// Gets the number of received bytes for this connection
		/// </summary>
		public int ReceivedBytes { get { return m_receivedBytes; } }

		/// <summary>
		/// Gets the number of resent reliable messages for this connection
		/// </summary>
		public int ResentMessages { get { return m_resentMessagesDueToHole + m_resentMessagesDueToDelay; } }

		// public double LastSendRespondedTo { get { return m_connection.m_lastSendRespondedTo; } }

#if USE_RELEASE_STATISTICS
		internal void PacketSent(int numBytes, int numMessages)
		{
			NetException.Assert(numBytes > 0 && numMessages > 0);
			m_sentPackets++;
			m_sentBytes += numBytes;
			m_sentMessages += numMessages;
		}
#else
		[Conditional("DEBUG")]
		internal void PacketSent(int numBytes, int numMessages)
		{
			NetException.Assert(numBytes > 0 && numMessages > 0);
			m_sentPackets++;
			m_sentBytes += numBytes;
			m_sentMessages += numMessages;
		}
#endif

#if USE_RELEASE_STATISTICS
		internal void PacketReceived(int numBytes, int numMessages)
		{
			NetException.Assert(numBytes > 0 && numMessages > 0);
			m_receivedPackets++;
			m_receivedBytes += numBytes;
			m_receivedMessages += numMessages;
		}
#else
		[Conditional("DEBUG")]
		internal void PacketReceived(int numBytes, int numMessages, int numFragments)
		{
			NetException.Assert(numBytes > 0 && numMessages > 0);
			m_receivedPackets++;
			m_receivedBytes += numBytes;
			m_receivedMessages += numMessages;
			m_receivedFragments += numFragments;
		}
#endif

#if USE_RELEASE_STATISTICS
		internal void MessageResent(MessageResendReason reason)
		{
			if (reason == MessageResendReason.Delay)
				m_resentMessagesDueToDelay++;
			else
				m_resentMessagesDueToHole++;
		}
#else
		[Conditional("DEBUG")]
		internal void MessageResent(MessageResendReason reason)
		{
			if (reason == MessageResendReason.Delay)
				m_resentMessagesDueToDelay++;
			else
				m_resentMessagesDueToHole++;
		}
#endif

		/// <summary>
		/// Returns a string that represents this object
		/// </summary>
		public override string ToString()
		{
			StringBuilder bdr = new StringBuilder();
			//bdr.AppendLine("Average roundtrip time: " + NetTime.ToReadable(m_connection.m_averageRoundtripTime));
			bdr.AppendLine("Current MTU: " + m_connection.m_currentMTU);
			bdr.AppendLine("Sent " + m_sentBytes + " bytes in " + m_sentMessages + " messages in " + m_sentPackets + " packets");
			bdr.AppendLine("Received " + m_receivedBytes + " bytes in " + m_receivedMessages + " messages (of which " + m_receivedFragments + " fragments) in " + m_receivedPackets + " packets");

			if (m_resentMessagesDueToDelay > 0)
				bdr.AppendLine("Resent messages (delay): " + m_resentMessagesDueToDelay);
			if (m_resentMessagesDueToHole > 0)
				bdr.AppendLine("Resent messages (holes): " + m_resentMessagesDueToHole);

			int numUnsent = 0;
			int numStored = 0;
			foreach (NetSenderChannelBase sendChan in m_connection.m_sendChannels)
			{
				if (sendChan == null)
					continue;
				numUnsent += sendChan.m_queuedSends.Count;

				var relSendChan = sendChan as NetReliableSenderChannel;
				if (relSendChan != null)
				{
					for (int i = 0; i < relSendChan.m_storedMessages.Length; i++)
						if (relSendChan.m_storedMessages[i].Message != null)
							numStored++;
				}
			}

			int numWithheld = 0;
			foreach (NetReceiverChannelBase recChan in m_connection.m_receiveChannels)
			{
				var relRecChan = recChan as NetReliableOrderedReceiver;
				if (relRecChan != null)
				{
					for (int i = 0; i < relRecChan.m_withheldMessages.Length; i++)
						if (relRecChan.m_withheldMessages[i] != null)
							numWithheld++;
				}
			}

			bdr.AppendLine("Unsent messages: " + numUnsent);
			bdr.AppendLine("Stored messages: " + numStored);
			bdr.AppendLine("Withheld messages: " + numWithheld);

			return bdr.ToString();
		}
	}
}
