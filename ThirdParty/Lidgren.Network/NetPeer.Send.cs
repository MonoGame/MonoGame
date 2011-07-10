using System;
using System.Collections.Generic;
using System.Threading;
using System.Net;

namespace Lidgren.Network
{
	public partial class NetPeer
	{
		/// <summary>
		/// Send a message to a specific connection
		/// </summary>
		/// <param name="msg">The message to send</param>
		/// <param name="recipient">The recipient connection</param>
		/// <param name="method">How to deliver the message</param>
		public NetSendResult SendMessage(NetOutgoingMessage msg, NetConnection recipient, NetDeliveryMethod method)
		{
			return SendMessage(msg, recipient, method, 0);
		}

		/// <summary>
		/// Send a message to a specific connection
		/// </summary>
		/// <param name="msg">The message to send</param>
		/// <param name="recipient">The recipient connection</param>
		/// <param name="method">How to deliver the message</param>
		/// <param name="sequenceChannel">Sequence channel within the delivery method</param>
		public NetSendResult SendMessage(NetOutgoingMessage msg, NetConnection recipient, NetDeliveryMethod method, int sequenceChannel)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");
			if (recipient == null)
				throw new ArgumentNullException("recipient");
			if (sequenceChannel >= NetConstants.NetChannelsPerDeliveryMethod)
				throw new ArgumentOutOfRangeException("sequenceChannel");

			NetException.Assert(
				((method != NetDeliveryMethod.Unreliable && method != NetDeliveryMethod.ReliableUnordered) ||
				((method == NetDeliveryMethod.Unreliable || method == NetDeliveryMethod.ReliableUnordered) && sequenceChannel == 0)),
				"Delivery method " + method + " cannot use sequence channels other than 0!"
			);

			NetException.Assert(method != NetDeliveryMethod.Unknown, "Bad delivery method!");

			if (msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			msg.m_isSent = true;

			int len = NetConstants.UnfragmentedMessageHeaderSize + msg.LengthBytes; // headers + length, faster than calling msg.GetEncodedSize
			if (len <= recipient.m_currentMTU)
			{
				Interlocked.Increment(ref msg.m_recyclingCount);
				return recipient.EnqueueMessage(msg, method, sequenceChannel);
			}
			else
			{
				// message must be fragmented!
				SendFragmentedMessage(msg, new NetConnection[] { recipient }, method, sequenceChannel);
				return NetSendResult.Queued; // could be different for each connection; Queued is "most true"
			}
		}

		internal int GetMTU(IList<NetConnection> recipients)
		{
			int mtu = int.MaxValue;
			foreach (NetConnection conn in recipients)
			{
				int cmtu = conn.m_currentMTU;
				if (cmtu < mtu)
					mtu = cmtu;
			}
			return mtu;
		}

		/// <summary>
		/// Send a message to a list of connections
		/// </summary>
		/// <param name="msg">The message to send</param>
		/// <param name="recipients">The list of recipients to send to</param>
		/// <param name="method">How to deliver the message</param>
		/// <param name="sequenceChannel">Sequence channel within the delivery method</param>
		public void SendMessage(NetOutgoingMessage msg, IList<NetConnection> recipients, NetDeliveryMethod method, int sequenceChannel)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");
			if (recipients == null)
				throw new ArgumentNullException("recipients");
			if (method == NetDeliveryMethod.Unreliable || method == NetDeliveryMethod.ReliableUnordered)
				NetException.Assert(sequenceChannel == 0, "Delivery method " + method + " cannot use sequence channels other than 0!");
			if (msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");

			int mtu = GetMTU(recipients);

			msg.m_isSent = true;

			int len = msg.LengthBytes;
			if (len <= m_configuration.MaximumTransmissionUnit)
			{
				Interlocked.Add(ref msg.m_recyclingCount, recipients.Count);
				foreach (NetConnection conn in recipients)
				{
					if (conn == null)
					{
						Interlocked.Decrement(ref msg.m_recyclingCount);
						continue;
					}
					NetSendResult res = conn.EnqueueMessage(msg, method, sequenceChannel);
					if (res == NetSendResult.Dropped)
					{
						LogDebug(msg + " dropped immediately due to full queues");
						Interlocked.Decrement(ref msg.m_recyclingCount);
					}
				}
			}
			else
			{
				// message must be fragmented!
				SendFragmentedMessage(msg, recipients, method, sequenceChannel);
			}

			return;
		}

		/// <summary>
		/// Send a message to an unconnected host
		/// </summary>
		public void SendUnconnectedMessage(NetOutgoingMessage msg, string host, int port)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");
			if (host == null)
				throw new ArgumentNullException("host");
			if (msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			if (msg.LengthBytes > m_configuration.MaximumTransmissionUnit)
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + m_configuration.MaximumTransmissionUnit + ")");

			IPAddress adr = NetUtility.Resolve(host);
			if (adr == null)
				throw new NetException("Failed to resolve " + host);

			msg.m_messageType = NetMessageType.Unconnected;

			Interlocked.Increment(ref msg.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(new IPEndPoint(adr, port), msg));
		}

		/// <summary>
		/// Send a message to an unconnected host
		/// </summary>
		public void SendUnconnectedMessage(NetOutgoingMessage msg, IPEndPoint recipient)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");
			if (recipient == null)
				throw new ArgumentNullException("recipient");
			if (msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			if (msg.LengthBytes > m_configuration.MaximumTransmissionUnit)
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + m_configuration.MaximumTransmissionUnit + ")");

			msg.m_messageType = NetMessageType.Unconnected;
			msg.m_isSent = true;

			Interlocked.Increment(ref msg.m_recyclingCount);
			m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(recipient, msg));
		}

		/// <summary>
		/// Send a message to an unconnected host
		/// </summary>
		public void SendUnconnectedMessage(NetOutgoingMessage msg, IList<IPEndPoint> recipients)
		{
			if (msg == null)
				throw new ArgumentNullException("msg");
			if (recipients == null)
				throw new ArgumentNullException("recipients");
			if (msg.m_isSent)
				throw new NetException("This message has already been sent! Use NetPeer.SendMessage() to send to multiple recipients efficiently");
			if (msg.LengthBytes > m_configuration.MaximumTransmissionUnit)
				throw new NetException("Unconnected messages too long! Must be shorter than NetConfiguration.MaximumTransmissionUnit (currently " + m_configuration.MaximumTransmissionUnit + ")");

			msg.m_messageType = NetMessageType.Unconnected;
			msg.m_isSent = true;

			Interlocked.Add(ref msg.m_recyclingCount, recipients.Count);
			foreach(IPEndPoint ep in recipients)
				m_unsentUnconnectedMessages.Enqueue(new NetTuple<IPEndPoint, NetOutgoingMessage>(ep, msg));
		}
	}
}
