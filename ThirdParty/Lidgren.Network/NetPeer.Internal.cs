#if !__ANDROID__ && !IOS && !UNITY_WEBPLAYER && !UNITY_ANDROID && !UNITY_IPHONE
#define IS_MAC_AVAILABLE
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
		private double m_lastSocketBind = float.MinValue;
		private NetUPnP m_upnp;

		internal readonly NetPeerConfiguration m_configuration;
		private readonly NetQueue<NetIncomingMessage> m_releasedIncomingMessages;
		internal readonly NetQueue<NetTuple<IPEndPoint, NetOutgoingMessage>> m_unsentUnconnectedMessages;

		internal Dictionary<IPEndPoint, NetConnection> m_handshakes;

		internal readonly NetPeerStatistics m_statistics;
		internal long m_uniqueIdentifier;
		internal bool m_executeFlushSendQueue;

		private AutoResetEvent m_messageReceivedEvent;
		private List<NetTuple<SynchronizationContext, SendOrPostCallback>> m_receiveCallbacks;

		/// <summary>
		/// Gets the socket, if Start() has been called
		/// </summary>
		public Socket Socket { get { return m_socket; } }

		/// <summary>
		/// Call this to register a callback for when a new message arrives
		/// </summary>
		public void RegisterReceivedCallback(SendOrPostCallback callback, SynchronizationContext syncContext = null)
		{
			if (syncContext == null)
				syncContext = SynchronizationContext.Current;
			if (syncContext == null)
				throw new NetException("Need a SynchronizationContext to register callback on correct thread!");
			if (m_receiveCallbacks == null)
				m_receiveCallbacks = new List<NetTuple<SynchronizationContext, SendOrPostCallback>>();
			m_receiveCallbacks.Add(new NetTuple<SynchronizationContext, SendOrPostCallback>(syncContext, callback));
		}

		/// <summary>
		/// Call this to unregister a callback, but remember to do it in the same synchronization context!
		/// </summary>
		public void UnregisterReceivedCallback(SendOrPostCallback callback)
		{
			if (m_receiveCallbacks == null)
				return;

			// remove all callbacks regardless of sync context
		RestartRemoveCallbacks:
			for (int i = 0; i < m_receiveCallbacks.Count; i++)
			{
				if (m_receiveCallbacks[i].Item2.Equals(callback))
				{
					m_receiveCallbacks.RemoveAt(i);
					goto RestartRemoveCallbacks;
				}
			}
			if (m_receiveCallbacks.Count < 1)
				m_receiveCallbacks = null;
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
				{
					try
					{
						tuple.Item1.Post(tuple.Item2, this);
					}
					catch (Exception ex)
					{
						LogWarning("Receive callback exception:" + ex);
					}
				}
			}
		}

		private void BindSocket(bool reBind)
		{
			double now = NetTime.Now;
			if (now - m_lastSocketBind < 1.0)
			{
				LogDebug("Suppressed socket rebind; last bound " + (now - m_lastSocketBind) + " seconds ago");
				return; // only allow rebind once every second
			}
			m_lastSocketBind = now;

			if (m_socket == null)
				m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			if (reBind)
				m_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, (int)1); 

			m_socket.ReceiveBufferSize = m_configuration.ReceiveBufferSize;
			m_socket.SendBufferSize = m_configuration.SendBufferSize;
			m_socket.Blocking = false;

			var ep = (EndPoint)new IPEndPoint(m_configuration.LocalAddress, reBind ? m_listenPort : m_configuration.Port);
			m_socket.Bind(ep);

			try
			{
				const uint IOC_IN = 0x80000000;
				const uint IOC_VENDOR = 0x18000000;
				uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
				m_socket.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);
			}
			catch
			{
				// ignore; SIO_UDP_CONNRESET not supported on this platform
			}

			IPEndPoint boundEp = m_socket.LocalEndPoint as IPEndPoint;
			LogDebug("Socket bound to " + boundEp + ": " + m_socket.IsBound);
			m_listenPort = boundEp.Port;
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
				BindSocket(false);

				m_receiveBuffer = new byte[m_configuration.ReceiveBufferSize];
				m_sendBuffer = new byte[m_configuration.SendBufferSize];
				m_readHelperMessage = new NetIncomingMessage(NetIncomingMessageType.Error);
				m_readHelperMessage.m_data = m_receiveBuffer;

				byte[] macBytes = new byte[8];
				MWCRandom.Instance.NextBytes(macBytes);

#if IS_MAC_AVAILABLE
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
				IPEndPoint boundEp = m_socket.LocalEndPoint as IPEndPoint;
				byte[] epBytes = BitConverter.GetBytes(boundEp.GetHashCode());
				byte[] combined = new byte[epBytes.Length + macBytes.Length];
				Array.Copy(epBytes, 0, combined, 0, epBytes.Length);
				Array.Copy(macBytes, 0, combined, epBytes.Length, macBytes.Length);
				m_uniqueIdentifier = BitConverter.ToInt64(NetUtility.CreateSHA1Hash(combined), 0);

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

			FlushDelayedPackets();

			// one final heartbeat, will send stuff and do disconnect
			Heartbeat();

			Thread.Sleep(10);

			lock (m_initializeLock)
			{
				try
				{
					if (m_socket != null)
					{
						try
						{
							m_socket.Shutdown(SocketShutdown.Receive);
						}
						catch(Exception ex)
						{
							LogDebug("Socket.Shutdown exception: " + ex.ToString());
						}

						try
						{
							m_socket.Close(2); // 2 seconds timeout
						}
						catch (Exception ex)
						{
							LogDebug("Socket.Close exception: " + ex.ToString());
						}
					}
				}
				finally
				{
					m_socket = null;
					m_status = NetPeerStatus.NotRunning;
					LogDebug("Shutdown complete");

					// wake up any threads waiting for server shutdown
					if (m_messageReceivedEvent != null)
						m_messageReceivedEvent.Set();
				}

				m_lastSocketBind = float.MinValue;
				m_receiveBuffer = null;
				m_sendBuffer = null;
				m_unsentUnconnectedMessages.Clear();
				m_connections.Clear();
				m_connectionLookup.Clear();
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
			if (delta > (1.0 / (double)maxCHBpS) || delta < 0.0) // max connection heartbeats/second max
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
							if (conn.m_status == NetConnectionStatus.Disconnected && m_handshakes.ContainsKey(conn.RemoteEndPoint))
							{
								LogWarning("Sanity fail! Handshakes list contained disconnected connection!");
								m_handshakes.Remove(conn.RemoteEndPoint);
							}
#endif
							break; // collection has been modified
						}
					}
				}

#if DEBUG
				SendDelayedPackets();
#endif

				// update m_executeFlushSendQueue
				if (m_configuration.m_autoFlushSendQueue)
					m_executeFlushSendQueue = true;

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
							m_connectionLookup.Remove(conn.RemoteEndPoint);
							break; // can't continue iteration here
						}
					}
				}
				m_executeFlushSendQueue = false;

				// send unsent unconnected messages
				NetTuple<IPEndPoint, NetOutgoingMessage> unsent;
				while (m_unsentUnconnectedMessages.TryDequeue(out unsent))
				{
					NetOutgoingMessage om = unsent.Item2;

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

			// update now
			dnow = NetTime.Now;
			now = (float)dnow;

			do
			{
				int bytesReceived = 0;
				try
				{
					bytesReceived = m_socket.ReceiveFrom(m_receiveBuffer, 0, m_receiveBuffer.Length, SocketFlags.None, ref m_senderRemote);
				}
				catch (SocketException sx)
				{
					switch (sx.SocketErrorCode)
					{
						case SocketError.ConnectionReset:
							// connection reset by peer, aka connection forcibly closed aka "ICMP port unreachable" 
							// we should shut down the connection; but m_senderRemote seemingly cannot be trusted, so which connection should we shut down?!
							// So, what to do?
							LogWarning("ConnectionReset");
							return;

						case SocketError.NotConnected:
							// socket is unbound; try to rebind it (happens on mobile when process goes to sleep)
							BindSocket(true);
							return;

						default:
							LogWarning("Socket exception: " + sx.ToString());
							return;
					}
				}

				if (bytesReceived < NetConstants.HeaderByteSize)
					return;

				//LogVerbose("Received " + bytesReceived + " bytes");

				IPEndPoint ipsender = (IPEndPoint)m_senderRemote;

				if (m_upnp != null && now < m_upnp.m_discoveryResponseDeadline && bytesReceived > 32)
				{
					// is this an UPnP response?
					string resp = System.Text.Encoding.ASCII.GetString(m_receiveBuffer, 0, bytesReceived);
					if (resp.Contains("upnp:rootdevice") || resp.Contains("UPnP/1.0"))
					{
						try
						{
							resp = resp.Substring(resp.ToLower().IndexOf("location:") + 9);
							resp = resp.Substring(0, resp.IndexOf("\r")).Trim();
							m_upnp.ExtractServiceUrl(resp);
							return;
						}
						catch (Exception ex)
						{
							LogDebug("Failed to parse UPnP response: " + ex.ToString());

							// don't try to parse this packet further
							return;
						}
					}
				}

				NetConnection sender = null;
				m_connectionLookup.TryGetValue(ipsender, out sender);

				//
				// parse packet into messages
				//
				int numMessages = 0;
				int numFragments = 0;
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

					numFragments++;

					ushort payloadBitLength = (ushort)(m_receiveBuffer[ptr++] | (m_receiveBuffer[ptr++] << 8));
					int payloadByteLength = NetUtility.BytesToHoldBits(payloadBitLength);

					if (bytesReceived - ptr < payloadByteLength)
					{
						LogWarning("Malformed packet; stated payload length " + payloadByteLength + ", remaining bytes " + (bytesReceived - ptr));
						return;
					}

					if (tp >= NetMessageType.Unused1 && tp <= NetMessageType.Unused29)
					{
						ThrowOrLog("Unexpected NetMessageType: " + tp);
						return;
					}

					try
					{
						if (tp >= NetMessageType.LibraryError)
						{
							if (sender != null)
								sender.ReceivedLibraryMessage(tp, ptr, payloadByteLength);
							else
								ReceivedUnconnectedLibraryMessage(dnow, ipsender, tp, ptr, payloadByteLength);
						}
						else
						{
							if (sender == null && !m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData))
								return; // dropping unconnected message since it's not enabled

							NetIncomingMessage msg = CreateIncomingMessage(NetIncomingMessageType.Data, payloadByteLength);
							msg.m_isFragment = isFragment;
							msg.m_receiveTime = dnow;
							msg.m_sequenceNumber = sequenceNumber;
							msg.m_receivedMessageType = tp;
							msg.m_senderConnection = sender;
							msg.m_senderEndPoint = ipsender;
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

				m_statistics.PacketReceived(bytesReceived, numMessages, numFragments);
				if (sender != null)
					sender.m_statistics.PacketReceived(bytesReceived, numMessages, numFragments);

			} while (m_socket.Available > 0);
		}

		/// <summary>
		/// If NetPeerConfiguration.AutoFlushSendQueue() is false; you need to call this to send all messages queued using SendMessage()
		/// </summary>
		public void FlushSendQueue()
		{
			m_executeFlushSendQueue = true;
		}

		internal void HandleIncomingDiscoveryRequest(double now, IPEndPoint senderEndPoint, int ptr, int payloadByteLength)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryRequest))
			{
				NetIncomingMessage dm = CreateIncomingMessage(NetIncomingMessageType.DiscoveryRequest, payloadByteLength);
				if (payloadByteLength > 0)
					Buffer.BlockCopy(m_receiveBuffer, ptr, dm.m_data, 0, payloadByteLength);
				dm.m_receiveTime = now;
				dm.m_bitLength = payloadByteLength * 8;
				dm.m_senderEndPoint = senderEndPoint;
				ReleaseMessage(dm);
			}
		}

		internal void HandleIncomingDiscoveryResponse(double now, IPEndPoint senderEndPoint, int ptr, int payloadByteLength)
		{
			if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.DiscoveryResponse))
			{
				NetIncomingMessage dr = CreateIncomingMessage(NetIncomingMessageType.DiscoveryResponse, payloadByteLength);
				if (payloadByteLength > 0)
					Buffer.BlockCopy(m_receiveBuffer, ptr, dr.m_data, 0, payloadByteLength);
				dr.m_receiveTime = now;
				dr.m_bitLength = payloadByteLength * 8;
				dr.m_senderEndPoint = senderEndPoint;
				ReleaseMessage(dr);
			}
		}

		private void ReceivedUnconnectedLibraryMessage(double now, IPEndPoint senderEndPoint, NetMessageType tp, int ptr, int payloadByteLength)
		{
			NetConnection shake;
			if (m_handshakes.TryGetValue(senderEndPoint, out shake))
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
					HandleIncomingDiscoveryRequest(now, senderEndPoint, ptr, payloadByteLength);
					return;
				case NetMessageType.DiscoveryResponse:
					HandleIncomingDiscoveryResponse(now, senderEndPoint, ptr, payloadByteLength);
					return;
				case NetMessageType.NatIntroduction:
					if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.NatIntroductionSuccess))
						HandleNatIntroduction(ptr);
					return;
				case NetMessageType.NatPunchMessage:
					if (m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.NatIntroductionSuccess))
						HandleNatPunch(ptr, senderEndPoint);
					return;
				case NetMessageType.ConnectResponse:

					lock (m_handshakes)
					{
						foreach (var hs in m_handshakes)
						{
							if (hs.Key.Address.Equals(senderEndPoint.Address))
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

									LogDebug("Detected host port change; rerouting connection to " + senderEndPoint);
									hsconn.MutateEndPoint(senderEndPoint);

									m_connectionLookup.Add(senderEndPoint, hsconn);
									m_handshakes.Add(senderEndPoint, hsconn);

									hsconn.ReceivedHandshake(now, tp, ptr, payloadByteLength);
									return;
								}
							}
						}
					}

					LogWarning("Received unhandled library message " + tp + " from " + senderEndPoint);
					return;
				case NetMessageType.Connect:
					if (m_configuration.AcceptIncomingConnections == false)
					{
						LogWarning("Received Connect, but we're not accepting incoming connections!");
						return;
					}
					// handle connect
					// It's someone wanting to shake hands with us!

					int reservedSlots = m_handshakes.Count + m_connections.Count;
					if (reservedSlots >= m_configuration.m_maximumConnections)
					{
						// server full
						NetOutgoingMessage full = CreateMessage("Server full");
						full.m_messageType = NetMessageType.Disconnect;
						SendLibrary(full, senderEndPoint);
						return;
					}

					// Ok, start handshake!
					NetConnection conn = new NetConnection(this, senderEndPoint);
					conn.m_status = NetConnectionStatus.ReceivedInitiation;
					m_handshakes.Add(senderEndPoint, conn);
					conn.ReceivedHandshake(now, tp, ptr, payloadByteLength);
					return;

				case NetMessageType.Disconnect:
					// this is probably ok
					LogVerbose("Received Disconnect from unconnected source: " + senderEndPoint);
					return;
				default:
					LogWarning("Received unhandled library message " + tp + " from " + senderEndPoint);
					return;
			}
		}

		internal void AcceptConnection(NetConnection conn)
		{
			// LogDebug("Accepted connection " + conn);
			conn.InitExpandMTU(NetTime.Now);

			if (m_handshakes.Remove(conn.m_remoteEndPoint) == false)
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
					m_connectionLookup.Add(conn.m_remoteEndPoint, conn);
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