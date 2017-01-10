using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Messages;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net
{
    public sealed partial class NetworkSession : ISessionBackendListener, IDisposable, IMessageQueue
    {
        private const int MinSupportedLocalGamers = 1;
        private const int MaxSupportedLocalGamers = 4;
        private const int MinSupportedGamers = 2;
        public const int MaxSupportedGamers = 64; // Should be public according to docs
        public const int MaxPreviousGamers = 10; // Should be public according to docs

        private NetworkMachine localMachine;
        private NetworkMachine hostMachine;
        private NetworkSessionPublicInfo publicInfo;
        private int uniqueIdCount;

        private bool gameStartRequestThisFrame;
        private bool gameEndRequestThisFrame;
        private bool resetReadyRequestThisFrame;

        private List<OutgoingMessage> messageQueue;
        private List<EventArgs> eventQueue;
        private List<NetworkGamer> allGamers;
        private List<NetworkGamer> remoteGamers;
        private List<NetworkGamer> previousGamers;

        internal bool allowHostMigration;
        internal bool allowJoinInProgress;
        internal int maxGamers;
        internal int privateGamerSlots;

        internal IList<SignedInGamer> pendingSignedInGamers; // Must be a list since order is important
        internal ICollection<PeerEndPoint> allowlist;
        internal ICollection<PeerEndPoint> pendingEndPoints;
        internal IDictionary<NetworkMachine, ICollection<NetworkMachine>> hostPendingConnections;
        internal IDictionary<NetworkMachine, ICollection<PeerEndPoint>> hostPendingAllowlistInsertions;

        internal NetworkSession(SessionBackend backend, IEnumerable<PeerEndPoint> initialAllowlist, bool isHost, int maxGamers, int privateGamerSlots, NetworkSessionType type, NetworkSessionProperties properties, IEnumerable<SignedInGamer> signedInGamers)
        {
            this.localMachine = new NetworkMachine(this, backend.LocalPeer, true, isHost);
            this.hostMachine = this.localMachine.IsHost ? this.localMachine : null;
            this.publicInfo = new NetworkSessionPublicInfo();
            this.uniqueIdCount = 0;

            this.gameStartRequestThisFrame = false;
            this.gameEndRequestThisFrame = false;
            this.resetReadyRequestThisFrame = false;

            this.messageQueue = new List<OutgoingMessage>();
            this.eventQueue = new List<EventArgs>();
            this.allGamers = new List<NetworkGamer>();
            this.remoteGamers = new List<NetworkGamer>();
            this.previousGamers = new List<NetworkGamer>();

            this.allowHostMigration = false;
            this.allowJoinInProgress = false;
            this.maxGamers = maxGamers;
            this.privateGamerSlots = privateGamerSlots;

            this.pendingSignedInGamers = new List<SignedInGamer>();
            foreach (SignedInGamer gamer in signedInGamers)
            {
                if (!this.pendingSignedInGamers.Contains(gamer))
                {
                    this.pendingSignedInGamers.Add(gamer);
                }
            }

            this.allowlist = new HashSet<PeerEndPoint>();
            foreach (PeerEndPoint endPoint in initialAllowlist)
            {
                this.allowlist.Add(endPoint);
            }

            this.pendingEndPoints = null;

            if (this.localMachine.IsHost)
            {
                // Initialize empty pending end point list so that the host is approved automatically
                this.pendingEndPoints = new HashSet<PeerEndPoint>();

                this.hostPendingConnections = new Dictionary<NetworkMachine, ICollection<NetworkMachine>>();
                this.hostPendingAllowlistInsertions = new Dictionary<NetworkMachine, ICollection<PeerEndPoint>>();
            }

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
            this.SessionProperties = properties;
            this.SessionProperties.Session = this;
            this.SessionState = NetworkSessionState.Lobby;
            this.SessionType = type;
            this.SimulatedLatency = TimeSpan.Zero;
            this.SimulatedPacketLoss = 0.0f;

            SignedInGamer.SignedOut += LocalGamerSignedOut;
        }
        
        internal SessionBackend Backend { get; }
        internal PacketPool PacketPool { get; }
        internal InternalMessages InternalMessages { get; }
        internal IList<NetworkMachine> RemoteMachines { get; }

        internal bool IsFullyConnected { get { return localMachine.IsFullyConnected; } }

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

                if (value == true)
                {
                    throw new NotImplementedException("Host migration is not yet implemented");
                }

                /*if (allowHostMigration != value)
                {
                    allowHostMigration = value;

                    InternalMessages.SessionStateChanged.Create(null);
                }*/
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

        // BackendListener
        internal string HostGamertag
        {
            get
            {
                return Host != null || Host.Gamertag == string.Empty ? Host.Gamertag : "Game starting...";
            }
        }

        internal int CurrentGamerCount { get { return allGamers.Count; } }

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

        internal int OpenPublicGamerSlots { get { return MaxGamers - PrivateGamerSlots - allGamers.Count; } }

        bool ISessionBackendListener.IsDiscoverableLocally
        {
            get { return IsHost && localMachine.IsFullyConnected && SessionType == NetworkSessionType.SystemLink; }
        }

        bool ISessionBackendListener.IsDiscoverableOnline
        {
            get { return IsHost && localMachine.IsFullyConnected && (SessionType == NetworkSessionType.PlayerMatch || SessionType == NetworkSessionType.Ranked); }
        }

        NetworkSessionPublicInfo ISessionBackendListener.SessionPublicInfo
        {
            get
            {
                publicInfo.Set(SessionType, SessionProperties, HostGamertag, MaxGamers, PrivateGamerSlots, CurrentGamerCount, OpenPrivateGamerSlots, OpenPublicGamerSlots);

                return publicInfo;
            }
        }

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
            eventQueue.Add(args);
        }

        internal void InvokeGamerLeftEvent(GamerLeftEventArgs args)
        {
            eventQueue.Add(args);
        }

        internal void InvokeGameStartedEvent(GameStartedEventArgs args)
        {
            eventQueue.Add(args);
        }

        internal void InvokeGameEndedEvent(GameEndedEventArgs args)
        {
            eventQueue.Add(args);
        }

        internal void InvokeSessionEnded(NetworkSessionEndedEventArgs args)
        {
            eventQueue.Add(args);
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
            
            if (gameStartRequestThisFrame)
            {
                return;
            }

            InternalMessages.GameStarted.Create(null);
            gameStartRequestThisFrame = true;
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

            if (gameEndRequestThisFrame)
            {
                return;
            }

            InternalMessages.GameEnded.Create(null);
            gameEndRequestThisFrame = true;
        }

        public void ResetReady()
        {
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
            {
                throw new ObjectDisposedException("NetworkSession");
            }
            if (!IsHost)
            {
                throw new InvalidOperationException("Only the host can perform this action");
            }

            if (resetReadyRequestThisFrame)
            {
                return;
            }

            InternalMessages.ResetReady.Create();
            resetReadyRequestThisFrame = true;
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

        void IMessageQueue.Place(OutgoingMessage msg)
        {
            messageQueue.Add(msg);
        }

        bool ISessionBackendListener.AllowConnectionFromClient(PeerEndPoint endPoint)
        {
            if (IsHost)
            {
                return (allowJoinInProgress || SessionState == NetworkSessionState.Lobby) && OpenPublicGamerSlots > 0;
            }

            if (allowlist.Contains(endPoint))
            {
                Debug.WriteLine("Connection from client in allowlist, allowing...");
                allowlist.Remove(endPoint);
                return true;
            }
            else
            {
                Debug.WriteLine("Connection from client not in allowlist, denying...");
                return false;
            }
        }

        void ISessionBackendListener.IntroducedAsClient(PeerEndPoint targetEndPoint)
        {
            if (IsHost || IsFullyConnected)
            {
                return;
            }

            if (allowlist.Contains(targetEndPoint))
            {
                Debug.WriteLine("Introduced to target in allowlist, connecting...");
                allowlist.Remove(targetEndPoint);
                Backend.Connect(targetEndPoint);
            }
            else
            {
                Debug.WriteLine("Introduced to target not in allowlist, doing nothing.");
            }
        }

        void ISessionBackendListener.PeerConnected(Peer peer)
        {
            // The first connection is always the (initial) host
            bool senderIsHost = !IsHost && hostMachine == null;

            // Create a pending network machine
            NetworkMachine newMachine = new NetworkMachine(this, peer, false, senderIsHost);
            AddMachine(newMachine);

            if (senderIsHost)
            {
                hostMachine = newMachine;
            }

            if (localMachine.IsFullyConnected)
            {
                InternalMessages.FullyConnected.Create(newMachine);
            }

            InternalMessages.ConnectionAcknowledged.Create(newMachine);

            if (IsHost)
            {
                // Save snapshot of current connections and send them to the new peer
                ISet<NetworkMachine> requestedConnections = new HashSet<NetworkMachine>(RemoteMachines);
                requestedConnections.Remove(newMachine);

                hostPendingConnections.Add(newMachine, requestedConnections);
                
                InternalMessages.ConnectToAllRequest.Create(requestedConnections, newMachine);

                // Introduce machines to each other
                hostPendingAllowlistInsertions.Add(newMachine, new HashSet<PeerEndPoint>());

                foreach (NetworkMachine existingMachine in requestedConnections)
                {
                    hostPendingAllowlistInsertions[newMachine].Add(existingMachine.peer.EndPoint);
                    InternalMessages.AllowEndPointRequest.Create(existingMachine.peer.EndPoint, newMachine);

                    if (!hostPendingAllowlistInsertions.ContainsKey(existingMachine))
                    {
                        hostPendingAllowlistInsertions.Add(existingMachine, new HashSet<PeerEndPoint>());
                    }

                    hostPendingAllowlistInsertions[existingMachine].Add(newMachine.peer.EndPoint);
                    InternalMessages.AllowEndPointRequest.Create(newMachine.peer.EndPoint, existingMachine);
                }
            }
        }

        void ISessionBackendListener.PeerDisconnected(Peer peer)
        {
            NetworkMachine disconnectedMachine = peer.Tag as NetworkMachine;

            RemoveMachine(disconnectedMachine);

            if (IsHost)
            {
                // Update pending peers
                foreach (NetworkMachine pendingMachine in RemoteMachines)
                {
                    if (pendingMachine.IsFullyConnected)
                    {
                        continue;
                    }

                    if (hostPendingConnections[pendingMachine].Contains(disconnectedMachine))
                    {
                        hostPendingConnections[pendingMachine].Remove(disconnectedMachine);

                        InternalMessages.ConnectToAllRequest.Create(hostPendingConnections[pendingMachine], pendingMachine);
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

        void ISessionBackendListener.ReceiveMessage(IncomingMessage data, Peer sender)
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

        private void HandleInitialConnection()
        {
            if (localMachine.IsFullyConnected || pendingEndPoints == null)
            {
                return;
            }

            foreach (PeerEndPoint endPoint in pendingEndPoints)
            {
                if (!Backend.IsConnectedToEndPoint(endPoint) || !(Backend.FindRemotePeerByEndPoint(endPoint).Tag as NetworkMachine).HasAcknowledgedLocalMachine)
                {
                    return;
                }
            }

            pendingEndPoints = null;

            InternalMessages.FullyConnected.Create(null);

            foreach (SignedInGamer pendingGamer in pendingSignedInGamers)
            {
                InternalMessages.GamerIdRequest.Create(HostMachine);
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

            gameStartRequestThisFrame = false;
            gameEndRequestThisFrame = false;
            resetReadyRequestThisFrame = false;
        }

        internal void SilentUpdate()
        {
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
            {
                return;
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
        }

        private void TriggerEvents()
        {
            // This is not an elegant solution but it is convenient. Performance is not a problem since events are rare!
            foreach (EventArgs arg in eventQueue)
            {
                if (arg is GamerJoinedEventArgs)
                {
                    GamerJoined?.Invoke(this, arg as GamerJoinedEventArgs);
                }
                else if (arg is GamerLeftEventArgs)
                {
                    GamerLeft?.Invoke(this, arg as GamerLeftEventArgs);
                }
                else if (arg is GameStartedEventArgs)
                {
                    GameStarted?.Invoke(this, arg as GameStartedEventArgs);
                }
                else if (arg is GameEndedEventArgs)
                {
                    GameEnded?.Invoke(this, arg as GameEndedEventArgs);
                }
                else if (arg is NetworkSessionEndedEventArgs)
                {
                    SessionEnded?.Invoke(this, arg as NetworkSessionEndedEventArgs);
                }
            }

            eventQueue.Clear();
        }

        public void Update()
        {
            if (IsDisposed || SessionState == NetworkSessionState.Ended)
            {
                throw new ObjectDisposedException("NetworkSession");
            }
            if (!localMachine.IsFullyConnected)
            {
                throw new NetworkException("NetworkSession not initialized properly. The ISessionCreator must call NetworkSession.SilentUpdate() until NetworkSession.IsFullyConnected is true before returning the NetworkSession.");
            }

            SilentUpdate();

            TriggerEvents();
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
