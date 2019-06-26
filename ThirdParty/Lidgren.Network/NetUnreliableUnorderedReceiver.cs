using System;

namespace Lidgren.Network
{
	internal sealed class NetUnreliableUnorderedReceiver : NetReceiverChannelBase
	{
		public NetUnreliableUnorderedReceiver(NetConnection connection)
			: base(connection)
		{
		}

		internal override void ReceiveMessage(NetIncomingMessage msg)
		{
			// ack no matter what
			m_connection.QueueAck(msg.m_receivedMessageType, msg.m_sequenceNumber);

			m_peer.ReleaseMessage(msg);
		}
	}
}
