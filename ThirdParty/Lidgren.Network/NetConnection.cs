using System;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace Lidgren.Network
{
	/// <summary>
	/// Represents a connection to a remote peer
	/// </summary>
	[DebuggerDisplay("RemoteUniqueIdentifier={RemoteUniqueIdentifier} RemoteEndpoint={RemoteEndpoint}")]
	public partial class NetConnection
	{
		internal NetPeer m_peer;
		internal NetPeerConfiguration m_peerConfiguration;
		internal NetConnectionStatus m_status;
		internal NetConnectionStatus m_visibleStatus;
		internal IPEndPoint m_remoteEndpoint;
		internal NetSenderChannelBase[] m_sendChannels;
		internal NetReceiverChannelBase[] m_receiveChannels;
		internal NetOutgoingMessage m_localHailMessage;
		internal long m_remoteUniqueIdentifier;
		internal NetQueue<NetTuple<NetMessageType, int>> m_queuedAcks;
		private int m_sendBufferWritePtr;
		private int m_sendBufferNumMessages;
		private object m_tag;
		internal NetConnectionStatistics m_statistics;

		/// <summary>
		/// Gets or sets the application defined object containing data about the connection
		/// </summary>
		public object Tag
		{
			get { return m_tag; }
			set { m_tag = value; }
		}

		/// <summary>
		/// Gets the peer which holds this connection
		/// </summary>
		public NetPeer Peer { get { return m_peer; } }

		/// <summary>
		/// Gets the current status of the connection (synced to the last status message read)
		/// </summary>
		public NetConnectionStatus Status { get { return m_visibleStatus; } }

		/// <summary>
		/// Gets various statistics for this connection
		/// </summary>
		public NetConnectionStatistics Statistics { get { return m_statistics; } }

		/// <summary>
		/// Gets the remote endpoint for the connection
		/// </summary>
		public IPEndPoint RemoteEndpoint { get { return m_remoteEndpoint; } }

		/// <summary>
		/// Gets the unique identifier of the remote NetPeer for this connection
		/// </summary>
		public long RemoteUniqueIdentifier { get { return m_remoteUniqueIdentifier; } }

		/// <summary>
		/// Gets the local hail message that was sent as part of the handshake
		/// </summary>
		public NetOutgoingMessage LocalHailMessage { get { return m_localHailMessage; } }

		// gets the time before automatically resending an unacked message
		internal float GetResendDelay()
		{
			float avgRtt = m_averageRoundtripTime;
			if (avgRtt <= 0)
				avgRtt = 0.1f; // "default" resend is based on 100 ms roundtrip time
			return 0.02f + (avgRtt * 2.0f); // 20 ms + double rtt
		}

		internal NetConnection(NetPeer peer, IPEndPoint remoteEndpoint)
		{
			m_peer = peer;
			m_peerConfiguration = m_peer.Configuration;
			m_status = NetConnectionStatus.None;
			m_visibleStatus = NetConnectionStatus.None;
			m_remoteEndpoint = remoteEndpoint;
			m_sendChannels = new NetSenderChannelBase[NetConstants.NumTotalChannels];
			m_receiveChannels = new NetReceiverChannelBase[NetConstants.NumTotalChannels];
			m_queuedAcks = new NetQueue<NetTuple<NetMessageType, int>>(4);
			m_statistics = new NetConnectionStatistics(this);
			m_averageRoundtripTime = -1.0f;
			m_currentMTU = m_peerConfiguration.MaximumTransmissionUnit;
		}

		/// <summary>
		/// Change the internal endpoint to this new one. Used when, during handshake, a switch in port is detected (due to NAT)
		/// </summary>
		internal void MutateEndpoint(IPEndPoint endpoint)
		{
			m_remoteEndpoint = endpoint;

		}

		internal void SetStatus(NetConnectionStatus status, string reason)
		{
			// user or library thread

			if (status == m_status)
				return;

			m_status = status;
			if (reason == null)
				reason = string.Empty;

			if (m_status == NetConnectionStatus.Connected)
			{
				m_timeoutDeadline = (float)NetTime.Now + m_peerConfiguration.m_connectionTimeout;
				m_peer.LogVerbose("Timeout deadline initialized to  " + m_timeoutDeadline);
			}

			if (m_peerConfiguration.IsMessageTypeEnabled(NetIncomingMessageType.StatusChanged))
			{
				NetIncomingMessage info = m_peer.CreateIncomingMessage(NetIncomingMessageType.StatusChanged, 4 + reason.Length + (reason.Length > 126 ? 2 : 1));
				info.m_senderConnection = this;
				info.m_senderEndpoint = m_remoteEndpoint;
				info.Write((byte)m_status);
				info.Write(reason);
				m_peer.ReleaseMessage(info);
			}
			else
			{
				// app dont want those messages, update visible status immediately
				m_visibleStatus = m_status;
			}
		}

		internal void Heartbeat(float now, uint frameCounter)
		{
			m_peer.VerifyNetworkThread();

			NetException.Assert(m_status != NetConnectionStatus.InitiatedConnect && m_status != NetConnectionStatus.RespondedConnect);

			if ((frameCounter % 5) == 0)
			{
				if (now > m_timeoutDeadline)
				{
					//
					// connection timed out
					//
					m_peer.LogVerbose("Connection timed out at " + now + " deadline was " + m_timeoutDeadline);
					ExecuteDisconnect("Connection timed out", true);
				}

				// send ping?
				if (m_status == NetConnectionStatus.Connected)
				{
					if (now > m_sentPingTime + m_peer.m_configuration.m_pingInterval)
						SendPing();

					// handle expand mtu
					MTUExpansionHeartbeat(now);
				}

				if (m_disconnectRequested)
				{
					ExecuteDisconnect(m_disconnectMessage, true);
					return;
				}
			}

			bool connectionReset; // TODO: handle connection reset

			//
			// Note: at this point m_sendBufferWritePtr and m_sendBufferNumMessages may be non-null; resends may already be queued up
			//

			byte[] sendBuffer = m_peer.m_sendBuffer;
			int mtu = m_currentMTU;

			if ((frameCounter % 3) == 0) // coalesce a few frames
			{
				//
				// send ack messages
				//
				while (m_queuedAcks.Count > 0)
				{
					int acks = (mtu - (m_sendBufferWritePtr + 5)) / 3; // 3 bytes per actual ack
					if (acks > m_queuedAcks.Count)
						acks = m_queuedAcks.Count;

					NetException.Assert(acks > 0);

					m_sendBufferNumMessages++;

					// write acks header
					sendBuffer[m_sendBufferWritePtr++] = (byte)NetMessageType.Acknowledge;
					sendBuffer[m_sendBufferWritePtr++] = 0; // no sequence number
					sendBuffer[m_sendBufferWritePtr++] = 0; // no sequence number
					int len = (acks * 3) * 8; // bits
					sendBuffer[m_sendBufferWritePtr++] = (byte)len;
					sendBuffer[m_sendBufferWritePtr++] = (byte)(len >> 8);

					// write acks
					for (int i = 0; i < acks; i++)
					{
						NetTuple<NetMessageType, int> tuple;
						m_queuedAcks.TryDequeue(out tuple);

						//m_peer.LogVerbose("Sending ack for " + tuple.Item1 + "#" + tuple.Item2);

						sendBuffer[m_sendBufferWritePtr++] = (byte)tuple.Item1;
						sendBuffer[m_sendBufferWritePtr++] = (byte)tuple.Item2;
						sendBuffer[m_sendBufferWritePtr++] = (byte)(tuple.Item2 >> 8);
					}

					if (m_queuedAcks.Count > 0)
					{
						// send packet and go for another round of acks
						NetException.Assert(m_sendBufferWritePtr > 0 && m_sendBufferNumMessages > 0);
						m_peer.SendPacket(m_sendBufferWritePtr, m_remoteEndpoint, m_sendBufferNumMessages, out connectionReset);
						m_statistics.PacketSent(m_sendBufferWritePtr, 1);
						m_sendBufferWritePtr = 0;
						m_sendBufferNumMessages = 0;
					}
				}
			}

			//
			// send queued messages
			//
			for (int i = m_sendChannels.Length - 1; i >= 0; i--)    // Reverse order so reliable messages are sent first
			{
				var channel = m_sendChannels[i];
				NetException.Assert(m_sendBufferWritePtr < 1 || m_sendBufferNumMessages > 0);
				if (channel != null)
					channel.SendQueuedMessages(now);
				NetException.Assert(m_sendBufferWritePtr < 1 || m_sendBufferNumMessages > 0);
			}

			//
			// Put on wire data has been written to send buffer but not yet sent
			//
			if (m_sendBufferWritePtr > 0)
			{
				m_peer.VerifyNetworkThread();
				NetException.Assert(m_sendBufferWritePtr > 0 && m_sendBufferNumMessages > 0);
				m_peer.SendPacket(m_sendBufferWritePtr, m_remoteEndpoint, m_sendBufferNumMessages, out connectionReset);
				m_statistics.PacketSent(m_sendBufferWritePtr, m_sendBufferNumMessages);
				m_sendBufferWritePtr = 0;
				m_sendBufferNumMessages = 0;
			}
		}
		
		// Queue an item for immediate sending on the wire
		// This method is called from the ISenderChannels
		internal void QueueSendMessage(NetOutgoingMessage om, int seqNr)
		{
			m_peer.VerifyNetworkThread();

			int sz = om.GetEncodedSize();
			if (sz > m_currentMTU)
				m_peer.LogWarning("Message larger than MTU! Fragmentation must have failed!");

			if (m_sendBufferWritePtr + sz > m_currentMTU)
			{
				bool connReset; // TODO: handle connection reset
				NetException.Assert(m_sendBufferWritePtr > 0 && m_sendBufferNumMessages > 0); // or else the message should have been fragmented earlier
				m_peer.SendPacket(m_sendBufferWritePtr, m_remoteEndpoint, m_sendBufferNumMessages, out connReset);
				m_statistics.PacketSent(m_sendBufferWritePtr, m_sendBufferNumMessages);
				m_sendBufferWritePtr = 0;
				m_sendBufferNumMessages = 0;
			}

			m_sendBufferWritePtr = om.Encode(m_peer.m_sendBuffer, m_sendBufferWritePtr, seqNr);
			m_sendBufferNumMessages++;

			NetException.Assert(m_sendBufferWritePtr > 0, "Encoded zero size message?");
			NetException.Assert(m_sendBufferNumMessages > 0);
		}

		/// <summary>
		/// Send a message to this remote connection
		/// </summary>
		/// <param name="msg">The message to send</param>
		/// <param name="method">How to deliver the message</param>
		/// <param name="sequenceChannel">Sequence channel within the delivery method</param>
		public NetSendResult SendMessage(NetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
		{
			return m_peer.SendMessage(msg, this, method, sequenceChannel);
		}

		// called by SendMessage() and NetPeer.SendMessage; ie. may be user thread
		internal NetSendResult EnqueueMessage(NetOutgoingMessage msg, NetDeliveryMethod method, int sequenceChannel)
		{
			NetMessageType tp = (NetMessageType)((int)method + sequenceChannel);
			msg.m_messageType = tp;

			// TODO: do we need to make this more thread safe?
			int channelSlot = (int)method - 1 + sequenceChannel;
			NetSenderChannelBase chan = m_sendChannels[channelSlot];
			if (chan == null)
				chan = CreateSenderChannel(tp);

			if (msg.GetEncodedSize() > m_currentMTU)
				throw new NetException("Message too large! Fragmentation failure?");

			return chan.Enqueue(msg);
		}

		// may be on user thread
		private NetSenderChannelBase CreateSenderChannel(NetMessageType tp)
		{
			NetSenderChannelBase chan;
			NetDeliveryMethod method = NetUtility.GetDeliveryMethod(tp);
			int sequenceChannel = (int)tp - (int)method;
			switch (method)
			{
				case NetDeliveryMethod.Unreliable:
				case NetDeliveryMethod.UnreliableSequenced:
					chan = new NetUnreliableSenderChannel(this, NetUtility.GetWindowSize(method));
					break;
				case NetDeliveryMethod.ReliableOrdered:
					chan = new NetReliableSenderChannel(this, NetUtility.GetWindowSize(method));
					break;
				case NetDeliveryMethod.ReliableSequenced:
				case NetDeliveryMethod.ReliableUnordered:
				default:
					chan = new NetReliableSenderChannel(this, NetUtility.GetWindowSize(method));
					break;
			}

			int channelSlot = (int)method - 1 + sequenceChannel;
			NetException.Assert(m_sendChannels[channelSlot] == null);
			m_sendChannels[channelSlot] = chan;

			return chan;
		}

		// received a library message while Connected
		internal void ReceivedLibraryMessage(NetMessageType tp, int ptr, int payloadLength)
		{
			m_peer.VerifyNetworkThread();

			float now = (float)NetTime.Now;

			switch (tp)
			{
				case NetMessageType.Disconnect:
					NetIncomingMessage msg = m_peer.SetupReadHelperMessage(ptr, payloadLength);
					ExecuteDisconnect(msg.ReadString(), false);
					break;
				case NetMessageType.Acknowledge:
					for (int i = 0; i < payloadLength; i+=3)
					{
						NetMessageType acktp = (NetMessageType)m_peer.m_receiveBuffer[ptr++]; // netmessagetype
						int seqNr = m_peer.m_receiveBuffer[ptr++];
						seqNr |= (m_peer.m_receiveBuffer[ptr++] << 8);

						NetSenderChannelBase chan = m_sendChannels[(int)acktp - 1];
						if (chan == null)
							chan = CreateSenderChannel(acktp);

						//m_peer.LogVerbose("Received ack for " + acktp + "#" + seqNr);

						chan.ReceiveAcknowledge(now, seqNr);
					}
					break;
				case NetMessageType.Ping:
					int pingNr = m_peer.m_receiveBuffer[ptr++];
					SendPong(pingNr);
					break;
				case NetMessageType.Pong:
					NetIncomingMessage pmsg = m_peer.SetupReadHelperMessage(ptr, payloadLength);
					int pongNr = pmsg.ReadByte();
					float remoteSendTime = pmsg.ReadSingle();
					ReceivedPong(now, pongNr, remoteSendTime);
					break;
				case NetMessageType.ExpandMTURequest:
					SendMTUSuccess(payloadLength);
					break;
				case NetMessageType.ExpandMTUSuccess:
					NetIncomingMessage emsg = m_peer.SetupReadHelperMessage(ptr, payloadLength);
					int size = emsg.ReadInt32();
					HandleExpandMTUSuccess(now, size);
					break;
				default:
					m_peer.LogWarning("Connection received unhandled library message: " + tp);
					break;
			}
		}

		internal void ReceivedMessage(NetIncomingMessage msg)
		{
			m_peer.VerifyNetworkThread();

			NetMessageType tp = msg.m_receivedMessageType;

			int channelSlot = (int)tp - 1;
			NetReceiverChannelBase chan = m_receiveChannels[channelSlot];
			if (chan == null)
				chan = CreateReceiverChannel(tp);

			chan.ReceiveMessage(msg);
		}

		private NetReceiverChannelBase CreateReceiverChannel(NetMessageType tp)
		{
			m_peer.VerifyNetworkThread();

			// create receiver channel
			NetReceiverChannelBase chan;
			NetDeliveryMethod method = NetUtility.GetDeliveryMethod(tp);
			switch (method)
			{
				case NetDeliveryMethod.Unreliable:
					chan = new NetUnreliableUnorderedReceiver(this);
					break;
				case NetDeliveryMethod.ReliableOrdered:
					chan = new NetReliableOrderedReceiver(this, NetConstants.ReliableOrderedWindowSize);
					break;
				case NetDeliveryMethod.UnreliableSequenced:
					chan = new NetUnreliableSequencedReceiver(this);
					break;
				case NetDeliveryMethod.ReliableUnordered:
					chan = new NetReliableUnorderedReceiver(this, NetConstants.ReliableOrderedWindowSize);
					break;
				case NetDeliveryMethod.ReliableSequenced:
					chan = new NetReliableSequencedReceiver(this, NetConstants.ReliableSequencedWindowSize);
					break;
				default:
					throw new NetException("Unhandled NetDeliveryMethod!");
			}

			int channelSlot = (int)tp - 1;
			NetException.Assert(m_receiveChannels[channelSlot] == null);
			m_receiveChannels[channelSlot] = chan;

			return chan;
		}

		internal void QueueAck(NetMessageType tp, int sequenceNumber)
		{
			m_queuedAcks.Enqueue(new NetTuple<NetMessageType, int>(tp, sequenceNumber));
		}

		/// <summary>
		/// Zero windowSize indicates that the channel is not yet instantiated (used)
		/// Negative freeWindowSlots means this amount of messages are currently queued but delayed due to closed window
		/// </summary>
		public void GetSendQueueInfo(NetDeliveryMethod method, int sequenceChannel, out int windowSize, out int freeWindowSlots)
		{
			int channelSlot = (int)method - 1 + sequenceChannel;
			var chan = m_sendChannels[channelSlot];
			if (chan == null)
			{
				windowSize = NetUtility.GetWindowSize(method);
				freeWindowSlots = windowSize;
				return;
			}

			windowSize = chan.WindowSize;
			freeWindowSlots = chan.GetAllowedSends() - chan.m_queuedSends.Count;
			return;
		}

		internal void Shutdown(string reason)
		{
			ExecuteDisconnect(reason, true);
		}

		/// <summary>
		/// Returns a string that represents this object
		/// </summary>
		public override string ToString()
		{
			return "[NetConnection to " + m_remoteEndpoint + "]";
		}
	}
}
