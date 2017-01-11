using System;

namespace Lidgren.Network
{
	internal sealed class NetUnreliableUnorderedReceiver : NetReceiverChannelBase
	{
		private bool m_doFlowControl;

		public NetUnreliableUnorderedReceiver(NetConnection connection)
			: base(connection)
		{
			m_doFlowControl = connection.Peer.Configuration.SuppressUnreliableUnorderedAcks == false;
		}

		internal override void ReceiveMessage(NetIncomingMessage msg)
		{
			if (m_doFlowControl)
				m_connection.QueueAck(msg.m_receivedMessageType, msg.m_sequenceNumber);

			m_peer.ReleaseMessage(msg);
		}
	}
}
