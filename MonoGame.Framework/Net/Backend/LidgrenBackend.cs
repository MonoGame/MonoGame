﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class LidgrenEndPoint : PeerEndPoint
    {
        internal static LidgrenEndPoint Parse(string input)
        {
            Guid guid;

            try { guid = Guid.Parse(input); }
            catch { guid = Guid.Empty; }

            return new LidgrenEndPoint(guid);
        }

        private Guid guid;

        public LidgrenEndPoint()
        {
            this.guid = Guid.NewGuid();
        }

        private LidgrenEndPoint(Guid guid)
        {
            this.guid = guid;
        }

        public override bool Equals(object obj)
        {
            LidgrenEndPoint otherLidgren = obj as LidgrenEndPoint;

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
            LidgrenEndPoint otherLidgren = other as LidgrenEndPoint;

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
        internal NetConnection connection;
        internal LidgrenEndPoint endPoint;
        internal IPEndPoint intIPEndPoint;
        internal IPEndPoint extIPEndPoint;

        public RemotePeer(NetConnection connection, LidgrenEndPoint endPoint, IPEndPoint intIPEndPoint)
        {
            this.connection = connection;
            this.endPoint = endPoint;
            this.intIPEndPoint = intIPEndPoint;
            this.extIPEndPoint = connection.RemoteEndPoint;
        }

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
        internal NetPeer peer;
        internal LidgrenEndPoint endPoint;

        public LocalPeer(NetPeer peer)
        {
            this.peer = peer;
            this.endPoint = new LidgrenEndPoint();
        }

        internal IPEndPoint InternalIPEndPoint
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

        internal void Connect(IPEndPoint target)
        {
            if (peer.GetConnection(target) == null)
            {
                NetOutgoingMessage hailMsg = peer.CreateMessage();
                hailMsg.Write(endPoint.ToString());
                hailMsg.Write(InternalIPEndPoint);
                peer.Connect(target, hailMsg);
            }
        }

        public override void Disconnect(string byeMessage)
        {
            peer.Shutdown(byeMessage);
        }
    }

    internal class LidgrenBackend : SessionBackend
    {
        private bool hasShutdown;
        private LocalPeer localPeer;
        private IList<RemotePeer> remotePeers;
        private List<NetConnection> reportedConnections;

        private GenericPool<LidgrenOutgoingMessage> outgoingMessagePool;
        private GenericPool<LidgrenIncomingMessage> incomingMessagePool;

        private DateTime lastMasterServerRegistration;
        private DateTime lastStatisticsUpdate;
        private int lastReceivedBytes;
        private int lastSentBytes;

        public LidgrenBackend(NetPeer peer)
        {
            this.hasShutdown = false;
            this.localPeer = new LocalPeer(peer);
            this.remotePeers = new List<RemotePeer>();
            this.reportedConnections = new List<NetConnection>();

            this.outgoingMessagePool = new GenericPool<LidgrenOutgoingMessage>();
            this.incomingMessagePool = new GenericPool<LidgrenIncomingMessage>();

            this.lastMasterServerRegistration = DateTime.MinValue;
            this.lastStatisticsUpdate = DateTime.Now;
            this.lastReceivedBytes = 0;
            this.lastSentBytes = 0;

            this.LocalPeer = localPeer;
        }

        public override bool HasShutdown { get { return hasShutdown; } }
        public override ISessionBackendListener Listener { get; set; }
        public override Peer LocalPeer { get; }

        public override TimeSpan SimulatedLatency
        {
#if DEBUG
            get { return TimeSpan.FromSeconds(localPeer.peer.Configuration.SimulatedRandomLatency); }
            set { localPeer.peer.Configuration.SimulatedRandomLatency = (float)value.TotalSeconds; }
#else
            get { return TimeSpan.Zero; }
            set { }
#endif
        }

        public override float SimulatedPacketLoss
        {
#if DEBUG
            get { return localPeer.peer.Configuration.SimulatedLoss; }
            set { localPeer.peer.Configuration.SimulatedLoss = value; }
#else
            get { return 0.0f; }
            set { }
#endif
        }

        public override int BytesPerSecondReceived { get; set; }
        public override int BytesPerSecondSent { get; set; }

        public override void Introduce(Peer client, Peer target)
        {
            RemotePeer remoteClient = client as RemotePeer;
            RemotePeer remoteTarget = target as RemotePeer;

            if (remoteClient == null || remoteTarget == null)
            {
                return;
            }

            // As the client will receive the NatIntroductionSuccess message, send the target's endPoint as token:
            string token = remoteTarget.endPoint.ToString();
            localPeer.peer.Introduce(remoteTarget.intIPEndPoint, remoteTarget.extIPEndPoint, remoteClient.intIPEndPoint, remoteClient.extIPEndPoint, token);
        }

        internal LidgrenPeer FindPeerById(long id)
        {
            if (localPeer.SessionId == id)
            {
                return localPeer;
            }

            foreach (RemotePeer remotePeer in remotePeers)
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
            LidgrenEndPoint ep = endPoint as LidgrenEndPoint;

            foreach (RemotePeer remotePeer in remotePeers)
            {
                if (remotePeer.endPoint.Equals(ep))
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

        public override OutgoingMessage GetMessage(Peer recipient, SendDataOptions options, int channel)
        {
            LidgrenOutgoingMessage msg = outgoingMessagePool.Get();
            msg.recipient = recipient;
            msg.options = options;
            msg.channel = channel;
            return msg;
        }

        public override void SendMessage(OutgoingMessage message)
        {
            LidgrenOutgoingMessage msg = message as LidgrenOutgoingMessage;

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

        protected void InvokeReceive(NetBuffer buffer, Peer sender)
        {
            LidgrenIncomingMessage incomingMsg = incomingMessagePool.Get();
            incomingMsg.Backend = this;
            incomingMsg.Buffer = buffer;

            Listener.ReceiveMessage(incomingMsg, sender);

            incomingMessagePool.Recycle(incomingMsg);
        }

        public override void Update()
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
                if (msg.MessageType == NetIncomingMessageType.DiscoveryRequest)
                {
                    if (Listener.IsDiscoverableLocally)
                    {
                        Debug.WriteLine("Discovery request received");

                        LidgrenOutgoingMessage responseMsg = outgoingMessagePool.Get();
                        responseMsg.Write(localPeer.endPoint.ToString());
                        Listener.SessionPublicInfo.Pack(responseMsg);

                        NetOutgoingMessage response = localPeer.peer.CreateMessage();
                        response.Write(responseMsg.Buffer);
                        localPeer.peer.SendDiscoveryResponse(response, msg.SenderEndPoint);

                        outgoingMessagePool.Recycle(responseMsg);
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.ConnectionApproval)
                {
                    LidgrenEndPoint clientEndPoint = LidgrenEndPoint.Parse(msg.ReadString());

                    if (Listener.AllowConnectionFromClient(clientEndPoint))
                    {
                        NetOutgoingMessage hailMsg = localPeer.peer.CreateMessage();
                        hailMsg.Write(localPeer.endPoint.ToString());
                        hailMsg.Write(localPeer.InternalIPEndPoint);
                        msg.SenderConnection.Approve(hailMsg);
                    }
                    else
                    {
                        msg.SenderConnection.Deny("Connection denied");
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.NatIntroductionSuccess)
                {
                    Debug.WriteLine("Nat introduction successful received from " + msg.SenderEndPoint);
                    LidgrenEndPoint targetEndPoint = LidgrenEndPoint.Parse(msg.ReadString());

                    if (Listener.ConnectAsClientWhenIntroducedToTarget(targetEndPoint))
                    {
                        localPeer.Connect(msg.SenderEndPoint);
                    }
                }
                else if (msg.MessageType == NetIncomingMessageType.StatusChanged)
                {
                    if (msg.SenderConnection == null)
                    {
                        throw new NetworkException("Sender connection is null");
                    }

                    NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                    Debug.WriteLine("Status now: " + status + " (Reason: " + msg.ReadString() + ")");

                    if (status == NetConnectionStatus.Connected)
                    {
                        LidgrenEndPoint endPoint = LidgrenEndPoint.Parse(msg.SenderConnection.RemoteHailMessage.ReadString());
                        IPEndPoint internalIPEndPoint = msg.SenderConnection.RemoteHailMessage.ReadIPEndPoint();

                        RemotePeer remotePeer = new RemotePeer(msg.SenderConnection, endPoint, internalIPEndPoint);
                        msg.SenderConnection.Tag = remotePeer;
                        remotePeers.Add(remotePeer);
                        reportedConnections.Add(msg.SenderConnection);

                        Listener.PeerConnected(remotePeer);
                    }
                    else if (status == NetConnectionStatus.Disconnected)
                    {
                        RemotePeer disconnectedPeer = msg.SenderConnection.Tag as RemotePeer;

                        if (disconnectedPeer != null) // If null, host responded to connect then peer disconnected
                        {
                            remotePeers.Remove(disconnectedPeer);
                            reportedConnections.Remove(msg.SenderConnection);

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

                    InvokeReceive(msg, msg.SenderConnection.Tag as RemotePeer);
                }
                else
                {
                    // Error checking
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

                localPeer.peer.Recycle(msg);

                if (HasShutdown)
                {
                    return;
                }
            }
        }

        protected void UpdateMasterServerRegistration()
        {
            if (!Listener.IsDiscoverableOnline)
            {
                return;
            }

            DateTime currentTime = DateTime.Now;
            TimeSpan elapsedTime = currentTime - lastMasterServerRegistration;

            if (elapsedTime >= NetworkSessionSettings.MasterServerRegistrationInterval)
            {
                RegisterWithMasterServer();

                lastMasterServerRegistration = currentTime;
            }
        }

        protected void RegisterWithMasterServer()
        {
            if (!Listener.IsDiscoverableOnline)
            {
                return;
            }

            IPEndPoint masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            LidgrenOutgoingMessage msg = outgoingMessagePool.Get();
            msg.Write(localPeer.peer.Configuration.AppIdentifier);
            msg.Write((byte)MasterServerMessageType.RegisterHost);
            msg.Write(localPeer.endPoint.ToString());
            msg.Write(localPeer.InternalIPEndPoint);
            Listener.SessionPublicInfo.Pack(msg);

            NetOutgoingMessage request = localPeer.peer.CreateMessage();
            request.Write(msg.Buffer);
            localPeer.peer.SendUnconnectedMessage(request, masterServerEndPoint);

            outgoingMessagePool.Recycle(msg);

            Debug.WriteLine("Registering with master server (EndPoint: " + localPeer.endPoint + ", IPEndPoint: " + localPeer.EndPoint + ")");
        }

        protected void UnregisterWithMasterServer()
        {
            if (!Listener.IsDiscoverableOnline)
            {
                return;
            }

            IPEndPoint masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            NetOutgoingMessage msg = localPeer.peer.CreateMessage();
            msg.Write(localPeer.peer.Configuration.AppIdentifier);
            msg.Write((byte)MasterServerMessageType.UnregisterHost);
            msg.Write(localPeer.endPoint.ToString());
            localPeer.peer.SendUnconnectedMessage(msg, masterServerEndPoint);

            Debug.WriteLine("Unregistering with master server (EndPoint: " + localPeer.SessionId + ")");
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

        public override void Shutdown(string byeMessage)
        {
            if (HasShutdown)
            {
                return;
            }

            hasShutdown = true;

            UnregisterWithMasterServer();

            localPeer.Disconnect(byeMessage);
        }
    }
}
