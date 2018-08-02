using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class LidgrenEndPoint : PeerEndPoint
    {
        public static LidgrenEndPoint Parse(string input)
        {
            Guid guid;
            try { guid = Guid.Parse(input); }
            catch { guid = Guid.Empty; }
            return new LidgrenEndPoint(guid);
        }

        private Guid guid;

        public LidgrenEndPoint()
        {
            guid = Guid.NewGuid();
        }

        private LidgrenEndPoint(Guid guid)
        {
            this.guid = guid;
        }

        public override bool Equals(object obj)
        {
            var otherLidgren = obj as LidgrenEndPoint;
            if (otherLidgren == null)
            {
                return false;
            }
            return guid.Equals(otherLidgren.guid);
        }

        public override int GetHashCode()
        {
            return guid.GetHashCode();
        }

        public override bool Equals(PeerEndPoint other)
        {
            var otherLidgren = other as LidgrenEndPoint;
            if (otherLidgren == null)
            {
                return false;
            }
            return guid.Equals(otherLidgren.guid);
        }

        public override string ToString()
        {
            return guid.ToString();
        }
    }

    internal abstract class LidgrenPeer : Peer
    {
        public abstract long SessionId { get; }
    }

    internal class RemotePeer : LidgrenPeer
    {
        private NetConnection connection;
        private LidgrenEndPoint endPoint;
        private IPEndPoint internalIp;
        private IPEndPoint externalIp;

        public RemotePeer(NetConnection connection, LidgrenEndPoint endPoint, IPEndPoint internalIp)
        {
            this.connection = connection;
            this.endPoint = endPoint;
            this.internalIp = internalIp;
            this.externalIp = connection.RemoteEndPoint;
        }

        public NetConnection Connection { get { return connection; } }
        public IPEndPoint InternalIp { get { return internalIp; } }
        public IPEndPoint ExternalIp { get { return externalIp; } }

        public override PeerEndPoint EndPoint { get { return endPoint; } }
        public override long SessionId { get { return connection.RemoteUniqueIdentifier; } }
        public override TimeSpan RoundtripTime { get { return TimeSpan.FromSeconds(connection.AverageRoundtripTime); } }
        public override object Tag { get; set; }

        public override void Disconnect(string byeMessage)
        {
            connection.Disconnect(byeMessage);
        }
    }

    internal class LocalPeer : LidgrenPeer
    {
        private LidgrenBackend backend;
        private NetPeer peer;
        private LidgrenEndPoint endPoint;

        public LocalPeer(LidgrenBackend backend, NetPeer peer)
        {
            this.backend = backend;
            this.peer = peer;
            this.endPoint = new LidgrenEndPoint();
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
            get { return peer.Statistics.ReceivedBytes;  }
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

        public override PeerEndPoint EndPoint { get { return endPoint; } }
        public override long SessionId { get { return peer.UniqueIdentifier; } }
        public override TimeSpan RoundtripTime { get { return TimeSpan.Zero; } }
        public override object Tag { get; set; }

        public void Introduce(RemotePeer remoteClient, RemotePeer remoteTarget)
        {
            Debug.WriteLine("Introducing client " + remoteClient.ExternalIp + " to host " + remoteTarget.ExternalIp + "...");

            // As the client will receive the NatIntroductionSuccess message, send the target's endPoint as token
            string token = remoteTarget.EndPoint.ToString();

            peer.Introduce(remoteTarget.InternalIp, remoteTarget.ExternalIp, remoteClient.InternalIp, remoteClient.ExternalIp, token);
        }

        public void Connect(IPEndPoint targetIp)
        {
            if (peer.GetConnection(targetIp) == null)
            {
                var hailMsg = peer.CreateMessage();
                hailMsg.Write(endPoint.ToString());
                hailMsg.Write(InternalIp);
                peer.Connect(targetIp, hailMsg);
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

        public void SendMessage(LidgrenOutgoingMessage msg)
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

        public void ReceiveMessages(GenericPool<LidgrenOutgoingMessage> outgoingPool, GenericPool<LidgrenIncomingMessage> incomingPool)
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
                else if (msg.MessageType == NetIncomingMessageType.ConnectionApproval)
                {
                    var clientEndPoint = LidgrenEndPoint.Parse(msg.ReadString());

                    if (Listener.AllowConnectionFromClient(clientEndPoint))
                    {
                        var hailMsg = peer.CreateMessage();
                        hailMsg.Write(endPoint.ToString());
                        hailMsg.Write(InternalIp);
                        msg.SenderConnection.Approve(hailMsg);
                    }
                    else
                    {
                        msg.SenderConnection.Deny("Connection denied");
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.NatIntroductionSuccess)
                {
                    Debug.WriteLine("NAT introduction successful received from " + msg.SenderEndPoint);
                    var targetEndPoint = LidgrenEndPoint.Parse(msg.ReadString());

                    if (Listener.AllowConnectionToTargetAsClient(targetEndPoint))
                    {
                        Connect(msg.SenderEndPoint);
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.StatusChanged)
                {
                    if (msg.SenderConnection == null)
                    {
                        throw new NetworkException("Sender connection is null");
                    }

                    var status = (NetConnectionStatus)msg.ReadByte();
                    Debug.WriteLine("Status now: " + status + " (Reason: " + msg.ReadString() + ")");

                    if (status == NetConnectionStatus.Connected)
                    {
                        var endPoint = LidgrenEndPoint.Parse(msg.SenderConnection.RemoteHailMessage.ReadString());
                        var internalIp = msg.SenderConnection.RemoteHailMessage.ReadIPEndPoint();

                        var remotePeer = new RemotePeer(msg.SenderConnection, endPoint, internalIp);
                        msg.SenderConnection.Tag = remotePeer;
                        backend.RemotePeers.Add(remotePeer);

                        Listener.PeerConnected(remotePeer);
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        var disconnectedPeer = msg.SenderConnection.Tag as RemotePeer;
                        if (disconnectedPeer != null) // If null, host responded to connect then peer disconnected
                        {
                            backend.RemotePeers.Remove(disconnectedPeer);

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

                    //InvokeReceive(msg, msg.SenderConnection.Tag as RemotePeer);
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
                            Debug.WriteLine("Lidgren: " + msg.ReadString());
                            break;
                        default:
                            Debug.WriteLine("Unhandled type: " + msg.MessageType);
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

        public void RegisterWithMasterServer(GenericPool<LidgrenOutgoingMessage> outgoingPool)
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

    internal class LidgrenBackend : SessionBackend
    {
        private LocalPeer localPeer;
        private List<RemotePeer> remotePeers = new List<RemotePeer>();
        private List<NetConnection> remoteConnections = new List<NetConnection>();

        private GenericPool<LidgrenOutgoingMessage> outgoingMessagePool = new GenericPool<LidgrenOutgoingMessage>();
        private GenericPool<LidgrenIncomingMessage> incomingMessagePool = new GenericPool<LidgrenIncomingMessage>();

        private DateTime lastMasterServerRegistration = DateTime.MinValue;
        private DateTime lastStatisticsUpdate = DateTime.Now;
        private int lastReceivedBytes = 0;
        private int lastSentBytes = 0;

        public LidgrenBackend(NetPeer peer)
        {
            localPeer = new LocalPeer(this, peer);
        }

        public List<RemotePeer> RemotePeers { get { return remotePeers; } }
        public List<NetConnection> RemoteConnections { get { return remoteConnections; } }

        public override bool HasShutdown { get { return localPeer.HasShutdown; } }

        public override ISessionBackendListener Listener
        {
            get { return localPeer.Listener; }
            set { localPeer.Listener = value; }
        }

        public override Peer LocalPeer { get { return localPeer; } }

        public override TimeSpan SimulatedLatency
        {
            get { return localPeer.SimulatedLatency; }
            set { localPeer.SimulatedLatency = value; }
        }

        public override float SimulatedPacketLoss
        {
            get { return localPeer.SimulatedPacketLoss; }
            set { localPeer.SimulatedPacketLoss = value; }
        }

        public override int BytesPerSecondReceived { get; set; }
        public override int BytesPerSecondSent { get; set; }

        public override void Introduce(Peer client, Peer target)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            var remoteClient = client as RemotePeer;
            var remoteTarget = target as RemotePeer;

            if (remoteClient == null || remoteTarget == null)
            {
                throw new InvalidOperationException("Both client and target must be remote");
            }

            localPeer.Introduce(remoteClient, remoteTarget);
        }

        public LidgrenPeer FindPeerById(long id)
        {
            if (localPeer.SessionId == id)
            {
                return localPeer;
            }

            foreach (var remotePeer in remotePeers)
            {
                if (remotePeer.SessionId == id)
                {
                    return remotePeer;
                }
            }

            return null;
        }

        public override Peer FindRemotePeerByEndPoint(PeerEndPoint endPoint)
        {
            foreach (var remotePeer in remotePeers)
            {
                if (remotePeer.EndPoint.Equals(endPoint))
                {
                    return remotePeer;
                }
            }

            return null;
        }

        public override bool IsConnectedToEndPoint(PeerEndPoint endPoint)
        {
            return FindRemotePeerByEndPoint(endPoint) != null;
        }

        public override OutgoingMessage GetMessage(Peer recipient, SendDataOptions options, int channel)
        {
            var msg = outgoingMessagePool.Get();
            msg.recipient = recipient;
            msg.options = options;
            msg.channel = channel;
            return msg;
        }

        public override void SendMessage(OutgoingMessage message)
        {
            var msg = message as LidgrenOutgoingMessage;
            if (msg == null)
            {
                throw new NetworkException("Not possible to mix backends");
            }

            // Send to remote peer(s) if recipient is not the local peer only
            if (msg.Recipient != localPeer)
            {
                localPeer.SendMessage(msg);
            }

            // Send to self if recipient is null or the local peer
            if (msg.Recipient == null || msg.Recipient == localPeer)
            {
                msg.Buffer.Position = 0;

                // Pretend that the message was sent to the local peer over the network
                var incomingMsg = incomingMessagePool.Get();
                incomingMsg.Set(this, msg.Buffer);

                Listener.ReceiveMessage(incomingMsg, localPeer);

                incomingMessagePool.Recycle(incomingMsg);
            }

            outgoingMessagePool.Recycle(msg);
        }

        public override void Update()
        {
            localPeer.ReceiveMessages(outgoingMessagePool, incomingMessagePool);

            if (localPeer.HasShutdown)
            {
                return;
            }

            UpdateMasterServerRegistration();

            UpdateStatistics();
        }

        protected void UpdateMasterServerRegistration()
        {
            if (!Listener.IsDiscoverableOnline)
            {
                return;
            }

            var currentTime = DateTime.Now;
            var elapsedTime = currentTime - lastMasterServerRegistration;

            if (elapsedTime >= NetworkSessionSettings.MasterServerRegistrationInterval)
            {
                localPeer.RegisterWithMasterServer(outgoingMessagePool);

                lastMasterServerRegistration = currentTime;
            }
        }

        protected void UpdateStatistics()
        {
            var currentTime = DateTime.Now;
            int receivedBytes = localPeer.TotalReceivedBytes;
            int sentBytes = localPeer.TotalSentBytes;
            double elapsedSeconds = (currentTime - lastStatisticsUpdate).TotalSeconds;

            if (elapsedSeconds >= 1.0)
            {
                BytesPerSecondReceived = (int)Math.Round((receivedBytes - lastReceivedBytes) / elapsedSeconds);
                BytesPerSecondSent = (int)Math.Round((sentBytes - lastSentBytes) / elapsedSeconds);

                lastStatisticsUpdate = currentTime;
                lastReceivedBytes = receivedBytes;
                lastSentBytes = sentBytes;
            }
        }

        public override void Shutdown(string byeMessage)
        {
            if (localPeer.HasShutdown)
            {
                return;
            }

            localPeer.Disconnect(byeMessage);
        }
    }
}
