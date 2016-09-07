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
        internal const int Port = 14242;
        internal const int DiscoveryTime = 1000;
        internal const int JoinTime = 1000;

        public const int MaxPreviousGamers = 10;
        public const int MaxSupportedGamers = 64;

        internal static NetworkSession Session = null;

        // Create
        public static NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            return NetworkSessionCreation.Create(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties);
        }
        public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers)
        {
            return NetworkSessionCreation.Create(sessionType, maxLocalGamers, maxGamers);
        }
        public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            return NetworkSessionCreation.Create(sessionType, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties);
        }
        // Find
        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            return NetworkSessionCreation.Find(sessionType, localGamers, searchProperties);
        }
        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties)
        {
            return NetworkSessionCreation.Find(sessionType, maxLocalGamers, searchProperties);
        }
        // Join
        public static NetworkSession Join(AvailableNetworkSession availableSession)
        {
            return NetworkSessionCreation.Join(availableSession);
        }

        internal PacketPool packetPool;

        internal NetPeer peer;
        private NetworkMachine localMachine;
        private NetConnection hostConnection;

        private IList<SignedInGamer> pendingSignedInGamers;
        internal IList<SignedInGamer> joiningSignedInGamers;
        internal IList<SignedInGamer> leavingSignedInGamers;
        internal ICollection<IPEndPoint> pendingEndPoints;

        // Host stores which remote machines existed before a particular machine connected
        internal Dictionary<NetworkMachine, ICollection<NetworkMachine>> pendingPeerConnections = new Dictionary<NetworkMachine, ICollection<NetworkMachine>>();

        private byte uniqueIdCount;
        private List<NetworkGamer> allGamers;
        private List<NetworkGamer> remoteGamers;
        private List<NetworkGamer> previousGamers;

        private NetBuffer internalBuffer;
        private DateTime lastTime;
        private int lastReceivedBytes;
        private int lastSentBytes;

        internal NetworkSession(NetPeer peer, NetConnection hostConnection, int maxGamers, int privateGamerSlots, NetworkSessionType type, NetworkSessionProperties properties, IEnumerable<SignedInGamer> signedInGamers)
        {
            this.peer = peer;
            this.localMachine = new NetworkMachine(this, null, hostConnection == null);
            this.hostConnection = hostConnection;

            this.pendingSignedInGamers = new List<SignedInGamer>(signedInGamers);
            this.joiningSignedInGamers = new List<SignedInGamer>();
            this.leavingSignedInGamers = new List<SignedInGamer>();

            if (hostConnection == null)
            {
                // Initialize empty pending end point list so that the host is approved automatically
                this.pendingEndPoints = new List<IPEndPoint>();
            }

            this.allGamers = new List<NetworkGamer>();
            this.remoteGamers = new List<NetworkGamer>();
            this.previousGamers = new List<NetworkGamer>();

            this.RemoteMachines = new List<NetworkMachine>();
            this.AllGamers = new GamerCollection<NetworkGamer>(this.allGamers);
            this.AllowHostMigration = false;
            this.AllowJoinInProgress = false;
            this.BytesPerSecondReceived = 0;
            this.BytesPerSecondSent = 0;
            this.IsDisposed = false;
            this.LocalGamers = this.localMachine.LocalGamers;
            this.MaxGamers = maxGamers;
            this.PreviousGamers = new GamerCollection<NetworkGamer>(this.previousGamers);
            this.PrivateGamerSlots = privateGamerSlots;
            this.RemoteGamers = new GamerCollection<NetworkGamer>(this.remoteGamers);
            this.SessionProperties = properties != null ? properties : new NetworkSessionProperties();
            this.SessionState = NetworkSessionState.Lobby;
            this.SessionType = type;
            this.SimulatedLatency = TimeSpan.Zero;
            this.SimulatedPacketLoss = 0.0f;

            this.packetPool = new PacketPool();
            this.internalBuffer = new NetBuffer();
            this.lastTime = DateTime.Now;
            this.lastReceivedBytes = this.peer.Statistics.ReceivedBytes;
            this.lastSentBytes = this.peer.Statistics.SentBytes;

            SignedInGamer.SignedOut += LocalGamerSignedOut;
        }

        internal IList<NetworkMachine> RemoteMachines { get; }
        public GamerCollection<NetworkGamer> AllGamers { get; }
        public bool AllowHostMigration { get; set; } // any peer can get, only host can set
        public bool AllowJoinInProgress { get; set; } // any peer can get, only host can set
        public int BytesPerSecondReceived { get; internal set; }
        public int BytesPerSecondSent { get; internal set; }

        internal NetworkMachine HostMachine
        {
            get { return IsHost ? localMachine : hostConnection.Tag as NetworkMachine; }
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

        public bool IsHost { get { return localMachine.IsHost; } }
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

        // For discovery response
        internal int CurrentGamerCount { get { return allGamers.Count; } }
        internal string HostGamertag
        {
            get
            {
                return Host != null || Host.Gamertag == string.Empty ? Host.Gamertag : "Game starting...";
            }
        }
        internal int OpenPrivateGamerSlots
        {
            get
            {
                int usedSlots = 0;

                foreach (NetworkGamer gamer in AllGamers)
                {
                    if (gamer.IsPrivateSlot)
                    {
                        usedSlots++;
                    }
                }

                return PrivateGamerSlots - usedSlots;
            }
        }
        internal int OpenPublicGamerSlots { get { return MaxGamers - PrivateGamerSlots - CurrentGamerCount; } }

        // Events
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
            if (localMachine.FindLocalGamerBySignedInGamer(signedInGamer) != null)
            {
                return;
            }

            pendingSignedInGamers.Add(signedInGamer);
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

            Send(new GameStartedSender());
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

            Send(new GameEndedSender());
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

        internal void AddMachine(NetworkMachine machine)
        {
            if (!machine.IsLocal)
            {
                RemoteMachines.Add(machine);
            }
        }

        internal void RemoveMachine(NetworkMachine machine)
        {
            // Remove gamers
            for (int i = machine.Gamers.Count - 1; i >= 0; i--)
            {
                RemoveGamer(machine.Gamers[i]);
            }

            // Remove machine
            if (!machine.IsLocal)
            {
                RemoteMachines.Remove(machine);
            }
        }

        internal void RemoveAllMachines()
        {
            RemoveMachine(localMachine);

            for (int i = RemoteMachines.Count - 1; i >= 0; i--)
            {
                RemoveMachine(RemoteMachines[i]);
            }
        }

        internal void AddGamer(NetworkGamer gamer)
        {
            gamer.Machine.AddGamer(gamer);
            allGamers.Add(gamer);
            allGamers.Sort(NetworkGamer.Comparer);
            if (!gamer.IsLocal)
            {
                remoteGamers.Add(gamer);
                remoteGamers.Sort(NetworkGamer.Comparer);
            }

            InvokeGamerJoinedEvent(new GamerJoinedEventArgs(gamer));
        }

        internal void RemoveGamer(NetworkGamer gamer)
        {
            gamer.HasLeftSession = true;

            gamer.Machine.RemoveGamer(gamer);
            allGamers.Remove(gamer);
            if (!gamer.IsLocal)
            {
                remoteGamers.Remove(gamer);
            }
            
            AddPreviousGamer(gamer);

            InvokeGamerLeftEvent(new GamerLeftEventArgs(gamer));
        }

        private void AddPreviousGamer(NetworkGamer gamer)
        {
            previousGamers.Add(gamer);

            if (previousGamers.Count > MaxPreviousGamers)
            {
                previousGamers.RemoveAt(0);
            }
        }

        private void LocalGamerSignedOut(object sender, SignedOutEventArgs args)
        {
            pendingSignedInGamers.Remove(args.Gamer);
            joiningSignedInGamers.Remove(args.Gamer);
            leavingSignedInGamers.Add(args.Gamer);
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

            message.Send(output, localMachine);
        }

        internal void Send(IInternalMessageSender message)
        {
            if (message.MessageType != InternalMessageType.UserMessage)
            {
                Debug.WriteLine("Sending " + message.MessageType + " to all peers...");
            }

            // Send to all remote machines
            foreach (NetworkMachine remoteMachine in RemoteMachines)
            {
                NetOutgoingMessage msg = peer.CreateMessage();
                EncodeMessage(message, msg);
                peer.SendMessage(msg, remoteMachine.connection, ToDeliveryMethod(message.Options), message.SequenceChannel);
            }

            // Send to self (Should be done last since then all Send() calls happen before any Receive() call)
            Send(message, localMachine);
        }

        internal void Send(IInternalMessageSender message, NetworkMachine recipient)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException("recipient");
            }

            if (message.MessageType != InternalMessageType.UserMessage)
            {
                Debug.WriteLine("Sending " + message.MessageType + " to " + MachineOwnerName(recipient) + "...");
            }

            if (recipient.IsLocal)
            {
                internalBuffer.LengthBits = 0;
                EncodeMessage(message, internalBuffer);

                internalBuffer.Position = 0;
                Receive(internalBuffer, localMachine);
            }
            else
            {
                NetOutgoingMessage msg = peer.CreateMessage();
                EncodeMessage(message, msg);
                peer.SendMessage(msg, recipient.connection, ToDeliveryMethod(message.Options), message.SequenceChannel);
            }
        }

        private void Receive(NetBuffer input, NetworkMachine senderMachine)
        {
            byte messageType = input.ReadByte();

            if ((InternalMessageType)messageType != InternalMessageType.UserMessage)
            {
                Debug.WriteLine("Receiving " + (InternalMessageType)messageType + " from " + MachineOwnerName(senderMachine) + "...");
            }

            Type receiverToInstantiate = InternalMessage.MessageToReceiverTypeMap[messageType];
            IInternalMessageReceiver receiver = (IInternalMessageReceiver)Activator.CreateInstance(receiverToInstantiate);
            receiver.Receive(input, localMachine, senderMachine);
        }

        public void Update()
        {
            // Recycle inbound packets from last frame
            foreach (LocalNetworkGamer localGamer in localMachine.LocalGamers)
            {
                localGamer.RecycleInboundPackets();
            }

            // Try to receive delayed inbound packets -> Might create new inbound packets
            foreach (LocalNetworkGamer localGamer in localMachine.LocalGamers)
            {
                localGamer.TryReceiveDelayedInboundPackets();
            }

            // Send accumulated outbound packets -> Might create new inbound packets
            foreach (LocalNetworkGamer localGamer in localMachine.LocalGamers)
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

            // Handle incoming messages -> Might create new inbound packets
            NetIncomingMessage msg;
            while ((msg = peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    // Discovery
                    case NetIncomingMessageType.DiscoveryRequest:
                        if (!IsHost)
                        {
                            throw new NetworkException("Discovery request received when not host");
                        }
                        Debug.WriteLine("Discovery request received");
                        NetOutgoingMessage response = peer.CreateMessage();
                        response.Write((byte)SessionType);
                        response.Write(MaxGamers);
                        response.Write(PrivateGamerSlots);
                        response.Write(CurrentGamerCount);
                        response.Write(HostGamertag);
                        response.Write(OpenPrivateGamerSlots);
                        response.Write(OpenPublicGamerSlots);
                        SessionProperties.Send(response);
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
                            AddMachine(senderMachine);

                            if (localMachine.IsFullyConnected)
                            {
                                Send(new FullyConnectedSender(), senderMachine);
                            }

                            Send(new ConnectionAcknowledgedSender(), senderMachine);

                            if (IsHost)
                            {
                                // Save snapshot of current connections and send them to the new peer
                                ICollection<NetworkMachine> requestedConnections = new HashSet<NetworkMachine>(RemoteMachines);
                                requestedConnections.Remove(senderMachine);
                                pendingPeerConnections.Add(senderMachine, requestedConnections);

                                Send(new ConnectToAllRequestSender(requestedConnections), senderMachine);
                            }
                        }

                        if (status == NetConnectionStatus.Disconnected)
                        {
                            // Remove gamers
                            NetworkMachine disconnectedMachine = msg.SenderConnection.Tag as NetworkMachine;
                            RemoveMachine(disconnectedMachine);

                            if (IsHost)
                            {
                                // Update pending peers
                                pendingPeerConnections.Remove(disconnectedMachine);

                                foreach (var pendingPair in pendingPeerConnections)
                                {
                                    NetworkMachine pendingMachine = pendingPair.Key;

                                    if (pendingPair.Value.Contains(disconnectedMachine))
                                    {
                                        pendingPair.Value.Remove(disconnectedMachine);

                                        Send(new ConnectToAllRequestSender(pendingPair.Value), pendingMachine);
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

            if (!localMachine.IsFullyConnected)
            {
                // Check if fully connected
                if (pendingEndPoints != null)
                {
                    bool done = true;

                    foreach (IPEndPoint endPoint in pendingEndPoints)
                    {
                        if (!(IsConnectedToEndPoint(endPoint) && (peer.GetConnection(endPoint).Tag as NetworkMachine).HasAcknowledgedLocalMachine))
                        {
                            done = false;
                        }
                    }

                    if (done)
                    {
                        Send(new FullyConnectedSender());
                    }
                }
            }

            if (localMachine.IsFullyConnected)
            {
                // Handle leaving signed in gamers
                if (leavingSignedInGamers.Count > 0)
                {
                    foreach (SignedInGamer leavingGamer in leavingSignedInGamers)
                    {
                        LocalNetworkGamer localGamer = localMachine.FindLocalGamerBySignedInGamer(leavingGamer);

                        if (localGamer != null)
                        {
                            RemoveGamer(localGamer);
                            Send(new GamerLeftSender(localGamer));
                        }
                    }

                    leavingSignedInGamers.Clear();
                }

                // Handle pending signed in gamers
                if (pendingSignedInGamers.Count > 0)
                {
                    foreach (SignedInGamer pendingGamer in pendingSignedInGamers)
                    {
                        joiningSignedInGamers.Add(pendingGamer);
                        Send(new GamerIdRequestSender(), HostMachine);
                    }

                    pendingSignedInGamers.Clear();
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

                //Debug.WriteLine("Statistics: BytesPerSecondReceived = " + BytesPerSecondReceived);
                //Debug.WriteLine("Statistics: BytesPerSecondSent     = " + BytesPerSecondSent);

                lastTime = currentTime;
                lastReceivedBytes = receivedBytes;
                lastSentBytes = sentBytes;
            }
        }

        internal void End(NetworkSessionEndReason reason)
        {
            if (IsDisposed)
            {
                return;
            }

            RemoveAllMachines();

            peer.Shutdown("Done");

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
