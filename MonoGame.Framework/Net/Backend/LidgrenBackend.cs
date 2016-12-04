using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class LidgrenEndPoint : IPeerEndPoint
    {
        public IPEndPoint endPoint;

        public LidgrenEndPoint(IPEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        public override bool Equals(object obj)
        {
            LidgrenEndPoint otherLidgren = obj as LidgrenEndPoint;

            if (otherLidgren == null)
            {
                return false;
            }

            return endPoint.Equals(otherLidgren.endPoint);
        }

        public override int GetHashCode()
        {
            return endPoint.GetHashCode();
        }

        public bool Equals(IPeerEndPoint other)
        {
            LidgrenEndPoint otherLidgren = other as LidgrenEndPoint;

            if (otherLidgren == null)
            {
                return false;
            }

            return endPoint.Equals(otherLidgren.endPoint);
        }

        public override string ToString()
        {
            return endPoint.ToString();
        }
    }

    internal interface ILidgrenPeer : IPeer
    {
        long Id { get; }
    }

    internal class RemotePeer : ILidgrenPeer
    {
        internal NetConnection connection;
        internal IPEndPoint internalEP;
        internal IPEndPoint externalEP;

        public RemotePeer(NetConnection connection, IPEndPoint internalEP)
        {
            this.connection = connection;
            this.internalEP = internalEP;
            this.externalEP = connection.RemoteEndPoint;

            this.EndPoint = new LidgrenEndPoint(connection.RemoteEndPoint);
        }

        public IPeerEndPoint EndPoint { get; }
        public long Id { get { return connection.RemoteUniqueIdentifier; } }
        public TimeSpan RoundtripTime { get { return TimeSpan.FromSeconds(connection.AverageRoundtripTime); } }
        public object Tag { get; set; }

        public void Disconnect(string byeMessage)
        {
            connection.Disconnect(byeMessage);
        }
    }

    internal class LocalPeer : ILidgrenPeer
    {
        internal NetPeer peer;
        internal IPEndPoint internalEP;

        public LocalPeer(NetPeer peer)
        {
            this.peer = peer;

            IPAddress address;
            NetUtility.GetMyAddress(out address);
            this.internalEP = new IPEndPoint(address, peer.Port);

            this.EndPoint = new LidgrenEndPoint(this.internalEP);
        }

        public IPeerEndPoint EndPoint { get; }
        public long Id { get { return peer.UniqueIdentifier; } }
        public TimeSpan RoundtripTime { get { return TimeSpan.Zero; } }
        public object Tag { get; set; }

        public void Disconnect(string byeMessage)
        {
            peer.Shutdown(byeMessage);
        }
    }

    internal class LidgrenBackend : ISessionBackend
    {
        private LocalPeer localPeer;
        private IPEndPoint initialConnectEndPoint;

        private IList<RemotePeer> remotePeers;
        private List<NetConnection> reportedConnections;
        
        private GenericPool<OutgoingMessage> outgoingMessagePool;
        private GenericPool<IncomingMessage> incomingMessagePool;

        private DateTime lastMasterServerRegistration;
        private DateTime lastStatisticsUpdate;
        private int lastReceivedBytes;
        private int lastSentBytes;

        public LidgrenBackend(NetPeer peer, IPEndPoint initialConnectEndPoint)
        {
            this.localPeer = new LocalPeer(peer);
            this.initialConnectEndPoint = initialConnectEndPoint;

            this.remotePeers = new List<RemotePeer>();
            this.reportedConnections = new List<NetConnection>();
            
            this.outgoingMessagePool = new GenericPool<OutgoingMessage>();
            this.incomingMessagePool = new GenericPool<IncomingMessage>();

            this.lastMasterServerRegistration = DateTime.MinValue;
            this.lastStatisticsUpdate = DateTime.Now;
            this.lastReceivedBytes = 0;
            this.lastSentBytes = 0;

            this.HasShutdown = false;
            this.LocalPeer = localPeer;
        }
        
        public bool HasShutdown { get; private set; }
        public IBackendListener Listener { get; set; }
        public IPeer LocalPeer { get; }

        public TimeSpan SimulatedLatency
        {
            get { return TimeSpan.FromSeconds(localPeer.peer.Configuration.SimulatedRandomLatency); }
            set { localPeer.peer.Configuration.SimulatedRandomLatency = (float)value.TotalSeconds; }
        }

        public float SimulatedPacketLoss
        {
            get { return localPeer.peer.Configuration.SimulatedLoss; }
            set { localPeer.peer.Configuration.SimulatedLoss = value; }
        }

        public int BytesPerSecondReceived { get; set; }
        public int BytesPerSecondSent { get; set; }

        internal ILidgrenPeer FindPeerById(long id)
        {
            if (localPeer.Id == id)
            {
                return localPeer;
            }

            foreach (RemotePeer remotePeer in remotePeers)
            {
                if (remotePeer.Id == id)
                {
                    return remotePeer;
                }
            }

            return null;
        }

        public void Introduce(IPeer client, IPeer target)
        {
            RemotePeer remoteClient = client as RemotePeer;
            RemotePeer remoteTarget = target as RemotePeer;

            if (remoteClient == null || remoteTarget == null)
            {
                return;
            }

            localPeer.peer.Introduce(remoteTarget.internalEP, remoteTarget.externalEP, remoteClient.internalEP, remoteClient.externalEP, string.Empty);
        }

        public void Connect(IPeerEndPoint endPoint)
        {
            LidgrenEndPoint ep = endPoint as LidgrenEndPoint;

            if (localPeer.peer.GetConnection(ep.endPoint) == null)
            {
                NetOutgoingMessage hailMsg = localPeer.peer.CreateMessage();
                hailMsg.Write(localPeer.internalEP);
                localPeer.peer.Connect(ep.endPoint, hailMsg);
            }
        }

        public IPeer FindRemotePeerByEndPoint(IPeerEndPoint endPoint)
        {
            LidgrenEndPoint ep = endPoint as LidgrenEndPoint;

            return localPeer.peer.GetConnection(ep.endPoint).Tag as RemotePeer;
        }

        public bool IsConnectedToEndPoint(IPeerEndPoint endPoint)
        {
            LidgrenEndPoint ep = endPoint as LidgrenEndPoint;

            return localPeer.peer.GetConnection(ep.endPoint) != null;
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

        public IOutgoingMessage GetMessage(IPeer recipient, SendDataOptions options, int channel)
        {
            OutgoingMessage msg = outgoingMessagePool.Get();
            msg.Recipient = recipient;
            msg.Options = options;
            msg.Channel = channel;
            return msg;
        }

        public void SendMessage(IOutgoingMessage message)
        {
            OutgoingMessage msg = message as OutgoingMessage;

            if (msg == null)
            {
                throw new NetworkException("Not possible to mix backends");
            }

            // Handle remote peers
            if (msg.Recipient != localPeer)
            {
                NetOutgoingMessage outgoingMsg = localPeer.peer.CreateMessage(msg.Buffer.LengthBytes);
                outgoingMsg.Write(msg.Buffer);

                if (msg.Recipient == null)
                {
                    if (reportedConnections.Count > 0)
                    {
                        localPeer.peer.SendMessage(outgoingMsg, reportedConnections, ToDeliveryMethod(msg.Options), msg.Channel);
                    }
                }
                else
                {
                    localPeer.peer.SendMessage(outgoingMsg, (msg.Recipient as RemotePeer).connection, ToDeliveryMethod(msg.Options), msg.Channel);
                }
            }

            // Handle self
            if (msg.Recipient == null || msg.Recipient == localPeer)
            {
                msg.Buffer.Position = 0;

                InvokeReceive(msg.Buffer, localPeer);
            }

            outgoingMessagePool.Recycle(msg);
        }

        private void InvokeReceive(NetBuffer buffer, IPeer sender)
        {
            IncomingMessage incomingMsg = incomingMessagePool.Get();
            incomingMsg.Backend = this;
            incomingMsg.Buffer = buffer;

            Listener.ReceiveMessage(incomingMsg, sender);
            
            incomingMessagePool.Recycle(incomingMsg);
        }

        public void Update()
        {
            ReceiveMessages();

            if (HasShutdown)
            {
                return;
            }

            UpdateMasterServerRegistration();

            UpdateStatistics();
        }

        protected void ReceiveMessages()
        {
            NetIncomingMessage msg;
            while ((msg = localPeer.peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    // Discovery
                    case NetIncomingMessageType.DiscoveryRequest:
                        if (initialConnectEndPoint == null)
                        {
                            Debug.WriteLine("Discovery request received");

                            OutgoingMessage responseMsg = outgoingMessagePool.Get();
                            Listener.SessionPublicInfo.Pack(responseMsg);

                            NetOutgoingMessage response = localPeer.peer.CreateMessage();
                            response.Write(responseMsg.Buffer);
                            localPeer.peer.SendDiscoveryResponse(response, msg.SenderEndPoint);

                            outgoingMessagePool.Recycle(responseMsg);
                        }
                        break;
                    // Connection approval
                    case NetIncomingMessageType.ConnectionApproval:
                        if (Listener.AllowConnectionFrom(new LidgrenEndPoint(msg.SenderEndPoint)))
                        {
                            NetOutgoingMessage hailMsg = localPeer.peer.CreateMessage();
                            hailMsg.Write(localPeer.internalEP);
                            msg.SenderConnection.Approve(hailMsg);
                        }
                        else
                        {
                            msg.SenderConnection.Deny("Connection denied");
                        }
                        break;
                    // Nat introduction
                    case NetIncomingMessageType.NatIntroductionSuccess:
                        Debug.WriteLine("Nat introduction successful");

                        if (localPeer.peer.ConnectionsCount == 0 && initialConnectEndPoint != null && msg.SenderEndPoint.Equals(initialConnectEndPoint))
                        {
                            // Initial connection introduced by master server
                            Debug.WriteLine("Connecting to initial end point...");

                            Connect(new LidgrenEndPoint(initialConnectEndPoint));
                        }
                        else
                        {
                            Listener.IntroducedAsClient(new LidgrenEndPoint(msg.SenderEndPoint));
                        }
                        break;
                    // Peer state changes
                    case NetIncomingMessageType.StatusChanged:
                        if (msg.SenderConnection == null)
                        {
                            throw new NetworkException("Sender connection is null");
                        }

                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        Debug.WriteLine("Status now: " + status + " - Reason: " + msg.ReadString());
                        
                        if (status == NetConnectionStatus.Connected)
                        {
                            IPEndPoint internalIPEndPoint = msg.SenderConnection.RemoteHailMessage.ReadIPEndPoint();

                            RemotePeer remotePeer = new RemotePeer(msg.SenderConnection, internalIPEndPoint);
                            msg.SenderConnection.Tag = remotePeer;
                            remotePeers.Add(remotePeer);
                            reportedConnections.Add(msg.SenderConnection);

                            Listener.PeerConnected(remotePeer);
                        }
                        else if (status == NetConnectionStatus.Disconnected)
                        {
                            RemotePeer disconnectedPeer = msg.SenderConnection.Tag as RemotePeer;

                            if (disconnectedPeer == null)
                            {
                                // Host responded to connect, then peer disconnected
                                break;
                            }

                            remotePeers.Remove(disconnectedPeer);
                            reportedConnections.Remove(msg.SenderConnection);

                            Listener.PeerDisconnected(disconnectedPeer);
                        }
                        break;
                    // Internal messages
                    case NetIncomingMessageType.Data:
                        if (msg.SenderConnection == null)
                        {
                            throw new NetworkException("Sender connection is null");
                        }

                        InvokeReceive(msg, msg.SenderConnection.Tag as RemotePeer);
                        break;
                    // Error checking
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

                localPeer.peer.Recycle(msg);

                if (HasShutdown)
                {
                    return;
                }
            }
        }

        protected void UpdateMasterServerRegistration()
        {
            if (!Listener.RegisterWithMasterServer)
            {
                return;
            }

            DateTime currentTime = DateTime.Now;
            TimeSpan elapsedTime = currentTime - lastMasterServerRegistration;

            if (elapsedTime >= NetworkSessionSettings.MasterServerRegistrationInterval)
            {
                IPEndPoint masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

                OutgoingMessage msg = outgoingMessagePool.Get();
                msg.Write((byte)MasterServerMessageType.RegisterHost);
                msg.Write(localPeer.Id);
                msg.Write(localPeer.EndPoint);
                Listener.SessionPublicInfo.Pack(msg);

                NetOutgoingMessage request = localPeer.peer.CreateMessage();
                request.Write(msg.Buffer);
                localPeer.peer.SendUnconnectedMessage(request, masterServerEndPoint);

                outgoingMessagePool.Recycle(msg);
                lastMasterServerRegistration = currentTime;

                Debug.WriteLine("Registering with master server (Id: " + localPeer.Id + ", IPEndPoint: " + localPeer.EndPoint + ")");
            }
        }

        protected void UpdateStatistics()
        {
            DateTime currentTime = DateTime.Now;
            int receivedBytes = localPeer.peer.Statistics.ReceivedBytes;
            int sentBytes = localPeer.peer.Statistics.SentBytes;
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

        public void Shutdown(string byeMessage)
        {
            HasShutdown = true;

            localPeer.Disconnect(byeMessage);
        }
    }
}
