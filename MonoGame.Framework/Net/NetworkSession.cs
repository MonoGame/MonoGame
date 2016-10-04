using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Messages;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net
{
    internal delegate NetworkSession AsyncCreate(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties);

    public sealed class NetworkSession : IDisposable, IBackendListener, IMessageQueue
    {
        internal const int Port = 14242;
        internal const int DiscoveryTime = 1000;
        internal const int JoinTime = 1000;

        public const int MaxPreviousGamers = 10;
        public const int MaxSupportedGamers = 64;
        private const int MinSupportedGamers = 2;

        internal static NetworkSession Session = null;

        private static AsyncCreate asyncCreateCaller;

        // Create (async)
        public static IAsyncResult BeginCreate(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, AsyncCallback callback, Object asyncState)
        {
            if (sessionType == NetworkSessionType.PlayerMatch || sessionType == NetworkSessionType.Ranked)
            {
                throw new NotImplementedException("PlayerMatch and Ranked are not implemented yet");
            }
            if (Session != null || asyncCreateCaller != null)
            {
                throw new InvalidOperationException("Only one NetworkSession allowed");
            }
            if (maxGamers < MinSupportedGamers || maxGamers > MaxSupportedGamers)
            {
                throw new ArgumentOutOfRangeException("maxGamers must be in the range [" + MinSupportedGamers + ", " + MaxSupportedGamers + "]");
            }
            if (privateGamerSlots < 0 || privateGamerSlots > maxGamers)
            {
                throw new ArgumentOutOfRangeException("privateGamerSlots must be in the range [0, maxGamers]");
            }

            asyncCreateCaller = new AsyncCreate(NetworkSessionCreation.Create);

            try
            {
                return asyncCreateCaller.BeginInvoke(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties, callback, asyncState);
            }
            catch { throw; }
        }

        public static NetworkSession EndCreate(IAsyncResult result)
        {
            try
            {
                NetworkSession session = asyncCreateCaller.EndInvoke(result);
                asyncCreateCaller = null;
                return session;
            }
            catch { throw; }
        }

        public static IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, AsyncCallback callback, Object asyncState)
        {
            throw new NotImplementedException();
        }
        public static IAsyncResult BeginCreate(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties, AsyncCallback callback, Object asyncState)
        {
            throw new NotImplementedException();
        }

        // Create (sync)
        public static NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            try
            {
                return EndCreate(BeginCreate(sessionType, localGamers, maxGamers, privateGamerSlots, sessionProperties, null, null));
            }
            catch { throw; }
        }
        public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers)
        {
            try
            {
                return EndCreate(BeginCreate(sessionType, maxLocalGamers, maxGamers, null, null));
            }
            catch { throw; }
        }
        public static NetworkSession Create(NetworkSessionType sessionType, int maxLocalGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            try
            {
                return EndCreate(BeginCreate(sessionType, maxLocalGamers, maxGamers, privateGamerSlots, sessionProperties, null, null));
            }
            catch { throw; }
        }

        // Find (sync)
        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            return NetworkSessionCreation.Find(sessionType, localGamers, searchProperties);
        }
        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, int maxLocalGamers, NetworkSessionProperties searchProperties)
        {
            throw new NotImplementedException();
        }

        // Join (sync)
        public static NetworkSession Join(AvailableNetworkSession availableSession)
        {
            return NetworkSessionCreation.Join(availableSession);
        }

        private IPEndPoint hostEndPoint;
        private NetworkMachine localMachine;
        private NetworkMachine hostMachine;
        private List<IOutgoingMessage> messageQueue;

        internal IList<SignedInGamer> pendingSignedInGamers;
        internal ICollection<IPEndPoint> pendingEndPoints;

        // Host stores which remote machines existed before a particular machine connected
        internal Dictionary<NetworkMachine, ICollection<NetworkMachine>> pendingPeerConnections = new Dictionary<NetworkMachine, ICollection<NetworkMachine>>();

        private int uniqueIdCount;
        private List<NetworkGamer> allGamers;
        private List<NetworkGamer> remoteGamers;
        private List<NetworkGamer> previousGamers;

        internal bool allowHostMigration;
        internal bool allowJoinInProgress;
        internal int maxGamers;
        internal int privateGamerSlots;

        internal NetworkSession(IBackend backend, IPEndPoint hostEndPoint, int maxGamers, int privateGamerSlots, NetworkSessionType type, NetworkSessionProperties properties, IEnumerable<SignedInGamer> signedInGamers)
        {
            this.hostEndPoint = hostEndPoint;
            this.localMachine = new NetworkMachine(this, backend.LocalPeer, true, hostEndPoint == null);
            this.hostMachine = hostEndPoint == null ? this.localMachine : null;
            this.messageQueue = new List<IOutgoingMessage>();

            this.pendingSignedInGamers = new List<SignedInGamer>();

            foreach (SignedInGamer gamer in signedInGamers)
            {
                if (!this.pendingSignedInGamers.Contains(gamer))
                {
                    this.pendingSignedInGamers.Add(gamer);
                }
            }

            this.pendingEndPoints = null;

            if (hostEndPoint == null)
            {
                // Initialize empty pending end point list so that the host is approved automatically
                this.pendingEndPoints = new List<IPEndPoint>();
            }

            this.allGamers = new List<NetworkGamer>();
            this.remoteGamers = new List<NetworkGamer>();
            this.previousGamers = new List<NetworkGamer>();

            this.allowHostMigration = false;
            this.allowJoinInProgress = false;
            this.maxGamers = maxGamers;
            this.privateGamerSlots = privateGamerSlots;

            this.Backend = backend;
            this.Backend.Listener = this;
            this.PacketPool = new PacketPool();
            this.InternalMessages = new InternalMessages(this.Backend, this, this.localMachine);
            this.RemoteMachines = new List<NetworkMachine>();

            this.AllGamers = new GamerCollection<NetworkGamer>(this.allGamers);
            this.BytesPerSecondReceived = 0;
            this.BytesPerSecondSent = 0;
            this.IsDisposed = false;
            this.LocalGamers = this.localMachine.LocalGamers;
            this.PreviousGamers = new GamerCollection<NetworkGamer>(this.previousGamers);
            this.RemoteGamers = new GamerCollection<NetworkGamer>(this.remoteGamers);
            this.SessionProperties = properties != null ? properties : new NetworkSessionProperties();
            this.SessionProperties.Session = this;
            this.SessionState = NetworkSessionState.Lobby;
            this.SessionType = type;
            this.SimulatedLatency = TimeSpan.Zero;
            this.SimulatedPacketLoss = 0.0f;

            SignedInGamer.SignedOut += LocalGamerSignedOut;
        }
        
        internal IBackend Backend { get; }
        internal PacketPool PacketPool { get; }
        internal InternalMessages InternalMessages { get; }
        internal IList<NetworkMachine> RemoteMachines { get; }

        public GamerCollection<NetworkGamer> AllGamers { get; }

        public bool AllowHostMigration
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                return allowHostMigration;
            }

            set
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }
                if (!IsHost)
                {
                    throw new InvalidOperationException("Only the host can perform this action");
                }

                if (allowHostMigration != value)
                {
                    allowHostMigration = value;

                    InternalMessages.SessionStateChanged.Create(null);
                }
            }
        }

        public bool AllowJoinInProgress
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                return allowJoinInProgress;
            }

            set
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }
                if (!IsHost)
                {
                    throw new InvalidOperationException("Only the host can perform this action");
                }

                if (allowJoinInProgress != value)
                {
                    allowJoinInProgress = value;

                    InternalMessages.SessionStateChanged.Create(null);
                }
            }
        }

        public int BytesPerSecondReceived { get; private set; }
        public int BytesPerSecondSent { get; private set; }

        internal NetworkMachine HostMachine
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                if (hostMachine == null)
                {
                    throw new NetworkException("Host machine is null at this time");
                }

                return hostMachine;
            }
        }

        public NetworkGamer Host
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                NetworkMachine hostMachine = HostMachine;

                if (hostMachine.Gamers.Count == 0)
                {
                    throw new NetworkException("NetworkSession not ready for use yet. Bug in internal session creation, gamer leaving or host migration code.");
                }

                return hostMachine.Gamers[0];
            }
        }

        public bool IsDisposed { get; private set; }

        public bool IsEveryoneReady
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

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

        public bool IsHost
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                return localMachine.IsHost;
            }
        }

        public GamerCollection<LocalNetworkGamer> LocalGamers { get; }

        public int MaxGamers
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                return maxGamers;
            }

            set
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }
                if (!IsHost)
                {
                    throw new InvalidOperationException("Only the host can perform this action");
                }
                if (value < MinSupportedGamers || value > MaxSupportedGamers)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (maxGamers != value)
                {
                    maxGamers = value;

                    InternalMessages.SessionStateChanged.Create(null);
                }
            }
        }
        
        public GamerCollection<NetworkGamer> PreviousGamers { get; }

        internal int MaxPossiblePrivateGamerSlots
        {
            get
            {
                int usedPublicSlots = 0;

                foreach (NetworkGamer gamer in AllGamers)
                {
                    if (!gamer.IsPrivateSlot)
                    {
                        usedPublicSlots++;
                    }
                }

                return maxGamers - usedPublicSlots;
            }
        }

        public int PrivateGamerSlots
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                return privateGamerSlots;
            }

            set
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }
                if (!IsHost)
                {
                    throw new InvalidOperationException("Only the host can perform this action");
                }
                if (value < 0 || value > MaxPossiblePrivateGamerSlots)
                {
                    throw new ArgumentOutOfRangeException();
                }

                if (privateGamerSlots != value)
                {
                    privateGamerSlots = value;

                    InternalMessages.SessionStateChanged.Create(null);
                }
            }
        }

        public GamerCollection<NetworkGamer> RemoteGamers { get; }
        public NetworkSessionProperties SessionProperties { get; }
        public NetworkSessionState SessionState { get; internal set; }
        public NetworkSessionType SessionType { get; }

        public TimeSpan SimulatedLatency // TODO: Should be applied even to local messages
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                return Backend.SimulatedLatency;
            }
            set
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                Backend.SimulatedLatency = value;
            }
        }

        public float SimulatedPacketLoss // TODO: Should be applied even to local messages
        {
            get
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                return Backend.SimulatedPacketLoss;
            }
            set
            {
                if (IsDisposed || SessionState == NetworkSessionState.Ended)
                {
                    throw new ObjectDisposedException("NetworkSession");
                }

                Backend.SimulatedPacketLoss = value;
            }
        }

        // For discovery response
        bool IBackendListener.ShouldSendDiscoveryResponse { get { return IsHost; } }
        int IBackendListener.CurrentGamerCount { get { return allGamers.Count; } }
        string IBackendListener.HostGamertag
        {
            get
            {
                return Host != null || Host.Gamertag == string.Empty ? Host.Gamertag : "Game starting...";
            }
        }
        int IBackendListener.OpenPrivateGamerSlots
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
        int IBackendListener.OpenPublicGamerSlots { get { return MaxGamers - PrivateGamerSlots - allGamers.Count; } }

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
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
            {
                throw new ObjectDisposedException("NetworkSession");
            }

            if (localMachine.FindLocalGamerBySignedInGamer(signedInGamer) != null)
            {
                return;
            }

            if (!pendingSignedInGamers.Contains(signedInGamer))
            {
                pendingSignedInGamers.Add(signedInGamer);

                InternalMessages.GamerIdRequest.Create(HostMachine);
            }
        }

        public void StartGame()
        {
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
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

            SessionState = NetworkSessionState.Playing;
            InternalMessages.GameStarted.Create(null);
        }

        public void EndGame()
        {
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
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

            SessionState = NetworkSessionState.Lobby;
            InternalMessages.GameEnded.Create(null);
        }

        public void ResetReady() // only host
        {
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
            {
                throw new ObjectDisposedException("NetworkSession");
            }

            throw new NotImplementedException();
        }

        public NetworkGamer FindGamerById(byte gamerId)
        {
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
            {
                throw new ObjectDisposedException("NetworkSession");
            }

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
            machine.HasLeftSession = true;

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

            LocalNetworkGamer localGamer = localMachine.FindLocalGamerBySignedInGamer(args.Gamer);

            if (localGamer != null)
            {
                InternalMessages.GamerLeft.Create(localGamer, null);
            }
        }

        internal bool GetNewUniqueId(out byte id)
        {
            // Cycle through all 0-255 values before re-using an old id
            for (int i = 0; i < 256; i++)
            {
                byte candidateId = (byte)uniqueIdCount;

                uniqueIdCount++;
                if (uniqueIdCount > 255)
                {
                    uniqueIdCount = 0;
                }

                if (FindGamerById(candidateId) == null)
                {
                    id = candidateId;
                    return true;
                }
            }

            id = 255;
            return false;
        }

        private string MachineOwnerName(NetworkMachine machine)
        {
            if (machine == null)
            {
                return "everyone";
            }
            else if (machine.IsLocal)
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

        void IMessageQueue.Place(IOutgoingMessage msg)
        {
            messageQueue.Add(msg);
        }

        void IBackendListener.PeerConnected(IPeer peer)
        {
            bool senderIsHost = peer.EndPoint == hostEndPoint;

            // Create a pending network machine
            NetworkMachine senderMachine = new NetworkMachine(this, peer, false, senderIsHost);
            AddMachine(senderMachine);

            if (senderIsHost)
            {
                hostMachine = senderMachine;
            }

            if (localMachine.IsFullyConnected)
            {
                InternalMessages.FullyConnected.Create(senderMachine);
            }

            InternalMessages.ConnectionAcknowledged.Create(senderMachine);

            if (IsHost)
            {
                // Save snapshot of current connections and send them to the new peer
                ICollection<NetworkMachine> requestedConnections = new HashSet<NetworkMachine>(RemoteMachines);
                requestedConnections.Remove(senderMachine);
                pendingPeerConnections.Add(senderMachine, requestedConnections);

                InternalMessages.ConnectToAllRequest.Create(requestedConnections, senderMachine);
            }
        }

        void IBackendListener.PeerDisconnected(IPeer peer)
        {
            NetworkMachine disconnectedMachine = peer.Tag as NetworkMachine;

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

                        InternalMessages.ConnectToAllRequest.Create(pendingPair.Value, pendingMachine);
                    }
                }
            }
            else
            {
                if (disconnectedMachine == HostMachine)
                {
                    // TODO: Host migration
                    End(NetworkSessionEndReason.HostEndedSession);
                }
            }
        }

        void IBackendListener.ReceiveMessage(IIncomingMessage data, IPeer sender)
        {
            NetworkMachine senderMachine = sender.Tag as NetworkMachine;

            byte messageType = data.ReadByte();

            if ((InternalMessageIndex)messageType != InternalMessageIndex.UserMessage)
            {
                Debug.WriteLine("Receiving " + (InternalMessageIndex)messageType + " from " + MachineOwnerName(senderMachine) + "...");
            }

            InternalMessage receiver = InternalMessages.FromIndex[messageType];
            receiver.Receive(data, senderMachine);
        }

        public void Update()
        {
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
            {
                throw new ObjectDisposedException("NetworkSession");
            }

            // Recycle inbound packets that the user has read from the last frame
            foreach (LocalNetworkGamer localGamer in localMachine.LocalGamers)
            {
                localGamer.RecycleInboundPackets();
            }

            // Handle incoming internal messages (Might add new inbound packets)
            Backend.Update();

            // Add delayed inbound packets if sender has joined (Might add new inbound packets)
            foreach (LocalNetworkGamer localGamer in localMachine.LocalGamers)
            {
                localGamer.TryAddDelayedInboundPackets();
            }

            HandleInitialConnection();

            // Queue outbound packets as internal messages
            foreach (LocalNetworkGamer localGamer in localMachine.LocalGamers)
            {
                localGamer.QueueOutboundPackets();
            }

            SendInternalMessages();

            foreach (LocalNetworkGamer localGamer in LocalGamers)
            {
                localGamer.RecycleOutboundPackets();
            }

            if (SessionState == NetworkSessionState.Ended)
            {
                return;
            }

            Backend.UpdateStatistics();
        }

        private void HandleInitialConnection()
        {
            if (localMachine.IsFullyConnected || pendingEndPoints == null)
            {
                return;
            }

            bool done = true;

            foreach (IPEndPoint endPoint in pendingEndPoints)
            {
                if (!(Backend.IsConnectedToEndPoint(endPoint) && (Backend.FindRemotePeerByEndPoint(endPoint).Tag as NetworkMachine).HasAcknowledgedLocalMachine))
                {
                    done = false;
                }
            }

            if (done)
            {
                InternalMessages.FullyConnected.Create(null);

                foreach (SignedInGamer pendingGamer in pendingSignedInGamers)
                {
                    InternalMessages.GamerIdRequest.Create(HostMachine);
                }
            }
        }

        private void SendInternalMessages()
        {
            for (int i = 0; i < messageQueue.Count; i++)
            {
                Backend.SendMessage(messageQueue[i]);

                if (SessionState == NetworkSessionState.Ended)
                {
                    break;
                }
            }

            messageQueue.Clear();
        }
        

        internal void End(NetworkSessionEndReason reason)
        {
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
            {
                return;
            }

            RemoveAllMachines();

            Backend.Shutdown("Done");

            SessionState = NetworkSessionState.Ended;
            Session = null;

            InvokeSessionEnded(new NetworkSessionEndedEventArgs(reason));
        }

        public void Dispose()
        {
            End(NetworkSessionEndReason.ClientSignedOut);

            IsDisposed = true;
        }
    }
}
