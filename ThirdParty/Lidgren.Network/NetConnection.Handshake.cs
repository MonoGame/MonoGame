using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
#endif

namespace Lidgren.Network
{
	public partial class NetConnection
	{
		internal bool m_connectRequested;
		internal bool m_disconnectRequested;
		internal bool m_disconnectReqSendBye;
		internal string m_disconnectMessage;
		internal bool m_connectionInitiator;
		internal NetIncomingMessage m_remoteHailMessage;
		internal double m_lastHandshakeSendTime;
		internal int m_handshakeAttempts;

		/// <summary>
		/// The message that the remote part specified via Connect() or Approve() - can be null.
		/// </summary>
		public NetIncomingMessage RemoteHailMessage { get { return m_remoteHailMessage; } }

		// heartbeat called when connection still is in m_handshakes of NetPeer
		internal void UnconnectedHeartbeat(double now)
		{
			m_peer.VerifyNetworkThread();

			if (m_disconnectRequested)
				ExecuteDisconnect(m_disconnectMessage, true);

			if (m_connectRequested)
			{
				switch (m_status)
				{
					case NetConnectionStatus.Connected:
					case NetConnectionStatus.RespondedConnect:
						// reconnect
						ExecuteDisconnect("Reconnecting", true);
						break;

					case NetConnectionStatus.InitiatedConnect:
						// send another connect attempt
						SendConnect(now);
						break;

					case NetConnectionStatus.Disconnected:
						m_peer.ThrowOrLog("This connection is Disconnected; spent. A new one should have been created");
						break;

					case NetConnectionStatus.Disconnecting:
						// let disconnect finish first
						break;

					case NetConnectionStatus.None:
					default:
						SendConnect(now);
						break;
				}
				return;
			}

			if (now - m_lastHandshakeSendTime > m_peerConfiguration.m_resendHandshakeInterval)
			{
				if (m_handshakeAttempts >= m_peerConfiguration.m_maximumHandshakeAttempts)
				{
					// failed to connect
					ExecuteDisconnect("Failed to establish connection - no response from remote host", true);
					return;
				}

				// resend handshake
				switch (m_status)
				{
					case NetConnectionStatus.InitiatedConnect:
						SendConnect(now);
						break;
					case NetConnectionStatus.RespondedConnect:
						SendConnectResponse(now, true);
						break;
					case NetConnectionStatus.RespondedAwaitingApproval:
						// awaiting approval
						m_lastHandshakeSendTime = now; // postpone handshake resend
						break;
					case NetConnectionStatus.None:
					case NetConnectionStatus.ReceivedInitiation:
					default:
						m_peer.LogWarning("Time to resend handshake, but status is " + m_status);
						break;
				}
			}
		}

		internal void ExecuteDisconnect(string reason, bool sendByeMessage)
		{
			m_peer.VerifyNetworkThread();

			// clear send queues
			for (int i = 0; i < m_sendChannels.Length; i++)
			{
				NetSenderChannelBase channel = m_sendChannels[i];
				if (channel != null)
					channel.Reset();
			}

			if (sendByeMessage)
				SendDisconnect(reason, true);

			if (m_status == NetConnectionStatus.ReceivedInitiation)
			{
				// nothing much has happened yet; no need to send disconnected status message
				m_status = NetConnectionStatus.Disconnected;
			}
			else
			{
				SetStatus(NetConnectionStatus.Disconnected, reason);
			}

			// in case we're still in handshake
			lock (m_peer.m_handshakes)
				m_peer.m_handshakes.Remove(m_remoteEndPoint);

			m_disconnectRequested = false;
			m_connectRequested = false;
			m_handshakeAttempts = 0;
		}

		internal void SendConnect(double now)
		{
			m_peer.VerifyNetworkThread();

			int preAllocate = 13 + m_peerConfiguration.AppIdentifier.Length;
			preAllocate += (m_localHailMessage == null ? 0 : m_localHailMessage.LengthBytes);

			NetOutgoingMessage om = m_peer.CreateMessage(preAllocate);
			om.m_messageType = NetMessageType.Connect;
			om.Write(m_peerConfiguration.AppIdentifier);
			om.Write(m_peer.m_uniqueIdentifier);
			om.Write((float)now);

			WriteLocalHail(om);
			
			m_peer.SendLibrary(om, m_remoteEndPoint);

			m_connectRequested = false;
			m_lastHandshakeSendTime = now;
			m_handshakeAttempts++;

			if (m_handshakeAttempts > 1)
				m_peer.LogDebug("Resending Connect...");
			SetStatus(NetConnectionStatus.InitiatedConnect, "Locally requested connect");
		}

		internal void SendConnectResponse(double now, bool onLibraryThread)
		{
			if (onLibraryThread)
				m_peer.VerifyNetworkThread();

			NetOutgoingMessage om = m_peer.CreateMessage(m_peerConfiguration.AppIdentifier.Length + 13 + (m_localHailMessage == null ? 0 : m_localHailMessage.LengthBytes));
			om.m_messageType = NetMessageType.ConnectResponse;
			om.Write(m_peerConfiguration.AppIdentifier);
			om.Write(m_peer.m_uniqueIdentifier);
			om.Write((float)now);
			Interlocked.Increment(ref om.m_recyclingCount);
			WriteLocalHail(om);

			if (onLibraryThread)
				m_peer.SendLibrary(om, m_remoteEndPoint);
			else
				m_peer.m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(m_remoteEndPoint, om));

			m_lastHandshakeSendTime = now;
			m_handshakeAttempts++;

			if (m_handshakeAttempts > 1)
				m_peer.LogDebug("Resending ConnectResponse...");

			SetStatus(NetConnectionStatus.RespondedConnect, "Remotely requested connect");
		}

		internal void SendDisconnect(string reason, bool onLibraryThread)
		{
			if (onLibraryThread)
				m_peer.VerifyNetworkThread();

			NetOutgoingMessage om = m_peer.CreateMessage(reason);
			om.m_messageType = NetMessageType.Disconnect;
			Interlocked.Increment(ref om.m_recyclingCount);
			if (onLibraryThread)
				m_peer.SendLibrary(om, m_remoteEndPoint);
			else
				m_peer.m_unsentUnconnectedMessages.Enqueue(new NetTuple<NetEndPoint, NetOutgoingMessage>(m_remoteEndPoint, om));
		}

		private void WriteLocalHail(NetOutgoingMessage om)
		{
			if (m_localHailMessage != null)
			{
				byte[] hi = m_localHailMessage.Data;
				if (hi != null && hi.Length >= m_localHailMessage.LengthBytes)
				{
					if (om.LengthBytes + m_localHailMessage.LengthBytes > m_peerConfiguration.m_maximumTransmissionUnit - 10)
						m_peer.ThrowOrLog("Hail message too large; can maximally be " + (m_peerConfiguration.m_maximumTransmissionUnit - 10 - om.LengthBytes));
					om.Write(m_localHailMessage.Data, 0, m_localHailMessage.LengthBytes);
				}
			}
		}

		internal void SendConnectionEstablished()
		{
			NetOutgoingMessage om = m_peer.CreateMessage(4);
			om.m_messageType = NetMessageType.ConnectionEstablished;
			om.Write((float)NetTime.Now);
			m_peer.SendLibrary(om, m_remoteEndPoint);

			m_handshakeAttempts = 0;

			InitializePing();
			if (m_status != NetConnectionStatus.Connected)
				SetStatus(NetConnectionStatus.Connected, "Connected to " + NetUtility.ToHexString(m_remoteUniqueIdentifier));
		}

		/// <summary>
		/// Approves this connection; sending a connection response to the remote host
		/// </summary>
		public void Approve()
		{
			if (m_status != NetConnectionStatus.RespondedAwaitingApproval)
			{
				m_peer.LogWarning("Approve() called in wrong status; expected RespondedAwaitingApproval; got " + m_status);
				return;
			}

			m_localHailMessage = null;
			m_handshakeAttempts = 0;
			SendConnectResponse(NetTime.Now, false);
		}

		/// <summary>
		/// Approves this connection; sending a connection response to the remote host
		/// </summary>
		/// <param name="localHail">The local hail message that will be set as RemoteHailMessage on the remote host</param>
		public void Approve(NetOutgoingMessage localHail)
		{
			if (m_status != NetConnectionStatus.RespondedAwaitingApproval)
			{
				m_peer.LogWarning("Approve() called in wrong status; expected RespondedAwaitingApproval; got " + m_status);
				return;
			}

			m_localHailMessage = localHail;
			m_handshakeAttempts = 0;
			SendConnectResponse(NetTime.Now, false);
		}

		/// <summary>
		/// Denies this connection; disconnecting it
		/// </summary>
		public void Deny()
		{
			Deny(string.Empty);
		}

		/// <summary>
		/// Denies this connection; disconnecting it
		/// </summary>
		/// <param name="reason">The stated reason for the disconnect, readable as a string in the StatusChanged message on the remote host</param>
		public void Deny(string reason)
		{
			// send disconnect; remove from handshakes
			SendDisconnect(reason, false);

			// remove from handshakes
			lock (m_peer.m_handshakes)
				m_peer.m_handshakes.Remove(m_remoteEndPoint);
		}

		internal void ReceivedHandshake(double now, NetMessageType tp, int ptr, int payloadLength)
		{
			m_peer.VerifyNetworkThread();

			byte[] hail;
			switch (tp)
			{
				case NetMessageType.Connect:
					if (m_status == NetConnectionStatus.ReceivedInitiation)
					{
						// Whee! Server full has already been checked
						bool ok = ValidateHandshakeData(ptr, payloadLength, out hail);
						if (ok)
						{
							if (hail != null)
							{
								m_remoteHailMessage = m_peer.CreateIncomingMessage(NetIncomingMessageType.Data, hail);
								m_remoteHailMessage.LengthBits = (hail.Length * 8);
							}
							else
							{
								m_remoteHailMessage = null; 
							}

							if (m_peerConfiguration.IsMessageTypeEnabled(NetIncomingMessageType.ConnectionApproval))
							{
								// ok, let's not add connection just yet
								NetIncomingMessage appMsg = m_peer.CreateIncomingMessage(NetIncomingMessageType.ConnectionApproval, (m_remoteHailMessage == null ? 0 : m_remoteHailMessage.LengthBytes));
								appMsg.m_receiveTime = now;
								appMsg.m_senderConnection = this;
								appMsg.m_senderEndPoint = this.m_remoteEndPoint;
								if (m_remoteHailMessage != null)
									appMsg.Write(m_remoteHailMessage.m_data, 0, m_remoteHailMessage.LengthBytes);
								SetStatus(NetConnectionStatus.RespondedAwaitingApproval, "Awaiting approval");
								m_peer.ReleaseMessage(appMsg);
								return;
							}

							SendConnectResponse((float)now, true);
						}
						return;
					}
					if (m_status == NetConnectionStatus.RespondedAwaitingApproval)
					{
						m_peer.LogWarning("Ignoring multiple Connect() most likely due to a delayed Approval");
						return;
					}
					if (m_status == NetConnectionStatus.RespondedConnect)
					{
						// our ConnectResponse must have been lost
						SendConnectResponse((float)now, true);
						return;
					}
					m_peer.LogDebug("Unhandled Connect: " + tp + ", status is " + m_status + " length: " + payloadLength);
					break;
				case NetMessageType.ConnectResponse:
					HandleConnectResponse(now, tp, ptr, payloadLength);
					break;

				case NetMessageType.ConnectionEstablished:
					switch (m_status)
					{
						case NetConnectionStatus.Connected:
							// ok...
							break;
						case NetConnectionStatus.Disconnected:
						case NetConnectionStatus.Disconnecting:
						case NetConnectionStatus.None:
							// too bad, almost made it
							break;
						case NetConnectionStatus.ReceivedInitiation:
							// uh, a little premature... ignore
							break;
						case NetConnectionStatus.InitiatedConnect:
							// weird, should have been RespondedConnect...
							break;
						case NetConnectionStatus.RespondedConnect:
							// awesome
				
							NetIncomingMessage msg = m_peer.SetupReadHelperMessage(ptr, payloadLength);
							InitializeRemoteTimeOffset(msg.ReadSingle());

							m_peer.AcceptConnection(this);
							InitializePing();
							SetStatus(NetConnectionStatus.Connected, "Connected to " + NetUtility.ToHexString(m_remoteUniqueIdentifier));
							return;
					}
					break;

				case NetMessageType.Disconnect:
					// ouch
					string reason = "Ouch";
					try
					{
						NetIncomingMessage inc = m_peer.SetupReadHelperMessage(ptr, payloadLength);
						reason = inc.ReadString();
					}
					catch
					{
					}
					ExecuteDisconnect(reason, false);
					break;

				case NetMessageType.Discovery:
					m_peer.HandleIncomingDiscoveryRequest(now, m_remoteEndPoint, ptr, payloadLength);
					return;

				case NetMessageType.DiscoveryResponse:
					m_peer.HandleIncomingDiscoveryResponse(now, m_remoteEndPoint, ptr, payloadLength);
					return;

				case NetMessageType.Ping:
					// silently ignore
					return;

				default:
					m_peer.LogDebug("Unhandled type during handshake: " + tp + " length: " + payloadLength);
					break;
			}
		}

		private void HandleConnectResponse(double now, NetMessageType tp, int ptr, int payloadLength)
		{
			byte[] hail;
			switch (m_status)
			{
				case NetConnectionStatus.InitiatedConnect:
					// awesome
					bool ok = ValidateHandshakeData(ptr, payloadLength, out hail);
					if (ok)
					{
						if (hail != null)
						{
							m_remoteHailMessage = m_peer.CreateIncomingMessage(NetIncomingMessageType.Data, hail);
							m_remoteHailMessage.LengthBits = (hail.Length * 8);
						}
						else
						{
							m_remoteHailMessage = null;
						}

						m_peer.AcceptConnection(this);
						SendConnectionEstablished();
						return;
					}
					break;
				case NetConnectionStatus.RespondedConnect:
					// hello, wtf?
					break;
				case NetConnectionStatus.Disconnecting:
				case NetConnectionStatus.Disconnected:
				case NetConnectionStatus.ReceivedInitiation:
				case NetConnectionStatus.None:
					// wtf? anyway, bye!
					break;
				case NetConnectionStatus.Connected:
					// my ConnectionEstablished must have been lost, send another one
					SendConnectionEstablished();
					return;
			}
		}

		private bool ValidateHandshakeData(int ptr, int payloadLength, out byte[] hail)
		{
			hail = null;

			// create temporary incoming message
			NetIncomingMessage msg = m_peer.SetupReadHelperMessage(ptr, payloadLength);
			try
			{
				string remoteAppIdentifier = msg.ReadString();
				long remoteUniqueIdentifier = msg.ReadInt64();
				InitializeRemoteTimeOffset(msg.ReadSingle());

				int remainingBytes = payloadLength - (msg.PositionInBytes - ptr);
				if (remainingBytes > 0)
					hail = msg.ReadBytes(remainingBytes);

				if (remoteAppIdentifier != m_peer.m_configuration.AppIdentifier)
				{
					ExecuteDisconnect("Wrong application identifier!", true);
					return false;
				}

				m_remoteUniqueIdentifier = remoteUniqueIdentifier;
			}
			catch(Exception ex)
			{
				// whatever; we failed
				ExecuteDisconnect("Handshake data validation failed", true);
				m_peer.LogWarning("ReadRemoteHandshakeData failed: " + ex.Message);
				return false;
			}
			return true;
		}
		
		/// <summary>
		/// Disconnect from the remote peer
		/// </summary>
		/// <param name="byeMessage">the message to send with the disconnect message</param>
		public void Disconnect(string byeMessage)
		{
			// user or library thread
			if (m_status == NetConnectionStatus.None || m_status == NetConnectionStatus.Disconnected)
				return;

			m_peer.LogVerbose("Disconnect requested for " + this);
			m_disconnectMessage = byeMessage;

			if (m_status != NetConnectionStatus.Disconnected && m_status != NetConnectionStatus.None)
				SetStatus(NetConnectionStatus.Disconnecting, byeMessage);

			m_handshakeAttempts = 0;
			m_disconnectRequested = true;
			m_disconnectReqSendBye = true;
		}
	}
}
