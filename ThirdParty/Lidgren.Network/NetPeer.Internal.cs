#if !ANDROID && !IOS && !PSS
#define IS_FULL_NET_AVAILABLE
#endif

using System;
using System.Net;
using System.Threading;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Lidgren.Network
{
	public partial class NetPeer
	{
		private NetPeerStatus m_status;
		private Thread m_networkThread;
		private Socket m_socket;
		internal byte[] m_sendBuffer;
		internal byte[] m_receiveBuffer;
		internal NetIncomingMessage m_readHelperMessage;
		private EndPoint m_senderRemote;
		private object m_initializeLock = new object();
		private uint m_frameCounter;
		private double m_lastHeartbeat;
		private NetUPnP m_upnp;

		internal readonly NetPeerConfiguration m_configuration;
		private readonly NetQueue<NetIncomingMessage> m_releasedIncomingMessages;
		internal readonly NetQueue<NetTuple<IPEndPoint, NetOutgoingMessage>> m_unsentUnconnectedMessages;

		internal Dictionary<IPEndPoint, NetConnection> m_handshakes;

		internal readonly NetPeerStatistics m_statistics;
		internal long m_uniqueIdentifier;

		private AutoResetEvent m_messageReceivedEvent = new AutoResetEvent(false);
		private List<NetTuple<SynchronizationContext, SendOrPostCallback>> m_receiveCallbacks;

		/// <summary>
		/// Gets the socket, if Start() has been called
		/// </summary>
		public Socket Socket { get { return m_socket; } }

		/// <summary>
		/// Call this to register a callback for when a new message arrives
		/// </summary>
		public void RegisterReceivedCallback(SendOrPostCallback callback)
		{
			if (m_receiveCallbacks == null)
				m_receiveCallbacks = new List<NetTuple<SynchronizationContext, SendOrPostCallback>>();
			m_receiveCallbacks.Add(new NetTuple<SynchronizationContext, SendOrPostCallback>(SynchronizationContext.Current, callback));
		}

		internal void ReleaseMessage(NetIncomingMessage msg)
		{
			NetException.Assert(msg.m_incomingMessageType != NetIncomingMessageType.Error);

			if (msg.m_isFragment)
			{
				HandleReleasedFragment(msg);
				return;
			}
			
			m_releasedIncomingMessages.Enqueue(msg);

			if (m_messageReceivedEvent != null)
				m_messageReceivedEvent.Set();

			if (m_receiveCallbacks != null)
			{
				foreach (var tuple in m_receiveCallbacks)
					tuple.Item1.Post(tuple.Item2, this);
			}
		}

		private void InitializeNetwork()
		{
			lock (m_initializeLock)
			{
				m_configuration.Lock();

				if (m_status == NetPeerStatus.Running)
					return;

				if (m_configuration.m_enableUPnP)
					m_upnp = new NetUPnP(this);

				InitializePools();

				m_releasedIncomingMessages.Clear();
				m_unsentUnconnectedMessages.Clear();
				m_handshakes.Clear();

				// bind to socket
				IPEndPoint iep = null;

				iep = new IPEndPoint(m_configuration.LocalAddress, m_configuration.Port);
				EndPoint ep = (EndPoint)iep;

				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				m_socket.ReceiveBufferSize = m_configuration.ReceiveBufferSize;
				m_socket.SendBufferSize = m_configuration.SendBufferSize;
				m_socket.Blocking = false;
				m_socket.Bind(ep);

				IPEndPoint boundEp = m_socket.LocalEndPoint as IPEndPoint;
				LogDebug("Socket bound to " + boundEp + ": " + m_socket.IsBound);
				m_listenPort = boundEp.Port;

				m_receiveBuffer = new byte[m_configuration.ReceiveBufferSize];
				m_sendBuffer = new byte[m_configuration.SendBufferSize];
				m_readHelperMessage = new NetIncomingMessage(NetIncomingMessageType.Error);
				m_readHelperMessage.m_data = m_receiveBuffer;

				byte[] macBytes = new byte[8];
				NetRandom.Instance.NextBytes(macBytes);

#if IS_FULL_NET_AVAILABLE
				try
				{
					System.Net.NetworkInformation.PhysicalAddress pa = NetUtility.GetMacAddress();
					if (pa != null)
					{
						macBytes = pa.GetAddressBytes();
						LogVerbose("Mac address is " + NetUtility.ToHexString(macBytes));
					}
					else
					{
						LogWarning("Failed to get Mac address");
					}
				}
				catch (NotSupportedException)
				{
					// not supported; lets just keep the random bytes set above
				}
#endif
				byte[] epBytes = BitConverter.GetBytes(boundEp.GetHashCode());
				byte[] combined = new byte[epBytes.Length + macBytes.Length];
				Array.Copy(epBytes, 0, combined, 0, epBytes.Length);
				Array.Copy(macBytes, 0, combined, epBytes.Length, macBytes.Length);
				m_uniqueIdentifier = BitConverter.ToInt64(SHA1.Create().ComputeHash(combined), 0);

				m_status = NetPeerStatus.Running;
			}
		}

		private void NetworkLoop()
		{
			VerifyNetworkThread();

			LogDebug("Network thread started");

			//
			// Network loop
			//
			do
			{
				try
				{
					Heartbeat();
				}
				catch (Exception ex)
				{
					LogWarning(ex.ToString());
				}
			} while (m_status == NetPeerStatus.Running);

			//
			// perform shutdown
			//
			ExecutePeerShutdown();
		}

		private void ExecutePeerShutdown()
		{
			VerifyNetworkThread();

			LogDebug("Shutting down...");

			// disconnect and make one final heartbeat
			var list = new List<NetConnection>(m_handshakes.Count + m_connections.Count);
			lock (m_connections)
			{
				foreach (var conn in m_connections)
					if (conn != null)
						list.Add(conn);

				lock (m_handshakes)
				{
					foreach (var hs in m_handshakes.Values)
						if (hs != null)
							list.Add(hs);

					// shut down connections
					foreach (NetConnection conn in list)
						conn.Shutdown(m_shutdownReason);
				}
			}

			// one final heartbeat, will send stuff and do disconnect
			Heartbeat();

			lock (m_initializeLock)
			{
				try
				{
					if (m_socket != null)
					{
						// Wrapped this in a try for MonoGame.  I think we are bombing here
						//  because there are no clients connecting during tests.
						try {
							m_socket.Shutdown(SocketShutdown.Receive);
						}
						catch (Exception socketException) {
							LogDebug("Exception trying to Shutdown Socket: " + socketException.Message);
						}
						m_socket.Close(2); // 2 seconds timeout
					}
					if (m_messageReceivedEvent != null)
					{
						m_messageReceivedEvent.Set();
						m_messageReceivedEvent.Close();
						m_messageReceivedEvent = null;
					}
				}
				finally
				{
					m_socket = null;
					m_status = NetPeerStatus.NotRunning;
					LogDebug("Shutdown complete");
				}

				m_receiveBuffer = null;
				m_sendBuffer = null;
				m_unsentUnconnectedMessages.Clear();
				m_connections.Clear();
				m_handshakes.Clear();
			}

			return;
		}

		private void Heartbeat()
		{
			VerifyNetworkThread();

			double dnow = NetTime.Now;
			float now = (float)dnow;

			double delta = dnow - m_lastHeartbeat;

			int maxCHBpS = 1250 - m_connections.Count;
			if (maxCHBpS < 250)
				maxCHBpS = 250;
			if (delta > (1.0 / (double)maxCHBpS)) // max connection heartbeats/second max
			{
				m_frameCounter++;
				m_lastHeartbeat = dnow;

				// do handshake heartbeats
				if ((m_frameCounter % 3) == 0)
				{
					foreach (var kvp in m_handshakes)
					{
						NetConnection conn = kvp.Value as NetConnection;
#if DEBUG
						// sanity check
						if (kvp.Key != kvp.Key)
							LogWarning("Sanity fail! Connection in handshake list under wrong key!");
#endif
						conn.UnconnectedHeartbeat(now);
						if (conn.m_status == NetConnectionStatus.Connected || conn.m_status == NetConnectionStatus.Disconnected)
						{
#if DEBUG
							// sanity check
							if (conn.m_status == NetConnectionStatus.Disconnected && m_handshakes.ContainsKey(conn.RemoteEndpoint))
							{
								LogWarning("Sanity fail! Handshakes list contained disconnected connection!");
								m_handshakes.Remove(conn.RemoteEndpoint);
							}
#endif
							break; // collection has been modified
						}
					}
				}

#if DEBUG
			SendDelayedPackets();
#endif

				// do connection heartbeats
				lock (m_connections)
				{
					foreach (NetConnection conn in m_connections)
					{
						conn.Heartbeat(now, m_frameCounter);
						if (conn.m_status == NetConnectionStatus.Disconnected)
						{
							//
							// remove connection
							//
							m_connections.Remove(conn);
							m_connectionLookup.Remove(conn.RemoteEndpoint);
							break; // can't continue iteration here
						}
					}
				}

				// send unsent unconnected messages
				NetTuple<IPEndPoint, NetOutgoingMessage> unsent;
				while (m_unsentUnconnectedMessages.TryDequeue(out unsent))
				{
					NetOutgoingMessage om = unsent.Item2;
#if DEBUG
                    if (om.m_messageType == NetMessageType.NatPunchMessage)
                    {
                        LogDebug("Sending Nat Punch Message to " + unsent.Item1.ToString());
                    }
#endif

					bool connReset;
					int len = om.Encode(m_sendBuffer, 0, 0);
					SendPacket(len, unsent.Item1, 1, out connReset);

					Interlocked.Decrement(ref om.m_recyclingCount);
					if (om.m_recyclingCount <= 0)
						Recycle(om);
				}
			}

			//
			// read from socket
			//
			if (m_socket == null)
				return;

			if (!m_socket.Poll(1000, SelectMode.SelectRead)) // wait up to 1 ms for data to arrive
				return;

			//if (m_socket == null || m_socket.Available < 1)
			//	return;

			do
			{
				int bytesReceived = 0;
				try
				{
					bytesReceived = m_socket.ReceiveFrom(m_receiveBuffer, 0, m_receiveBuffer.Length, SocketFlags.None, ref m_senderRemote);
				}
				catch (SocketException sx)
				{
					if (sx.SocketErrorCode == SocketError.ConnectionReset)
					{
						// connection reset by peer, aka connection forcibly closed aka "ICMP port unreachable" 
						// we should shut down the connection; but m_senderRemote seemingly cannot be trusted, so which connection should we shut down?!
						// So, what to do?
						return;
					}

					LogWarning(sx.ToString());
					return;
				}

				if (bytesReceived < NetConstants.HeaderByteSize)
					return;

				//LogVerbose("Received " + bytesReceived + " bytes");

				IPEndPoint ipsender = (IPEndPoint)m_senderRemote;

				if (ipsender.Port == 1900)
				{
					// UPnP response
					try
					{
						string resp = System.Text.Encoding.UTF8.GetString(m_receiveBuffer, 0, bytesReceived);
						if (resp.Contains("upnp:rootdevice"))
						{
							resp = resp.Substring(resp.ToLower().IndexOf("location:") + 9);
							resp = resp.Substring(0, resp.IndexOf("\r")).Trim();
							m_upnp.ExtractServiceUrl(resp);
							return;
						}
					}
					catch { }
				}

				NetConnection sender = null;
				m_connectionLookup.TryGetValue(ipsender, out sender);

				double receiveTime = NetTime.Now;
				//
				// parse packet into messages
				//
				int numMessages = 0;
				int ptr = 0;
				while ((bytesReceived - ptr) >= NetConstants.HeaderByteSize)
				{
					// decode header
					//  8 bits - NetMessageType
					//  1 bit  - Fragment?
					// 15 bits - Sequence number
					// 16 bits - Payload length in bits

					numMessages++;

					NetMessageType tp = (NetMessageType)m_receiveBuffer[ptr++];

					byte low = m_receiveBuffer[ptr++];
					byte high = m_receiveBuffer[ptr++];

					bool isFragment = ((low & 1) == 1);
					ushort sequenceNumber = (ushort)((low >> 1) | (((int)high) << 7));

					ushort payloadBitLength = (ushort)(m_receiveBuffer[ptr++] | (m_receiveBuffer[ptr++] << 8));
					int payloadByteLength = NetUtility.BytesToHoldBits(payloadBitLength);

					if (bytesReceived - ptr < payloadByteLength)
					{
						LogWarning("Malformed packet; stated payload length " + payloadByteLength + ", remaining bytes " + (bytesReceived - ptr));
						return;
					}

					try
					{
						NetException.Assert(tp < NetMessageType.Unused1 || tp > NetMessageType.Unused29);

						if (tp >= NetMessageType.LibraryError)
						{
							if (sender != null)
								sender.ReceivedLibraryMessage(tp, ptr, payloadByteLength);
							else
								ReceivedUnconnectedLibraryMessage(receiveTime, ipsender, tp, ptr, payloadByteLength);
						}
						else
						{
							if (sender == null && !m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData))
								return; // dropping unconnected message since it's not enabled

							NetIncomingMessage msg = CreateIncomingMessage(NetIncomingMessageType.Data, payloadByteLength);
							msg.m_isFragment = isFragment;
							msg.m_receiveTime = receiveTime;
							msg.m_sequenceNumber = sequenceNumber;
							msg.m_receivedMessageType = tp;
							msg.m_senderConnection = sender;
							msg.m_senderEndpoint = ipsender;
							msg.m_bitLength = payloadBitLength;
							Buffer.BlockCopy(m_receiveBuffer, ptr, msg.m_data, 0, payloadByteLength);
							if (sender != null)
							{
								if (tp == NetMessageType.Unconnected)
								{
									// We're connected; but we can still send unconnected messages to this peer
									msg.m_incomingMessageType = NetIncomingMessageType.UnconnectedData;
									ReleaseMessage(msg);
								}
								else
								{
									// connected application (non-library) message
									sender.ReceivedMessage(msg);
								}
							}
							else
							{
								// at this point we know the message type is enabled
								// unconnected application (non-library) message
								msg.m_incomingMessageType = NetIncomingMessageType.UnconnectedData;
								ReleaseMessage(msg);
							}
						}
					}
					catch (Exception ex)
					{
						LogError("Packet parsing error: " + ex.Message + " from " + ipsender);
					}
					ptr += payloadByteLength;
				}

				m_statistics.PacketReceived(bytesReceived, numMessages);
				if (sender != null)
					sender.m_statistics.PacketReceived(bytesReceived, numMessages);

			} while (m_socket.Available > 0);
		}

		internal void HandleIncomingDiscoveryRequest(double now, IPEndPoint senderEndpoint, int ptr, int payloadByteLength)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryRequest))
			{
				NetIncomingMessage dm = CreateIncomingMessage(NetIncomingMessageType.DiscoveryRequest, payloadByteLength);
				if (payloadByteLength > 0)
					Buffer.BlockCopy(m_receiveBuffer, ptr, dm.m_data, 0, payloadByteLength);
				dm.m_receiveTime = now;
				dm.m_bitLength = payloadByteLength * 8;
				dm.m_senderEndpoint = senderEndpoint;
				ReleaseMessage(dm);
			}
		}

		internal void HandleIncomingDiscoveryResponse(double now, IPEndPoint senderEndpoint, int ptr, int payloadByteLength)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryResponse))
			{
				NetIncomingMessage dr = CreateIncomingMessage(NetIncomingMessageType.DiscoveryResponse, payloadByteLength);
				if (payloadByteLength > 0)
					Buffer.BlockCopy(m_receiveBuffer, ptr, dr.m_data, 0, payloadByteLength);
				dr.m_receiveTime = now;
				dr.m_bitLength = payloadByteLength * 8;
				dr.m_senderEndpoint = senderEndpoint;
				ReleaseMessage(dr);
			}
		}

		private void ReceivedUnconnectedLibraryMessage(double now, IPEndPoint senderEndpoint, NetMessageType tp, int ptr, int payloadByteLength)
		{
			NetConnection shake;
			if (m_handshakes.TryGetValue(senderEndpoint, out shake))
			{
				shake.ReceivedHandshake(now, tp, ptr, payloadByteLength);
				return;
			}

			//
			// Library message from a completely unknown sender; lets just accept Connect
			//
			switch (tp)
			{
				case NetMessageType.Discovery:
					HandleIncomingDiscoveryRequest(now, senderEndpoint, ptr, payloadByteLength);
					return;
				case NetMessageType.DiscoveryResponse:
					HandleIncomingDiscoveryResponse(now, senderEndpoint, ptr, payloadByteLength);
					return;
				case NetMessageType.NatIntroduction:
					HandleNatIntroduction(ptr);
					return;
				case NetMessageType.NatPunchMessage:
					HandleNatPunch(ptr, senderEndpoint);
					return;
				case NetMessageType.ConnectResponse:

					lock (m_handshakes)
					{
						foreach (var hs in m_handshakes)
						{
							if (hs.Key.Address.Equals(senderEndpoint.Address))
							{
								if (hs.Value.m_connectionInitiator)
								{
									//
									// We are currently trying to connection to XX.XX.XX.XX:Y
									// ... but we just received a ConnectResponse from XX.XX.XX.XX:Z
									// Lets just assume the router decided to use this port instead
									//
									var hsconn = hs.Value;
									m_connectionLookup.Remove(hs.Key);
									m_handshakes.Remove(hs.Key);

									LogDebug("Detected host port change; rerouting connection to " + senderEndpoint);
									hsconn.MutateEndpoint(senderEndpoint);

									m_connectionLookup.Add(senderEndpoint, hsconn);
									m_handshakes.Add(senderEndpoint, hsconn);

									hsconn.ReceivedHandshake(now, tp, ptr, payloadByteLength);
									return;
								}
							}
						}
					}

					LogWarning("Received unhandled library message " + tp + " from " + senderEndpoint);
					return;
				case NetMessageType.Connect:
					// proceed
					break;
				case NetMessageType.Disconnect:
					// this is probably ok
					LogVerbose("Received Disconnect from unconnected source: " + senderEndpoint);
					return;
				default:
					LogWarning("Received unhandled library message " + tp + " from " + senderEndpoint);
					return;
			}

			// It's someone wanting to shake hands with us!

			int reservedSlots = m_handshakes.Count + m_connections.Count;
			if (reservedSlots >= m_configuration.m_maximumConnections)
			{
				// server full
				NetOutgoingMessage full = CreateMessage("Server full");
				full.m_messageType = NetMessageType.Disconnect;
				SendLibrary(full, senderEndpoint);
				return;
			}

			// Ok, start handshake!
			NetConnection conn = new NetConnection(this, senderEndpoint);
			m_handshakes.Add(senderEndpoint, conn);
			conn.ReceivedHandshake(now, tp, ptr, payloadByteLength);

			return;
		}

		internal void AcceptConnection(NetConnection conn)
		{
			// LogDebug("Accepted connection " + conn);
			conn.InitExpandMTU(NetTime.Now);

			if (m_handshakes.Remove(conn.m_remoteEndpoint) == false)
				LogWarning("AcceptConnection called but m_handshakes did not contain it!");

			lock (m_connections)
			{
				if (m_connections.Contains(conn))
				{
					LogWarning("AcceptConnection called but m_connection already contains it!");
				}
				else
				{
					m_connections.Add(conn);
					m_connectionLookup.Add(conn.m_remoteEndpoint, conn);
				}
			}
		}

		[Conditional("DEBUG")]
		internal void VerifyNetworkThread()
		{
			Thread ct = Thread.CurrentThread;
			if (Thread.CurrentThread != m_networkThread)
				throw new NetException("Executing on wrong thread! Should be library system thread (is " + ct.Name + " mId " + ct.ManagedThreadId + ")");
		}

		internal NetIncomingMessage SetupReadHelperMessage(int ptr, int payloadLength)
		{
			VerifyNetworkThread();

			m_readHelperMessage.m_bitLength = (ptr + payloadLength) * 8;
			m_readHelperMessage.m_readPosition = (ptr * 8);
			return m_readHelperMessage;
		}
	}
}
