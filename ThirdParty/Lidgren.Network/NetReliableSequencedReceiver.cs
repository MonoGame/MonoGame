using System;

namespace Lidgren.Network
{
	internal sealed class NetReliableSequencedReceiver : NetReceiverChannelBase
	{
		private int m_windowStart;
		private int m_windowSize;

		public NetReliableSequencedReceiver(NetConnection connection, int windowSize)
			: base(connection)
		{
			m_windowSize = windowSize;
		}

		private void AdvanceWindow()
		{
			m_windowStart = (m_windowStart + 1) % NetConstants.NumSequenceNumbers;
		}

		internal override void ReceiveMessage(NetIncomingMessage message)
		{
			int nr = message.m_sequenceNumber;

			int relate = NetUtility.RelativeSequenceNumber(nr, m_windowStart);

			// ack no matter what
			m_connection.QueueAck(message.m_receivedMessageType, nr);

			if (relate == 0)
			{
				// Log("Received message #" + message.SequenceNumber + " right on time");

				//
				// excellent, right on time
				//

				AdvanceWindow();
				m_peer.ReleaseMessage(message);
				return;
			}

			if (relate < 0)
			{
				m_connection.m_statistics.MessageDropped();
				m_peer.LogVerbose("Received message #" + message.m_sequenceNumber + " DROPPING LATE or DUPE");
				return;
			}

			// relate > 0 = early message
			if (relate > m_windowSize)
			{
				// too early message!
				m_connection.m_statistics.MessageDropped();
				m_peer.LogDebug("Received " + message + " TOO EARLY! Expected " + m_windowStart);
				return;
			}

			// ok
			m_windowStart = (m_windowStart + relate) % NetConstants.NumSequenceNumbers;
			m_peer.ReleaseMessage(message);
			return;
		}
	}
}
