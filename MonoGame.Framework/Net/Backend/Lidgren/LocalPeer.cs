using System;
using System.Diagnostics;
using System.Net;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class LocalPeer : LidgrenPeer
    {
        private SessionBackend backend;
        private NetPeer peer;
        private GuidEndPoint endPoint;

        public LocalPeer(SessionBackend backend, NetPeer peer)
        {
            this.backend = backend;
            this.peer = peer;
            this.endPoint = new GuidEndPoint();
        }

        public bool HasShutdown { get; private set; }
        public ISessionBackendListener Listener { get; set; }

        public TimeSpan SimulatedLatency
        {
#if DEBUG
            get { return TimeSpan.FromSeconds(peer.Configuration.SimulatedRandomLatency); }
            set { peer.Configuration.SimulatedRandomLatency = (float)value.TotalSeconds; }
#else
            get { return TimeSpan.Zero; }
            set { }
#endif
        }

        public float SimulatedPacketLoss
        {
#if DEBUG
            get { return peer.Configuration.SimulatedLoss; }
            set { peer.Configuration.SimulatedLoss = value; }
#else
            get { return 0.0f; }
            set { }
#endif
        }

        public int TotalReceivedBytes
        {
            get { return peer.Statistics.ReceivedBytes; }
        }

        public int TotalSentBytes
        {
            get { return peer.Statistics.SentBytes; }
        }

        public IPEndPoint InternalIp
        {
            get
            {
                IPAddress mask;
                IPAddress address = NetUtility.GetMyAddress(out mask);
                return new IPEndPoint(address, peer.Port);
            }
        }

        public override BasePeerEndPoint EndPoint { get { return endPoint; } }
        public override long SessionId { get { return peer.UniqueIdentifier; } }
        public override TimeSpan RoundtripTime { get { return TimeSpan.Zero; } }
        public override object Tag { get; set; }

        public void Introduce(RemotePeer remoteClient, RemotePeer remoteHost)
        {
            Debug.WriteLine("Introducing client " + remoteClient.ExternalIp + " to host " + remoteHost.ExternalIp + "...");

            // The client will receive the NatIntroductionSuccess message
            string token = new IntroducerToken(remoteHost.EndPoint as GuidEndPoint,
                                                        remoteHost.ExternalIp,
                                                        remoteClient.ExternalIp).Serialize();

            peer.Introduce(remoteHost.InternalIp, remoteHost.ExternalIp, remoteClient.InternalIp, remoteClient.ExternalIp, token);
        }

        public void Connect(IPEndPoint destinationIp, IPEndPoint destinationExternalIp, IPEndPoint observedExternalIp)
        {
            if (peer.GetConnection(destinationIp) == null)
            {
                var hailMsg = peer.CreateMessage();
                hailMsg.Write(endPoint.ToString());
                hailMsg.Write(InternalIp);
                hailMsg.Write(observedExternalIp);
                hailMsg.Write(destinationExternalIp);
                peer.Connect(destinationIp, hailMsg);
            }
        }

        public override void Disconnect(string byeMessage)
        {
            HasShutdown = true;

            UnregisterWithMasterServer();

            peer.Shutdown(byeMessage);
        }

        private NetDeliveryMethod ToDeliveryMethod(SendDataOptions options)
        {
            switch (options)
            {
                case SendDataOptions.InOrder:
                    return NetDeliveryMethod.UnreliableSequenced;
                case SendDataOptions.Reliable:
                    return NetDeliveryMethod.ReliableUnordered;
                case SendDataOptions.ReliableInOrder:
                    return NetDeliveryMethod.ReliableOrdered;
                case SendDataOptions.Chat:
                    return NetDeliveryMethod.ReliableUnordered;
                case SendDataOptions.Chat & SendDataOptions.InOrder:
                    return NetDeliveryMethod.ReliableOrdered;
                default:
                    throw new InvalidOperationException("Could not convert SendDataOptions!");
            }
        }

        public void SendMessage(OutgoingMessage msg)
        {
            var outgoingMsg = peer.CreateMessage(msg.Buffer.LengthBytes);
            outgoingMsg.Write(msg.Buffer);

            if (msg.Recipient == null)
            {
                var connections = backend.RemoteConnections;
                if (connections.Count > 0)
                {
                    peer.SendMessage(outgoingMsg, connections, ToDeliveryMethod(msg.Options), msg.Channel);
                }
            }
            else
            {
                peer.SendMessage(outgoingMsg, (msg.Recipient as RemotePeer).Connection, ToDeliveryMethod(msg.Options), msg.Channel);
            }
        }

        public void ReceiveMessages(GenericPool<OutgoingMessage> outgoingPool, GenericPool<IncomingMessage> incomingPool)
        {
            NetIncomingMessage msg;
            while ((msg = peer.ReadMessage()) != null)
            {
                if (msg.MessageType == NetIncomingMessageType.DiscoveryRequest)
                {
                    if (Listener.IsDiscoverableLocally)
                    {
                        Debug.WriteLine("Discovery request received.");

                        var responseMsg = outgoingPool.Get();
                        responseMsg.Write(endPoint.ToString());
                        Listener.SessionPublicInfo.Pack(responseMsg);

                        var response = peer.CreateMessage();
                        response.Write(responseMsg.Buffer);
                        peer.SendDiscoveryResponse(response, msg.SenderEndPoint);
                        outgoingPool.Recycle(responseMsg);
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.NatIntroductionSuccess)
                {
                    // This peer is a client from a NAT introduction standpoint
                    var hostPunchIp = msg.SenderEndPoint;

                    Debug.WriteLine($"NAT introduction successful received from {hostPunchIp}");

                    if (IntroducerToken.Deserialize(msg.ReadString(), out IntroducerToken token))
                    {
                        if (Listener.AllowConnectionToHostAsClient(token.HostEndPoint))
                        {
                            Connect(hostPunchIp, token.HostExternalIp, token.ClientExternalIp);
                        }
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.ConnectionApproval)
                {
                    // This peer is a host from a NAT introduction standpoint
                    var clientEndPoint = GuidEndPoint.Parse(msg.ReadString());
                    var clientInternalIp = msg.ReadIPEndPoint();
                    var clientExternalIp = msg.ReadIPEndPoint();
                    var hostExternalIp = msg.ReadIPEndPoint(); // From IntroducerToken above

                    if (Listener.AllowConnectionFromClient(clientEndPoint))
                    {
                        var hailMsg = peer.CreateMessage();
                        hailMsg.Write(endPoint.ToString());
                        hailMsg.Write(InternalIp);
                        hailMsg.Write(hostExternalIp); // External ip unknown to this peer, must come from outisde
                        msg.SenderConnection.Approve(hailMsg);
                    }
                    else
                    {
                        msg.SenderConnection.Deny("Connection denied");
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.StatusChanged)
                {
                    if (msg.SenderConnection == null)
                    {
                        throw new NetworkException("Sender connection is null");
                    }

                    var status = (NetConnectionStatus)msg.ReadByte();
                    Debug.WriteLine($"Status now: {status} (Reason: {msg.ReadString()})");

                    if (status == NetConnectionStatus.Connected)
                    {
                        var hailMsg = msg.SenderConnection.RemoteHailMessage;
                        var endPoint = GuidEndPoint.Parse(hailMsg.ReadString());
                        var internalIp = hailMsg.ReadIPEndPoint();
                        var externalIp = hailMsg.ReadIPEndPoint();

                        var remotePeer = new RemotePeer(msg.SenderConnection, endPoint, internalIp, externalIp);
                        msg.SenderConnection.Tag = remotePeer;
                        backend.AddRemotePeer(remotePeer);
                        Listener.PeerConnected(remotePeer);
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        var disconnectedPeer = msg.SenderConnection.Tag as RemotePeer;
                        if (disconnectedPeer != null) // If null, host responded to connect then peer disconnected
                        {
                            backend.RemoveRemotePeer(disconnectedPeer);
                            Listener.PeerDisconnected(disconnectedPeer);
                        }
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.Data)
                {
                    if (msg.SenderConnection == null)
                    {
                        throw new NetworkException("Sender connection is null");
                    }

                    var incomingMsg = incomingPool.Get();
                    incomingMsg.Set(backend, msg);
                    Listener.ReceiveMessage(incomingMsg, msg.SenderConnection.Tag as RemotePeer);
                    incomingPool.Recycle(incomingMsg);
                }
                else
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            Debug.WriteLine($"Lidgren: {msg.ReadString()}");
                            break;
                        default:
                            Debug.WriteLine($"Unhandled type: {msg.MessageType}");
                            break;
                    }
                }

                peer.Recycle(msg);

                if (HasShutdown)
                {
                    return;
                }
            }
        }

        public void RegisterWithMasterServer(GenericPool<OutgoingMessage> outgoingPool)
        {
            if (!Listener.IsDiscoverableOnline)
            {
                return;
            }

            var masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            var msg = outgoingPool.Get();
            msg.Write(peer.Configuration.AppIdentifier);
            msg.Write((byte)MasterServerMessageType.RegisterHost);
            msg.Write(endPoint.ToString());
            msg.Write(InternalIp);
            Listener.SessionPublicInfo.Pack(msg);

            var request = peer.CreateMessage();
            request.Write(msg.Buffer);
            peer.SendUnconnectedMessage(request, masterServerEndPoint);

            outgoingPool.Recycle(msg);

            Debug.WriteLine("Registering with master server (EndPoint: " + endPoint + ", InternalIp: " + InternalIp + ")");
        }

        public void UnregisterWithMasterServer()
        {
            if (!Listener.IsDiscoverableOnline)
            {
                return;
            }

            var masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            var msg = peer.CreateMessage();
            msg.Write(peer.Configuration.AppIdentifier);
            msg.Write((byte)MasterServerMessageType.UnregisterHost);
            msg.Write(endPoint.ToString());
            peer.SendUnconnectedMessage(msg, masterServerEndPoint);

            Debug.WriteLine("Unregistering with master server (EndPoint: " + endPoint + ")");
        }
    }
}
