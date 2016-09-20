using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend
{
    internal class LidgrenOutgoingMessage : IOutgoingMessage
    {
        internal NetBuffer buffer;

        public LidgrenOutgoingMessage()
        {
            this.buffer = null;
            this.Recipient = null;
            this.Options = SendDataOptions.None;
            this.Channel = 0;
        }
        
        public IPeer Recipient { get; internal set; }
        public SendDataOptions Options { get; internal set; }
        public int Channel { get; internal set; }

        public void Write(string value)
        {
            buffer.Write(value);
        }

        public void Write(byte[] value)
        {
            buffer.Write(value);
        }

        public void Write(int value)
        {
            buffer.Write(value);
        }

        public void Write(bool value)
        {
            buffer.Write(value);
        }

        public void Write(byte value)
        {
            buffer.Write(value);
        }

        public void Write(IPEndPoint value)
        {
            buffer.Write(value);
        }

        public void Write(IPeer value)
        {
            buffer.Write((value as ILidgrenPeer).Id);
        }
    }
    internal class LidgrenIncomingMessage : IIncomingMessage
    {
        internal LidgrenBackend backend;
        internal NetBuffer buffer;

        public LidgrenIncomingMessage()
        {
            this.backend = null;
            this.buffer = null;
        }

        public bool ReadBoolean()
        {
            return buffer.ReadBoolean();
        }

        public byte ReadByte()
        {
            return buffer.ReadByte();
        }

        public void ReadBytes(byte[] into, int offset, int length)
        {
            buffer.ReadBytes(into, offset, length);
        }

        public int ReadInt()
        {
            return buffer.ReadInt32();
        }

        public IPEndPoint ReadIPEndPoint()
        {
            return buffer.ReadIPEndPoint();
        }

        public IPeer ReadPeer()
        {
            return backend.FindPeerById(buffer.ReadInt64());
        }

        public string ReadString()
        {
            return buffer.ReadString();
        }
    }

    internal class Pool<T> where T : new()
    {
        private IList<T> freeMessages = new List<T>();

        public T Get()
        {
            T item;

            if (freeMessages.Count > 0)
            {
                int lastIndex = freeMessages.Count - 1;
                item = freeMessages[lastIndex];
                freeMessages.RemoveAt(lastIndex);
            }
            else
            {
                item = new T();
            }

            return item;
        }

        public void Recycle(T item)
        {
            freeMessages.Add(item);
        }
    }
    
    internal interface ILidgrenPeer : IPeer
    {
        long Id { get; }
    }

    internal class LidgrenRemotePeer : ILidgrenPeer
    {
        internal NetConnection connection;

        public LidgrenRemotePeer(NetConnection connection)
        {
            this.connection = connection;
        }

        public long Id { get { return connection.RemoteUniqueIdentifier; } }
        public IPEndPoint EndPoint { get { return connection.RemoteEndPoint; } }
        public TimeSpan RoundtripTime { get { return TimeSpan.FromSeconds(connection.AverageRoundtripTime); } }
        public object Tag { get; set; }

        public void Disconnect(string byeMessage)
        {
            connection.Disconnect(byeMessage);
        }
    }

    internal class LidgrenLocalPeer : ILidgrenPeer
    {
        internal NetPeer peer;

        public LidgrenLocalPeer(NetPeer peer)
        {
            this.peer = peer;
        }

        public long Id { get { return peer.UniqueIdentifier; } }
        public IPEndPoint EndPoint { get { throw new InvalidOperationException(); } }
        public TimeSpan RoundtripTime { get { return TimeSpan.Zero; } }
        public object Tag { get; set; }

        public void Disconnect(string byeMessage)
        {
            peer.Shutdown(byeMessage);
        }
    }

    internal class LidgrenBackend : IBackend
    {
        private LidgrenLocalPeer localPeer;
        private IList<LidgrenRemotePeer> remotePeers;
        private List<NetConnection> reportedConnections;

        private Pool<NetBuffer> bufferPool;
        private Pool<LidgrenOutgoingMessage> outgoingMessagePool;
        private Pool<LidgrenIncomingMessage> incomingMessagePool;

        private DateTime lastTime;
        private int lastReceivedBytes;
        private int lastSentBytes;

        public LidgrenBackend(NetPeer peer)
        {
            this.localPeer = new LidgrenLocalPeer(peer);
            this.remotePeers = new List<LidgrenRemotePeer>();
            this.reportedConnections = new List<NetConnection>();

            this.bufferPool = new Pool<NetBuffer>();
            this.outgoingMessagePool = new Pool<LidgrenOutgoingMessage>();
            this.incomingMessagePool = new Pool<LidgrenIncomingMessage>();

            this.lastTime = DateTime.Now;
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

            foreach (LidgrenRemotePeer remotePeer in remotePeers)
            {
                if (remotePeer.Id == id)
                {
                    return remotePeer;
                }
            }

            return null;
        }

        public void Connect(IPEndPoint endPoint)
        {
            localPeer.peer.Connect(endPoint);
        }

        public IPeer FindRemotePeerByEndPoint(IPEndPoint endPoint)
        {
            return localPeer.peer.GetConnection(endPoint).Tag as LidgrenRemotePeer;
        }

        public bool IsConnectedToEndPoint(IPEndPoint endPoint)
        {
            return localPeer.peer.GetConnection(endPoint) != null;
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
            NetBuffer buffer;

            if (recipient == localPeer)
            {
                buffer = bufferPool.Get();
            }
            else
            {
                buffer = localPeer.peer.CreateMessage();
            }

            LidgrenOutgoingMessage msg = outgoingMessagePool.Get();
            msg.buffer = buffer;
            msg.Recipient = recipient;
            msg.Options = options;
            msg.Channel = channel;
            return msg;
        }

        public void SendMessage(IOutgoingMessage data)
        {
            LidgrenOutgoingMessage msg = data as LidgrenOutgoingMessage;

            if (msg == null)
            {
                throw new NetworkException("Not possible to mix backends");
            }

            if (msg.Recipient == null)
            {
                // Prepare for reading
                long prevPos = msg.buffer.Position;
                msg.buffer.Position = 0;

                InvokeReceive(msg.buffer, localPeer);

                // Prepare for sending
                msg.buffer.Position = prevPos;

                if (reportedConnections.Count > 0)
                {
                    NetOutgoingMessage outgoingMsg = msg.buffer as NetOutgoingMessage;
                    localPeer.peer.SendMessage(outgoingMsg, reportedConnections, ToDeliveryMethod(msg.Options), msg.Channel);
                }
            }
            else if (msg.Recipient == localPeer)
            {
                NetBuffer buffer = msg.buffer;
                buffer.Position = 0;

                InvokeReceive(buffer, localPeer);

                buffer.LengthBits = 0;
                buffer.Position = 0;
                bufferPool.Recycle(buffer);
            }
            else
            {
                NetOutgoingMessage outgoingMsg = msg.buffer as NetOutgoingMessage;
                LidgrenRemotePeer recipient = msg.Recipient as LidgrenRemotePeer;
                localPeer.peer.SendMessage(outgoingMsg, recipient.connection, ToDeliveryMethod(msg.Options), msg.Channel);
            }

            msg.buffer = null;
            msg.Recipient = null;
            msg.Options = SendDataOptions.None;
            msg.Channel = 0;
            outgoingMessagePool.Recycle(msg);
        }

        private void InvokeReceive(NetBuffer buffer, IPeer sender)
        {
            LidgrenIncomingMessage incomingMsg = incomingMessagePool.Get();
            incomingMsg.backend = this;
            incomingMsg.buffer = buffer;

            Listener.ReceiveMessage(incomingMsg, sender);

            incomingMsg.backend = null;
            incomingMsg.buffer = null;
            incomingMessagePool.Recycle(incomingMsg);
        }

        public void Update()
        {
            NetIncomingMessage msg;
            while ((msg = localPeer.peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    // Discovery
                    case NetIncomingMessageType.DiscoveryRequest:
                        if (!Listener.ShouldSendDiscoveryResponse)
                        {
                            throw new NetworkException("Discovery request received when not host");
                        }

                        Debug.WriteLine("Discovery request received");
                        NetOutgoingMessage response = localPeer.peer.CreateMessage();
                        response.Write((byte)Listener.SessionType);
                        Listener.SessionProperties.Send(response);

                        response.Write(Listener.MaxGamers);
                        response.Write(Listener.PrivateGamerSlots);
                        response.Write(Listener.CurrentGamerCount);
                        response.Write(Listener.HostGamertag);
                        response.Write(Listener.OpenPrivateGamerSlots);
                        response.Write(Listener.OpenPublicGamerSlots);
                        
                        localPeer.peer.SendDiscoveryResponse(response, msg.SenderEndPoint);
                        break;
                    // Peer state changes
                    case NetIncomingMessageType.StatusChanged:
                        if (msg.SenderConnection == null)
                        {
                            throw new NetworkException("Sender connection is null");
                        }

                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        Debug.WriteLine("Status now: " + status + "; Reason: " + msg.ReadString());

                        if (status == NetConnectionStatus.Connected)
                        {
                            LidgrenRemotePeer remotePeer = new LidgrenRemotePeer(msg.SenderConnection);
                            msg.SenderConnection.Tag = remotePeer;
                            remotePeers.Add(remotePeer);
                            reportedConnections.Add(msg.SenderConnection);

                            Listener.PeerConnected(remotePeer);
                        }
                        else if (status == NetConnectionStatus.Disconnected)
                        {
                            LidgrenRemotePeer disconnectedPeer = msg.SenderConnection.Tag as LidgrenRemotePeer;

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

                        InvokeReceive(msg, msg.SenderConnection.Tag as LidgrenRemotePeer);
                        break;
                    // Unconnected data
                    case NetIncomingMessageType.UnconnectedData:
                        Debug.WriteLine("Unconnected data received!");
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

        public void UpdateStatistics()
        {
            DateTime currentTime = DateTime.Now;
            int receivedBytes = localPeer.peer.Statistics.ReceivedBytes;
            int sentBytes = localPeer.peer.Statistics.SentBytes;
            double elapsedSeconds = (currentTime - lastTime).TotalSeconds;

            if (elapsedSeconds >= 1.0)
            {
                BytesPerSecondReceived = (int)Math.Round((receivedBytes - lastReceivedBytes) / elapsedSeconds);
                BytesPerSecondSent = (int)Math.Round((sentBytes - lastSentBytes) / elapsedSeconds);

                //Debug.WriteLine("Statistics: BytesPerSecondReceived = " + BytesPerSecondReceived);
                //Debug.WriteLine("Statistics: BytesPerSecondSent     = " + BytesPerSecondSent);

                lastTime = currentTime;
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
