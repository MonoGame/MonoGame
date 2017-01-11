using System;
using System.Net;
using System.Threading;

#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
#endif

namespace Lidgren.Network
{
	public partial class NetPeer
	{
		/// <summary>
		/// Emit a discovery signal to all hosts on your subnet
		/// </summary>
		public void DiscoverLocalPeers(int serverPort)
		{
			NetOutgoingMessage um = CreateMessage(0);
			um.m_messageType = NetMessageType.Discovery;
			Interlocked.Increment(ref um.m_recyclingCount);

			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(new NetEndPoint(NetUtility.GetBroadcastAddress(), serverPort), um));
		}

		/// <summary>
		/// Emit a discovery signal to a single known host
		/// </summary>
		public bool DiscoverKnownPeer(string host, int serverPort)
		{
			var address = NetUtility.Resolve(host);
			if (address == null)
				return false;
			DiscoverKnownPeer(new NetEndPoint(address, serverPort));
			return true;
		}

		/// <summary>
		/// Emit a discovery signal to a single known host
		/// </summary>
		public void DiscoverKnownPeer(NetEndPoint endPoint)
		{
			NetOutgoingMessage om = CreateMessage(0);
			om.m_messageType = NetMessageType.Discovery;
			om.m_recyclingCount = 1;
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(endPoint, om));
		}

		/// <summary>
		/// Send a discovery response message
		/// </summary>
		public void SendDiscoveryResponse(NetOutgoingMessage msg, NetEndPoint recipient)
		{
			if (recipient == null)
				throw new ArgumentNullException("recipient");

			if (msg == null)
				msg = CreateMessage(0);
			else if (msg.m_isSent)
				throw new NetException("Message has already been sent!");

			if (msg.LengthBytes >= m_configuration.MaximumTransmissionUnit)
				throw new NetException("Cannot send discovery message larger than MTU (currently " + m_configuration.MaximumTransmissionUnit + " bytes)");

			msg.m_messageType = NetMessageType.DiscoveryResponse;
			Interlocked.Increment(ref msg.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(recipient, msg));
		}
	}
}
