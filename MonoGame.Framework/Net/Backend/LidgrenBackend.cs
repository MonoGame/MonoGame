using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend
{
    internal class LidgrenInternalMessage : IOutgoingMessage
    {
        internal NetBuffer buffer = new NetBuffer();
        internal LidgrenLocalPeer recipient;
        internal SendDataOptions options;
        internal int channel;

        public IPeer Recipient { get { return recipient; } }
        public SendDataOptions Options { get { return options; } }
        public int Channel { get { return channel; } }

        public void Write(int value)
        {
            buffer.Write(value);
        }

        public void Write(byte[] value)
        {
            buffer.Write(value);
        }

        public void Write(string value)
        {
            buffer.Write(value);
        }

        public void Write(byte value)
        {
            buffer.Write(value);
        }

        public void Write(bool value)
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
    internal class LidgrenOutgoingMessage : IOutgoingMessage
    {
        internal NetOutgoingMessage buffer;
        internal LidgrenRemotePeer recipient;
        internal SendDataOptions options;
        internal int channel;

        public IPeer Recipient { get { return recipient; } }
        public SendDataOptions Options { get { return options; } }
        public int Channel { get { return channel; } }

        public void Write(int value)
        {
            buffer.Write(value);
        }

        public void Write(byte[] value)
        {
            buffer.Write(value);
        }

        public void Write(string value)
        {
            buffer.Write(value);
        }

        public void Write(byte value)
        {
            buffer.Write(value);
        }

        public void Write(bool value)
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
        private LidgrenBackend backend;
        internal NetBuffer buffer;

        public LidgrenIncomingMessage(LidgrenBackend backend)
        {
            this.backend = backend;
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

    internal class LidgrenOutgoingMessagePool<T> where T : IOutgoingMessage, new()
    {
        private IList<T> freeMessages = new List<T>();

        public T Get()
        {
            T msg;

            if (freeMessages.Count > 0)
            {
                msg = freeMessages[freeMessages.Count - 1];
            }
            else
            {
                msg = new T();
            }

            return msg;
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

        private LidgrenOutgoingMessagePool<LidgrenInternalMessage> internalMessagePool;
        private LidgrenOutgoingMessagePool<LidgrenOutgoingMessage> outgoingMessagePool;
        private LidgrenIncomingMessage incomingMessage;

        private DateTime lastTime;
        private int lastReceivedBytes;
        private int lastSentBytes;

        public LidgrenBackend(NetPeer peer)
        {
            this.localPeer = new LidgrenLocalPeer(peer);
            this.remotePeers = new List<LidgrenRemotePeer>();

            this.internalMessagePool = new LidgrenOutgoingMessagePool<LidgrenInternalMessage>();
            this.outgoingMessagePool = new LidgrenOutgoingMessagePool<LidgrenOutgoingMessage>();
            this.incomingMessage = new LidgrenIncomingMessage(this);

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

        public IOutgoingMessage GetMessageBuffer(IPeer recipient, SendDataOptions options, int channel)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException("recipient");
            }

            if (recipient == localPeer)
            {
                LidgrenInternalMessage msg = internalMessagePool.Get();
                msg.buffer.LengthBits = 0;
                msg.recipient = recipient as LidgrenLocalPeer;
                msg.options = options;
                msg.channel = channel;
                return msg;
            }
            else
            {
                LidgrenOutgoingMessage msg = outgoingMessagePool.Get();
                msg.buffer = localPeer.peer.CreateMessage();
                msg.recipient = recipient as LidgrenRemotePeer;
                msg.options = options;
                msg.channel = channel;
                return msg;
            }
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

        public void SendToPeer(IOutgoingMessage data)
        {
            if (data is LidgrenInternalMessage)
            {
                LidgrenInternalMessage msg = data as LidgrenInternalMessage;

                incomingMessage.buffer = msg.buffer;
                Listener.Receive(incomingMessage, localPeer);
            }
            else if (data is LidgrenOutgoingMessage)
            {
                LidgrenOutgoingMessage msg = data as LidgrenOutgoingMessage;

                localPeer.peer.SendMessage(msg.buffer, msg.recipient.connection, ToDeliveryMethod(msg.Options), msg.Channel);
            }
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

                            Listener.PeerDisconnected(disconnectedPeer);
                        }
                        break;
                    // Internal messages
                    case NetIncomingMessageType.Data:
                        if (msg.SenderConnection == null)
                        {
                            throw new NetworkException("Sender connection is null");
                        }

                        incomingMessage.buffer = msg;
                        Listener.Receive(incomingMessage, msg.SenderConnection.Tag as LidgrenRemotePeer);
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
