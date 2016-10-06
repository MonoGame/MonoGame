using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal interface IResetable
    {
        void Reset();
    }

    internal class Pool<T> where T : IResetable, new()
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
                item.Reset();
            }

            return item;
        }

        public void Recycle(T item)
        {
            item.Reset();
            freeMessages.Add(item);
        }
    }

    internal class OutgoingMessage : IOutgoingMessage, IResetable
    {
        public OutgoingMessage()
        {
            this.Buffer = new NetBuffer();
        }

        internal NetBuffer Buffer { get; private set; }
        public IPeer Recipient { get; internal set; }
        public SendDataOptions Options { get; internal set; }
        public int Channel { get; internal set; }

        public void Reset()
        {
            Buffer.LengthBits = 0;
            Buffer.Position = 0;
            Recipient = null;
            Options = SendDataOptions.None;
            Channel = 0;
        }

        public void Write(string value)
        {
            Buffer.Write(value);
        }

        public void Write(byte[] value)
        {
            Buffer.Write(value);
        }

        public void Write(int value)
        {
            Buffer.Write(value);
        }

        public void Write(bool value)
        {
            Buffer.Write(value);
        }

        public void Write(byte value)
        {
            Buffer.Write(value);
        }

        public void Write(IPEndPoint value)
        {
            Buffer.Write(value);
        }

        public void Write(IPeer value)
        {
            Buffer.Write((value as ILidgrenPeer).Id);
        }
    }

    internal class IncomingMessage : IIncomingMessage, IResetable
    {
        internal LidgrenBackend Backend { get; set; }
        internal NetBuffer Buffer { get; set; }

        public void Reset()
        {
            Backend = null;
            Buffer = null;
        }

        public bool ReadBoolean()
        {
            return Buffer.ReadBoolean();
        }

        public byte ReadByte()
        {
            return Buffer.ReadByte();
        }

        public void ReadBytes(byte[] into, int offset, int length)
        {
            Buffer.ReadBytes(into, offset, length);
        }

        public int ReadInt()
        {
            return Buffer.ReadInt32();
        }

        public IPEndPoint ReadIPEndPoint()
        {
            return Buffer.ReadIPEndPoint();
        }

        public IPeer ReadPeer()
        {
            return Backend.FindPeerById(Buffer.ReadInt64());
        }

        public string ReadString()
        {
            return Buffer.ReadString();
        }
    }
    
    internal interface ILidgrenPeer : IPeer
    {
        long Id { get; }
    }

    internal class RemotePeer : ILidgrenPeer
    {
        internal NetConnection connection;

        public RemotePeer(NetConnection connection)
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

    internal class LocalPeer : ILidgrenPeer
    {
        internal NetPeer peer;

        public LocalPeer(NetPeer peer)
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

    internal class LidgrenBackend : ISessionBackend
    {
        private LocalPeer localPeer;
        private IList<RemotePeer> remotePeers;
        private List<NetConnection> reportedConnections;
        
        private Pool<OutgoingMessage> outgoingMessagePool;
        private Pool<IncomingMessage> incomingMessagePool;

        private DateTime lastTime;
        private int lastReceivedBytes;
        private int lastSentBytes;

        public LidgrenBackend(NetPeer peer)
        {
            this.localPeer = new LocalPeer(peer);
            this.remotePeers = new List<RemotePeer>();
            this.reportedConnections = new List<NetConnection>();
            
            this.outgoingMessagePool = new Pool<OutgoingMessage>();
            this.incomingMessagePool = new Pool<IncomingMessage>();

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

            foreach (RemotePeer remotePeer in remotePeers)
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
            return localPeer.peer.GetConnection(endPoint).Tag as RemotePeer;
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
                msg.Buffer.Position = 0;

                NetOutgoingMessage outgoingMsg = localPeer.peer.CreateMessage(msg.Buffer.LengthBytes);
                outgoingMsg.Write(msg.Buffer);

                if (msg.Recipient == null)
                {
                    // Send to all remote peers
                    if (reportedConnections.Count > 0)
                    {
                        localPeer.peer.SendMessage(outgoingMsg, reportedConnections, ToDeliveryMethod(msg.Options), msg.Channel);
                    }
                }
                else
                {
                    // Send to specific remote peer
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

                        OutgoingMessage outgoingMsg = outgoingMessagePool.Get();
                        outgoingMsg.Write((byte)Listener.SessionType);
                        Listener.SessionProperties.Pack(outgoingMsg);
                        outgoingMsg.Write(Listener.MaxGamers);
                        outgoingMsg.Write(Listener.PrivateGamerSlots);
                        outgoingMsg.Write(Listener.CurrentGamerCount);
                        outgoingMsg.Write(Listener.HostGamertag);
                        outgoingMsg.Write(Listener.OpenPrivateGamerSlots);
                        outgoingMsg.Write(Listener.OpenPublicGamerSlots);

                        outgoingMsg.Buffer.Position = 0;

                        NetOutgoingMessage response = localPeer.peer.CreateMessage();
                        response.Write(outgoingMsg.Buffer);
                        localPeer.peer.SendDiscoveryResponse(response, msg.SenderEndPoint);

                        outgoingMessagePool.Recycle(outgoingMsg);
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
                            RemotePeer remotePeer = new RemotePeer(msg.SenderConnection);
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
