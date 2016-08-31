using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using Microsoft.Xna.Framework.Net.Messages;
using Microsoft.Xna.Framework.GamerServices;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class NetworkSession : IDisposable
    {
        private const int Port = 14242;
        private const int DiscoveryTime = 1000;
        private const int JoinTime = 1000;
        public const int MaxPreviousGamers = 10;
        public const int MaxSupportedGamers = 64;

        internal static NetworkSession Session = null;

        internal static NetPeerConfiguration CreateNetPeerConfig(bool specifyPort)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("MonoGameApp");

            config.Port = specifyPort ? Port : 0;
            config.AcceptIncomingConnections = true;

            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            return config;
        }

        public static NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            if (Session != null)
            {
                throw new InvalidOperationException("Only one NetworkSession allowed");
            }
            if (maxGamers < 2 || maxGamers > MaxSupportedGamers)
            {
                throw new ArgumentOutOfRangeException("maxGamers must be in the range [2, " + MaxSupportedGamers + "]");
            }
            if (privateGamerSlots < 0 || privateGamerSlots > maxGamers)
            {
                throw new ArgumentOutOfRangeException("privateGamerSlots must be in the range[0, maxGamers]");
            }

            NetPeer peer = new NetPeer(CreateNetPeerConfig(true));

            try
            {
                peer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Internal error", e);
            }

            Session = new NetworkSession(peer, null, maxGamers, privateGamerSlots, sessionType, sessionProperties, localGamers);
            return Session;
        }

        // ArgumentOutOfRangeException if maxLocalGamers is < 1 or > 4
        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            if (sessionType == NetworkSessionType.Local)
            {
                throw new ArgumentException("Find cannot be used with NetworkSessionType.Local");
            }

            // Send discover requests on subnet
            NetPeer discoverPeer = new NetPeer(CreateNetPeerConfig(false));
            discoverPeer.Start();
            discoverPeer.DiscoverLocalPeers(Port);

            Thread.Sleep(DiscoveryTime);

            // Get list of answers
            List<AvailableNetworkSession> availableSessions = new List<AvailableNetworkSession>();

            NetIncomingMessage msg;
            while ((msg = discoverPeer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        // Ignore own message
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        NetworkSessionType remoteSessionType = (NetworkSessionType)msg.ReadByte();

                        int maxGamers = msg.ReadInt32();
                        int privateGamerSlots = msg.ReadInt32();
                        int currentGamerCount = msg.ReadInt32();
                        string hostGamertag = msg.ReadString();
                        int openPrivateGamerSlots = msg.ReadInt32();
                        int openPublicGamerSlots = msg.ReadInt32();
                        NetworkSessionProperties sessionProperties = null;

                        if (remoteSessionType == sessionType)
                        {
                            availableSessions.Add(new AvailableNetworkSession(msg.SenderEndPoint, localGamers, maxGamers, privateGamerSlots, sessionType, currentGamerCount, hostGamertag, openPrivateGamerSlots, openPublicGamerSlots, sessionProperties));
                        }
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

                discoverPeer.Recycle(msg);
            }

            discoverPeer.Shutdown("Discovery peer done");

            return new AvailableNetworkSessionCollection(availableSessions);
        }

        public static NetworkSession Join(AvailableNetworkSession availableSession)
        {
            if (Session != null)
            {
                throw new InvalidOperationException("Only one NetworkSession allowed");
            }
            if (availableSession == null)
            {
                throw new ArgumentNullException("availableSession");
            }
            // TODO: NetworkSessionJoinException if availableSession full/not joinable/cannot be found

            NetPeer peer = new NetPeer(CreateNetPeerConfig(false));
            peer.Start();
            peer.Connect(availableSession.remoteEndPoint);

            Thread.Sleep(JoinTime);

            if (peer.ConnectionsCount == 0)
            {
                throw new NetworkSessionJoinException("Connection failed", NetworkSessionJoinError.SessionNotFound);
            }

            NetConnection hostConnection = peer.GetConnection(availableSession.remoteEndPoint);
            int maxGamers = availableSession.maxGamers;
            int privateGamerSlots = availableSession.privateGamerSlots;
            NetworkSessionType sessionType = availableSession.sessionType;
            NetworkSessionProperties sessionProperties = availableSession.SessionProperties;
            IEnumerable<SignedInGamer> localGamers = availableSession.localGamers;

            Session = new NetworkSession(peer, hostConnection, maxGamers, privateGamerSlots, sessionType, sessionProperties, localGamers);
            return Session;
        }

        internal NetPeer peer;
        internal NetworkMachine machine;
        internal NetConnection hostConnection;

        internal IList<SignedInGamer> pendingSignedInGamers;
        internal ICollection<IPEndPoint> pendingEndPoints;

        // Host stores which connections were open when a particular peer connected
        internal Dictionary<NetConnection, ICollection<NetConnection>> pendingPeerConnections = new Dictionary<NetConnection, ICollection<NetConnection>>();

        private byte uniqueIdCount;
        private IList<NetworkGamer> allGamers;
        private IList<NetworkGamer> allRemoteGamers;

        internal PacketPool packetPool;
        private NetBuffer internalBuffer;
        private DateTime lastTime;
        private int lastReceivedBytes;
        private int lastSentBytes;

        internal NetworkSession(NetPeer peer, NetConnection hostConnection, int maxGamers, int privateGamerSlots, NetworkSessionType type, NetworkSessionProperties properties, IEnumerable<SignedInGamer> signedInGamers)
        {
            this.peer = peer;
            this.machine = new NetworkMachine(this, null, hostConnection == null);
            this.hostConnection = hostConnection;

            this.pendingSignedInGamers = new List<SignedInGamer>(signedInGamers);

            if (hostConnection == null)
            {
                // Initialize empty pending end point list so that the host is approved automatically
                this.pendingEndPoints = new List<IPEndPoint>();
            }

            this.allGamers = new List<NetworkGamer>();
            this.allRemoteGamers = new List<NetworkGamer>();

            this.AllGamers = new GamerCollection<NetworkGamer>(this.allGamers);
            this.AllowHostMigration = false;
            this.AllowJoinInProgress = false;
            this.BytesPerSecondReceived = 0;
            this.BytesPerSecondSent = 0;
            this.IsDisposed = false;
            this.LocalGamers = this.machine.LocalGamers;
            this.MaxGamers = maxGamers;
            this.PrivateGamerSlots = privateGamerSlots;
            this.RemoteGamers = new GamerCollection<NetworkGamer>(this.allRemoteGamers);
            this.SessionProperties = properties;
            this.SessionState = NetworkSessionState.Lobby;
            this.SessionType = type;
            this.SimulatedLatency = TimeSpan.Zero;
            this.SimulatedPacketLoss = 0.0f;

            this.packetPool = new PacketPool();
            this.internalBuffer = new NetBuffer();
            this.lastTime = DateTime.Now;
            this.lastReceivedBytes = this.peer.Statistics.ReceivedBytes;
            this.lastSentBytes = this.peer.Statistics.SentBytes;

            // Store machine in peer tag
            this.peer.Tag = this.machine;
        }

        public GamerCollection<NetworkGamer> AllGamers { get; }
        public bool AllowHostMigration { get; set; } // any peer can get, only host can set
        public bool AllowJoinInProgress { get; set; } // any peer can get, only host can set
        public int BytesPerSecondReceived { get; internal set; }
        public int BytesPerSecondSent { get; internal set; }

        internal NetworkMachine HostMachine
        {
            get { return IsHost ? machine : hostConnection.Tag as NetworkMachine; }
        }

        public NetworkGamer Host
        {
            get
            {
                NetworkMachine hostMachine = HostMachine;

                if (hostMachine == null || hostMachine.Gamers.Count == 0)
                {
                    return null;
                }

                return hostMachine.Gamers[0];
            }
        }

        public bool IsDisposed { get; private set; }

        public bool IsEveryoneReady
        {
            get
            {
                foreach (NetworkGamer gamer in allGamers)
                {
                    if (!gamer.IsReady)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool IsHost { get { return machine.IsHost; } }
        public GamerCollection<LocalNetworkGamer> LocalGamers { get; }
        public int MaxGamers { get; set; } // only host can set
        public GamerCollection<NetworkGamer> PreviousGamers { get; }
        public int PrivateGamerSlots { get; set; } // only host can set
        public GamerCollection<NetworkGamer> RemoteGamers { get; }
        public NetworkSessionProperties SessionProperties { get; } // should be synchronized
        public NetworkSessionState SessionState { get; internal set; }
        public NetworkSessionType SessionType { get; }

        public TimeSpan SimulatedLatency // TODO: Should be applied even to local messages
        {
            get { return TimeSpan.FromSeconds(peer.Configuration.SimulatedRandomLatency); }
            set { peer.Configuration.SimulatedRandomLatency = (float)value.TotalSeconds; }
        }

        public float SimulatedPacketLoss // TODO: Should be applied even to local messages
        {
            get { return peer.Configuration.SimulatedLoss; }
            set { peer.Configuration.SimulatedLoss = value; }
        }

        internal int CurrentGamerCount { get { return allGamers.Count; } }
        internal string HostGamertag { get { return machine.LocalGamers.Count > 0 ? machine.LocalGamers[0].Gamertag : "Game starting up..."; } }
        internal int OpenPrivateGamerSlots { get { return PrivateGamerSlots; } }
        internal int OpenPublicGamerSlots { get { return MaxGamers - PrivateGamerSlots - CurrentGamerCount; } }

        public event EventHandler<GamerJoinedEventArgs> GamerJoined;
        public event EventHandler<GamerLeftEventArgs> GamerLeft;
        public event EventHandler<GameStartedEventArgs> GameStarted;
        public event EventHandler<GameEndedEventArgs> GameEnded;
        public event EventHandler<HostChangedEventArgs> HostChanged;
        public static event EventHandler<InviteAcceptedEventArgs> InviteAccepted;
        public event EventHandler<NetworkSessionEndedEventArgs> SessionEnded;
        public event EventHandler<WriteLeaderboardsEventArgs> WriteArbitratedLeaderboard; // No documentation exists
        public event EventHandler<WriteLeaderboardsEventArgs> WriteTrueSkill; // No documentation exists
        public event EventHandler<WriteLeaderboardsEventArgs> WriteUnarbitratedLeaderboard; // No documentation exists

        internal void InvokeGamerJoinedEvent(GamerJoinedEventArgs args)
        {
            GamerJoined?.Invoke(this, args);
        }

        internal void InvokeGamerLeftEvent(GamerLeftEventArgs args)
        {
            GamerLeft?.Invoke(this, args);
        }

        internal void InvokeGameStartedEvent(GameStartedEventArgs args)
        {
            GameStarted?.Invoke(this, args);
        }

        internal void InvokeGameEndedEvent(GameEndedEventArgs args)
        {
            GameEnded?.Invoke(this, args);
        }

        internal void InvokeSessionEnded(NetworkSessionEndedEventArgs args)
        {
            SessionEnded?.Invoke(this, args);
        }

        public void AddLocalGamer(SignedInGamer signedInGamer)
        {
            if (machine.FindLocalGamerBySignedInGamer(signedInGamer) != null)
            {
                return;
            }

            pendingSignedInGamers.Add(signedInGamer);

            Send(new GamerJoinRequestMessageSender(), HostMachine);
        }

        public void StartGame()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("NetworkSession");
            }
            if (!IsHost)
            {
                throw new InvalidOperationException("Only the host can perform this action");
            }
            if (SessionState != NetworkSessionState.Lobby)
            {
                throw new InvalidOperationException("The game can only be started from the lobby state");
            }
            if (!IsEveryoneReady)
            {
                throw new InvalidOperationException("Not all players are ready"); // TODO: See if this is the expected behavior
            }

            Send(new StartGameMessageSender());
        }

        public void EndGame()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("NetworkSession");
            }
            if (!IsHost)
            {
                throw new InvalidOperationException("Only the host can perform this action");
            }
            if (SessionState != NetworkSessionState.Playing)
            {
                throw new InvalidOperationException("The game can only end from the playing state");
            }

            Send(new EndGameMessageSender());
        }

        public void ResetReady() // only host
        {
            throw new NotImplementedException();
        }

        public NetworkGamer FindGamerById(byte gamerId)
        {
            foreach (NetworkGamer gamer in AllGamers)
            {
                if (gamer.Id == gamerId)
                {
                    return gamer;
                }
            }

            return null;
        }

        internal void AddGamer(NetworkGamer gamer)
        {
            gamer.Machine.AddGamer(gamer);

            allGamers.Add(gamer);
            if (!gamer.IsLocal)
            {
                allRemoteGamers.Add(gamer);
            }

            InvokeGamerJoinedEvent(new GamerJoinedEventArgs(gamer));
        }

        internal void RemoveGamer(NetworkGamer gamer)
        {
            gamer.Machine.RemoveGamer(gamer);

            allGamers.Remove(gamer);
            if (!gamer.IsLocal)
            {
                allRemoteGamers.Remove(gamer);
            }

            InvokeGamerLeftEvent(new GamerLeftEventArgs(gamer));
        }

        internal void RemoveGamersLocally()
        {
            machine.RemoveGamersLocally();

            foreach (NetConnection connection in peer.Connections)
            {
                NetworkMachine remoteMachine = connection.Tag as NetworkMachine;

                remoteMachine.RemoveGamersLocally();
            }
        }

        internal bool GetNewUniqueId(out byte id)
        {
            // TODO: Make foolproof
            if (uniqueIdCount >= 255)
            {
                id = 255;
                return false;
            }

            id = uniqueIdCount;
            uniqueIdCount++;
            return true;
        }

        internal bool IsConnectedToEndPoint(IPEndPoint endPoint)
        {
            return peer.GetConnection(endPoint) != null;
        }

        private string MachineOwnerName(NetworkMachine machine)
        {
            if (machine.IsLocal)
            {
                if (machine.IsHost)
                {
                    return "self (host)";
                }
                else
                {
                    return "self";
                }
            }
            else if (machine.IsHost)
            {
                return "host";
            }
            else
            {
                return "peer";
            }
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

        private void EncodeMessage(IInternalMessageSender message, NetBuffer output)
        {
            output.Write((byte)message.MessageType);

            message.Send(output, machine);
        }

        internal void Send(IInternalMessageSender message)
        {
            if (message.MessageType != InternalMessageType.User)
            {
                Debug.WriteLine("Sending " + message.MessageType + " to all peers...");
            }

            // Send to all peers
            if (peer.Connections.Count > 0)
            {
                NetOutgoingMessage msg = peer.CreateMessage();
                EncodeMessage(message, msg);
                peer.SendMessage(msg, peer.Connections, ToDeliveryMethod(message.Options), message.SequenceChannel);
            }

            // Send to self (Should be done last since then all Send() calls happen before any Receive() call)
            Send(message, machine);
        }

        internal void Send(IInternalMessageSender message, NetworkMachine recipient)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException("recipient");
            }

            if (message.MessageType != InternalMessageType.User)
            {
                Debug.WriteLine("Sending " + message.MessageType + " to " + MachineOwnerName(recipient) + "...");
            }

            if (recipient.IsLocal)
            {
                internalBuffer.LengthBits = 0;
                EncodeMessage(message, internalBuffer);

                internalBuffer.Position = 0;
                Receive(internalBuffer, machine);
            }
            else
            {
                NetOutgoingMessage msg = peer.CreateMessage();
                EncodeMessage(message, msg);
                peer.SendMessage(msg, recipient.connection, ToDeliveryMethod(message.Options), message.SequenceChannel);
            }
        }

        private void Receive(NetBuffer input, NetworkMachine sender)
        {
            byte messageType = input.ReadByte();

            if ((InternalMessageType)messageType != InternalMessageType.User)
            {
                Debug.WriteLine("Receiving " + (InternalMessageType)messageType + " from " + MachineOwnerName(sender) + "...");
            }

            Type receiverToInstantiate = InternalMessage.MessageToReceiverTypeMap[messageType];
            IInternalMessageReceiver receiver = (IInternalMessageReceiver)Activator.CreateInstance(receiverToInstantiate);
            receiver.Receive(input, machine, sender);
        }

        public void Update()
        {
            // Recycle inbound packets from last frame
            foreach (LocalNetworkGamer localGamer in machine.LocalGamers)
            {
                localGamer.RecycleInboundPackets();
            }

            // Send accumulated outbound packets -> will create new inbound packets
            foreach (LocalNetworkGamer localGamer in machine.LocalGamers)
            {
                foreach (OutboundPacket outboundPacket in localGamer.OutboundPackets)
                {
                    IInternalMessageSender userMessage = new UserMessageSender(outboundPacket.sender, outboundPacket.recipient, outboundPacket.options, outboundPacket.packet);

                    if (outboundPacket.recipient == null)
                    {
                        Send(userMessage);
                    }
                    else
                    {
                        Send(userMessage, outboundPacket.recipient.Machine);
                    }
                }

                localGamer.RecycleOutboundPackets();
            }

            // Handle incoming messages -> will create new inbound packets
            NetIncomingMessage msg;
            while ((msg = peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    // Discovery
                    case NetIncomingMessageType.DiscoveryRequest:
                        Debug.WriteLine("Discovery request received");
                        NetOutgoingMessage response = peer.CreateMessage();
                        response.Write((byte)SessionType);
                        response.Write(MaxGamers);
                        response.Write(PrivateGamerSlots);
                        response.Write(CurrentGamerCount);
                        response.Write(HostGamertag);
                        response.Write(OpenPrivateGamerSlots);
                        response.Write(OpenPublicGamerSlots);
                        peer.SendDiscoveryResponse(response, msg.SenderEndPoint);
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
                            // Create a pending network machine
                            NetworkMachine senderMachine = new NetworkMachine(this, msg.SenderConnection, msg.SenderConnection == hostConnection);
                            msg.SenderConnection.Tag = senderMachine;

                            if (!machine.IsPending)
                            {
                                Send(new NoLongerPendingMessageSender(), senderMachine);
                            }

                            if (IsHost)
                            {
                                // Save snapshot of current connections and send them to new peer
                                ICollection<NetConnection> requestedConnections = new HashSet<NetConnection>(peer.Connections);
                                requestedConnections.Remove(msg.SenderConnection);
                                pendingPeerConnections.Add(msg.SenderConnection, requestedConnections);

                                Send(new InitializePendingMessageSender(requestedConnections), senderMachine);
                            }
                        }

                        if (status == NetConnectionStatus.Disconnected)
                        {
                            // Remove gamers
                            NetworkMachine disconnectedMachine = msg.SenderConnection.Tag as NetworkMachine;

                            disconnectedMachine.RemoveGamersLocally();

                            if (IsHost)
                            {
                                // If disconnected peer was pending, remove it
                                pendingPeerConnections.Remove(msg.SenderConnection);

                                // Update pending peers
                                foreach (var pendingPeer in pendingPeerConnections)
                                {
                                    NetworkMachine pendingMachine = pendingPeer.Key.Tag as NetworkMachine;

                                    if (!pendingMachine.IsPending)
                                    {
                                        continue;
                                    }

                                    if (pendingPeer.Value.Contains(msg.SenderConnection))
                                    {
                                        pendingPeer.Value.Remove(msg.SenderConnection);

                                        Send(new InitializePendingMessageSender(pendingPeer.Value), pendingMachine);
                                    }
                                }
                            }
                            else
                            {
                                if (msg.SenderConnection == hostConnection)
                                {
                                    // TODO: Host migration
                                    End(NetworkSessionEndReason.HostEndedSession);
                                }
                            }
                        }
                        break;
                    // Unconnected data
                    case NetIncomingMessageType.UnconnectedData:
                        Debug.WriteLine("Unconnected data received!");
                        break;
                    // Custom data
                    case NetIncomingMessageType.Data:
                        if (msg.SenderConnection == null)
                        {
                            throw new NetworkException("Sender connection is null");
                        }

                        Receive(msg, msg.SenderConnection.Tag as NetworkMachine);
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

                peer.Recycle(msg);
            }

            // Handle pending machine
            if (machine.IsPending && pendingEndPoints != null)
            {
                bool done = true;

                foreach (IPEndPoint endPoint in pendingEndPoints)
                {
                    if (!IsConnectedToEndPoint(endPoint))
                    {
                        done = false;
                    }
                }

                if (done)
                {
                    Send(new NoLongerPendingMessageSender());

                    // Handle pending signed in gamers
                    if (pendingSignedInGamers.Count > 0)
                    {
                        Send(new GamerJoinRequestMessageSender(), HostMachine);
                    }
                }
            }

            UpdateStatistics();
        }

        private void UpdateStatistics()
        {
            DateTime currentTime = DateTime.Now;
            int receivedBytes = peer.Statistics.ReceivedBytes;
            int sentBytes = peer.Statistics.SentBytes;
            double elapsedSeconds = (currentTime - lastTime).TotalSeconds;

            if (elapsedSeconds >= 1.0)
            {
                BytesPerSecondReceived = (int)Math.Round((receivedBytes - lastReceivedBytes) / elapsedSeconds);
                BytesPerSecondSent = (int)Math.Round((sentBytes - lastSentBytes) / elapsedSeconds);

                lastTime = currentTime;
                lastReceivedBytes = receivedBytes;
                lastSentBytes = sentBytes;

                Debug.WriteLine("Statistics: BytesPerSecondReceived = " + BytesPerSecondReceived);
                Debug.WriteLine("Statistics: BytesPerSecondSent     = " + BytesPerSecondSent);
                
                foreach (NetworkGamer gamer in AllGamers)
                {
                    Debug.WriteLine("Gamer: " + gamer.DisplayName + "(" + gamer.Id + ")");
                }
            }
        }

        internal void End(NetworkSessionEndReason reason)
        {
            if (IsDisposed)
            {
                return;
            }

            RemoveGamersLocally();

            peer.Shutdown("Peer done");

            IsDisposed = true;
            Session = null;

            InvokeSessionEnded(new NetworkSessionEndedEventArgs(reason));
        }

        public void Dispose()
        {
            End(NetworkSessionEndReason.ClientSignedOut);
        }
    }
}