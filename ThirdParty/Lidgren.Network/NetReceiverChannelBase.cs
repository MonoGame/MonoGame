using System;

namespace Lidgren.Network
{
	internal abstract class NetReceiverChannelBase
	{
		internal NetPeer m_peer;
		internal NetConnection m_connection;

		public NetReceiverChannelBase(NetConnection connection)
		{
			m_connection = connection;
			m_peer = connection.m_peer;
		}

		internal abstract void ReceiveMessage(NetIncomingMessage msg);
	}
}
