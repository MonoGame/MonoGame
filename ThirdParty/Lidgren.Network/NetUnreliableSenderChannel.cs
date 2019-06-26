using System;
using System.Threading;

namespace Lidgren.Network
{
	/// <summary>
	/// Sender part of Selective repeat ARQ for a particular NetChannel
	/// </summary>
	internal sealed class NetUnreliableSenderChannel : NetSenderChannelBase
	{
		private NetConnection m_connection;
		private int m_windowStart;
		private int m_windowSize;
		private int m_sendStart;

		private NetBitVector m_receivedAcks;

		internal override int WindowSize { get { return m_windowSize; } }

		internal NetUnreliableSenderChannel(NetConnection connection, int windowSize)
		{
			m_connection = connection;
			m_windowSize = windowSize;
			m_windowStart = 0;
			m_sendStart = 0;
			m_receivedAcks = new NetBitVector(NetConstants.NumSequenceNumbers);
			m_queuedSends = new NetQueue<NetOutgoingMessage>(8);
		}

		internal override int GetAllowedSends()
		{
			int retval = m_windowSize - ((m_sendStart + NetConstants.NumSequenceNumbers) - m_windowStart) % m_windowSize;
			NetException.Assert(retval >= 0 && retval <= m_windowSize);
			return retval;
		}

		internal override void Reset()
		{
			m_receivedAcks.Clear();
			m_queuedSends.Clear();
			m_windowStart = 0;
			m_sendStart = 0;
		}

		internal override NetSendResult Enqueue(NetOutgoingMessage message)
		{
			int queueLen = m_queuedSends.Count + 1;
			int left = GetAllowedSends();
			if (queueLen > left || (message.LengthBytes > m_connection.m_currentMTU && m_connection.m_peerConfiguration.UnreliableSizeBehaviour == NetUnreliableSizeBehaviour.DropAboveMTU))
			{
				m_connection.Peer.Recycle(message);
				return NetSendResult.Dropped;
			}

			m_queuedSends.Enqueue(message);
			return NetSendResult.Sent;
		}

		// call this regularely
		internal override void SendQueuedMessages(float now)
		{
			int num = GetAllowedSends();
			if (num < 1)
				return;

			// queued sends
			while (m_queuedSends.Count > 0 && num > 0)
			{
				NetOutgoingMessage om;
				if (m_queuedSends.TryDequeue(out om))
					ExecuteSend(om);
				num--;
			}
		}

		private void ExecuteSend(NetOutgoingMessage message)
		{
			m_connection.m_peer.VerifyNetworkThread();

			int seqNr = m_sendStart;
			m_sendStart = (m_sendStart + 1) % NetConstants.NumSequenceNumbers;

			m_connection.QueueSendMessage(message, seqNr);

			Interlocked.Decrement(ref message.m_recyclingCount);
			if (message.m_recyclingCount <= 0)
				m_connection.m_peer.Recycle(message);

			return;
		}
		
		// remoteWindowStart is remote expected sequence number; everything below this has arrived properly
		// seqNr is the actual nr received
		internal override void ReceiveAcknowledge(float now, int seqNr)
		{
			// late (dupe), on time or early ack?
			int relate = NetUtility.RelativeSequenceNumber(seqNr, m_windowStart);

			if (relate < 0)
			{
				//m_connection.m_peer.LogDebug("Received late/dupe ack for #" + seqNr);
				return; // late/duplicate ack
			}

			if (relate == 0)
			{
				//m_connection.m_peer.LogDebug("Received right-on-time ack for #" + seqNr);

				// ack arrived right on time
				NetException.Assert(seqNr == m_windowStart);

				m_receivedAcks[m_windowStart] = false;
				m_windowStart = (m_windowStart + 1) % NetConstants.NumSequenceNumbers;

				return;
			}

			// Advance window to this position
			m_receivedAcks[seqNr] = true;

			while (m_windowStart != seqNr)
			{
				m_receivedAcks[m_windowStart] = false;
				m_windowStart = (m_windowStart + 1) % NetConstants.NumSequenceNumbers;
			}
		}
	}
}
