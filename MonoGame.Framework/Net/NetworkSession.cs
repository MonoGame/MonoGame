using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.GamerServices;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    public sealed partial class NetworkSession : IDisposable
    {
        private const string LoadingGamertag = "...";
        private const int MinSupportedLocalGamers = 1;
        private const int MaxSupportedLocalGamers = 4;
        private const int MinSupportedGamers = 1;
        public const int MaxSupportedGamers = 64; // Should be public according to docs
        public const int MaxPreviousGamers = 10; // Should be public according to docs

        internal readonly PacketPool packetPool = new PacketPool();

        private readonly NetPeer peer;
        private readonly bool isHost;
        private readonly byte machineId;
        private readonly NetworkSessionType type;
        private readonly Guid guid;

        private NetworkSessionProperties properties;
        private NetworkMachine localMachine;
        private NetworkMachine hostMachine;
        private NetConnection hostConnection;
        private List<NetworkMachine> allMachines = new List<NetworkMachine>();
        private Dictionary<byte, NetworkMachine> machineFromId = new Dictionary<byte, NetworkMachine>();
        private Dictionary<NetworkMachine, NetConnection> connectionFromMachine = new Dictionary<NetworkMachine, NetConnection>();

        private NetworkSessionPublicInfo publicInfo = new NetworkSessionPublicInfo();
        private NetworkSessionState state = NetworkSessionState.Lobby;

        private bool allowHostMigration = false;
        private bool allowJoinInProgress = false;
        private int maxGamers = MaxSupportedGamers;
        private int privateGamerSlots = 0;

        private bool gameStartRequestThisFrame = false;
        private bool gameEndRequestThisFrame = false;
        private bool resetReadyRequestThisFrame = false;

        private bool hasFailedMasterServerValidation = false;
        private bool isRegisteredAsHostWithMasterServer = false;
        private string masterServerGeneralInfo = null;
        private DateTime lastMasterServerReport = DateTime.MinValue;

        private List<EventArgs> eventQueue = new List<EventArgs>();
        private List<LocalNetworkGamer> localGamers = new List<LocalNetworkGamer>();
        private Dictionary<SignedInGamer, LocalNetworkGamer> localGamerFromSignedInGamer = new Dictionary<SignedInGamer, LocalNetworkGamer>();
        private List<NetworkGamer> allGamers = new List<NetworkGamer>();
        private List<NetworkGamer> remoteGamers = new List<NetworkGamer>();
        private List<NetworkGamer> previousGamers = new List<NetworkGamer>();
        private Dictionary<byte, NetworkGamer> gamerFromId = new Dictionary<byte, NetworkGamer>();

        private List<SignedInGamer> pendingSignedInGamers = new List<SignedInGamer>();

        internal NetworkSession(NetPeer peer, bool isHost, byte machineId, NetworkSessionType type, NetworkSessionProperties properties, int maxGamers, int privateGamerSlots, IEnumerable<SignedInGamer> localGamers, string hostDisplayName, string hostGamertag)
        {
            if (peer.Configuration.AutoFlushSendQueue) throw new InvalidOperationException("Peer must not flush send queue automatically");
            if (isHost && machineId != 0) throw new InvalidOperationException("Host must have machine id 0");
            if (!isHost && machineId == 0) throw new InvalidOperationException("Client cannot have machine id 0");

            this.peer = peer;
            this.isHost = isHost;
            this.machineId = machineId;
            this.type = type;
            this.guid = Guid.NewGuid();

            this.properties = properties;
            this.localMachine = new NetworkMachine(this, true, isHost, machineId);
            this.hostMachine = isHost ? this.localMachine : new NetworkMachine(this, false, true, 0);

            AddMachine(this.localMachine, null);

            if (!isHost)
            {
                if (peer.ConnectionsCount != 1 || peer.Connections[0].Status != NetConnectionStatus.Connected)
                {
                    throw new InvalidOperationException("Client peer must be connected to host before NetworkSession can be instantiated");
                }
                hostConnection = peer.Connections[0];
                hostConnection.Tag = this.hostMachine;

                AddMachine(this.hostMachine, hostConnection);

                // Add host gamer with id 0, important for NetworkSession.Host property
                AddGamer(new NetworkGamer(this.hostMachine, 0, privateGamerSlots > 0, false, hostDisplayName, hostGamertag));
            }

            this.maxGamers = maxGamers;
            this.privateGamerSlots = privateGamerSlots;

            if (isHost)
            {
                // Add local gamers directly to make sure that there is always at least one gamer in the session
                byte id = 0;
                foreach (var gamer in localGamers)
                {
                    AddGamer(new LocalNetworkGamer(gamer, this.localMachine, id++, GetOpenPrivateGamerSlots() > 0));
                }
                if (allGamers.Count == 0)
                {
                    throw new InvalidOperationException();
                }
            }
            else
            {
                foreach (var gamer in localGamers)
                {
                    AddLocalGamer(gamer);
                }
            }

            this.AllGamers = new GamerCollection<NetworkGamer>(new List<NetworkGamer>(), this.allGamers);
            this.PreviousGamers = new GamerCollection<NetworkGamer>(new List<NetworkGamer>(), this.previousGamers);
            this.RemoteGamers = new GamerCollection<NetworkGamer>(new List<NetworkGamer>(), this.remoteGamers);
            this.LocalGamers = new GamerCollection<LocalNetworkGamer>(new List<LocalNetworkGamer>(), this.localGamers);
            this.BytesPerSecondReceived = 0;
            this.BytesPerSecondSent = 0;

            this.SimulatedLatency = TimeSpan.Zero;
            this.SimulatedPacketLoss = 0.0f;

            SignedInGamer.SignedOut += LocalGamerSignedOut;
        }

        public GamerCollection<NetworkGamer> AllGamers { get; private set; }
        public GamerCollection<NetworkGamer> PreviousGamers { get; private set; }
        public GamerCollection<NetworkGamer> RemoteGamers { get; private set; }
        public GamerCollection<LocalNetworkGamer> LocalGamers { get; private set; }
        public int BytesPerSecondReceived { get; private set; }
        public int BytesPerSecondSent { get; private set; }

        /// <summary>
        /// Determines whether this session has failed master server validation at least once. Only applicable
        /// to sessions of type PlayerMatch and Ranked. This is a MonoGame-only extension.
        /// </summary>
        public bool HasFailedMasterServerValidation
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                if (type != NetworkSessionType.PlayerMatch && type != NetworkSessionType.Ranked)
                {
                    throw new InvalidOperationException("Can only be invoked on a session of type PlayerMarch or Ranked");
                }
                return hasFailedMasterServerValidation;
            }
        }

        /// <summary>
        /// Determines whether this session has successfully been registered as a host with the master server.
        /// Only applicable to sessions of type PlayerMatch and Ranked. Only the host of a session is allowed
        /// to call this property. This is a MonoGame-only extension.
        /// </summary>
        public bool IsRegisteredAsHostWithMasterServer
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                if (!isHost)
                {
                    throw new InvalidOperationException("Only the host can perform this action");
                }
                if (type != NetworkSessionType.PlayerMatch && type != NetworkSessionType.Ranked)
                {
                    throw new InvalidOperationException("Can only be invoked on a session of type PlayerMarch or Ranked");
                }
                return isRegisteredAsHostWithMasterServer;
            }
        }

        /// <summary>
        /// A generic string that is provided by the master server after RequestMasterServerGeneralInfo() has been
        /// called. Will be null until the first response has been received. Only applicable to sessions of type
        /// PlayerMatch and Ranked.
        ///
        /// This property can be used to synchronize a game mechanic such as time or difficulty accross multiple
        /// sessions or to check if the game is up to date. See the corresponding RequestMasterServerGeneralInfo()
        /// method. This is a MonoGame-only extension.
        /// </summary>
        public string MasterServerGeneralInfo
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return masterServerGeneralInfo;
            }
        }

        /// <summary>
        /// Request a general info string from the master server. Only applicable to the PlayerMatch and Ranked
        /// session types. See summary for the MasterServerGeneralInfo property.This is a MonoGame-only extension.
        /// </summary>
        public void RequestMasterServerGeneralInfo()
        {
            if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
            if (type != NetworkSessionType.PlayerMatch && type != NetworkSessionType.Ranked)
            {
                throw new InvalidOperationException("Can only be invoked on a session of type PlayerMarch or Ranked");
            }
            NetworkMasterServer.RequestGeneralInfo(peer);
        }

        public bool IsDisposed { get; private set; }

        public NetworkSessionType SessionType
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return type;
            }
        }

        public NetworkSessionProperties SessionProperties
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return properties;
            }
        }

        public NetworkSessionState SessionState
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return state;
            }
        }

        public bool AllowHostMigration
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return allowHostMigration;
            }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                if (!isHost)
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

                    SendSessionStateChanged(null);
                }*/
            }
        }

        public bool AllowJoinInProgress
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return allowJoinInProgress;
            }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                if (!isHost)
                {
                    throw new InvalidOperationException("Only the host can perform this action");
                }
                if (allowJoinInProgress != value)
                {
                    allowJoinInProgress = value;

                    SendSessionStateChanged(null);
                }
            }
        }

        public NetworkGamer Host
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return gamerFromId[0];
            }
        }

        public bool IsEveryoneReady
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                foreach (var gamer in allGamers)
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
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return isHost;
            }
        }

        public int MaxGamers
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return maxGamers;
            }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                if (!isHost)
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

                    SendSessionStateChanged(null);
                }
            }
        }

        internal int MaxPossiblePrivateGamerSlots
        {
            get
            {
                int usedPublicSlots = 0;
                foreach (var gamer in allGamers)
                {
                    if (!gamer.isPrivateSlot)
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
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                return privateGamerSlots;
            }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
                if (!isHost)
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

                    SendSessionStateChanged(null);
                }
            }
        }

        public TimeSpan SimulatedLatency // TODO: Should be applied even to local messages
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
#if DEBUG
                return TimeSpan.FromSeconds(peer.Configuration.SimulatedRandomLatency);
#else
                return TimeSpan.Zero;
#endif
            }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
#if DEBUG
                peer.Configuration.SimulatedRandomLatency = (float)value.TotalSeconds;
#endif
            }
        }

        public float SimulatedPacketLoss // TODO: Should be applied even to local messages
        {
            get
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
#if DEBUG
                return peer.Configuration.SimulatedLoss;
#else
                return 0.0f;
#endif
            }
            set
            {
                if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
#if DEBUG
                peer.Configuration.SimulatedLoss = value;
#endif
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
            if (IsDisposed) throw new ObjectDisposedException("NetworkSession");

            if (localGamerFromSignedInGamer.ContainsKey(signedInGamer))
            {
                return;
            }

            if (pendingSignedInGamers.Contains(signedInGamer))
            {
                // Already waiting to join
                return;
            }

            if (GetOpenSlotsForMachine(localMachine) > 0)
            {
                pendingSignedInGamers.Add(signedInGamer);

                SendGamerIdRequest();
            }
        }

        public void RemoveLocalGamer(SignedInGamer gamer)
        {
            if (IsDisposed) throw new ObjectDisposedException("NetworkSession");

            pendingSignedInGamers.Remove(gamer);

            if (localGamerFromSignedInGamer.ContainsKey(gamer))
            {
                SendGamerLeft(localGamerFromSignedInGamer[gamer]);
            }
        }

        public void StartGame()
        {
            if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
            if (!isHost)
            {
                throw new InvalidOperationException("Only the host can perform this action");
            }
            if (state != NetworkSessionState.Lobby)
            {
                throw new InvalidOperationException("The game can only be started from the lobby state");
            }
            if (!IsEveryoneReady)
            {
                throw new InvalidOperationException("The game cannot be started unless everyone is ready");
            }

            if (gameStartRequestThisFrame)
            {
                return;
            }
            SendStartGame();
            gameStartRequestThisFrame = true;
        }

        public void EndGame()
        {
            if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
            if (!isHost)
            {
                throw new InvalidOperationException("Only the host can perform this action");
            }
            if (state != NetworkSessionState.Playing)
            {
                throw new InvalidOperationException("The game can only end from the playing state");
            }

            if (gameEndRequestThisFrame)
            {
                return;
            }
            SendEndGame();
            gameEndRequestThisFrame = true;
        }

        public void ResetReady()
        {
            if (IsDisposed) throw new ObjectDisposedException("NetworkSession");
            if (!isHost)
            {
                throw new InvalidOperationException("Only the host can perform this action");
            }

            if (resetReadyRequestThisFrame)
            {
                return;
            }
            SendResetReady();
            resetReadyRequestThisFrame = true;
        }

        /// <summary>
        /// Find the gamer with the given unique id. Note that this method may return null if the message containing
        /// the unique id arrived to a peer before the internal GamerJoined message arrived.
        /// </summary>
        /// <param name="gamerId"></param>
        /// <returns></returns>
        public NetworkGamer FindGamerById(byte gamerId)
        {
            if (IsDisposed) throw new ObjectDisposedException("NetworkSession");

            if (gamerFromId.ContainsKey(gamerId))
            {
                var gamer = gamerFromId[gamerId];
                if (gamer.state == NetworkGamerState.Added)
                {
                    return gamer;
                }
            }
            return null;
        }

        private void AddMachine(NetworkMachine machine, NetConnection connection)
        {
            allMachines.Add(machine);
            machineFromId.Add(machine.id, machine);
            if (connection != null)
            {
                connectionFromMachine.Add(machine, connection);
            }
        }

        private void RemoveMachine(NetworkMachine machine)
        {
            for (int i = machine.gamers.Count - 1; i >= 0; i--)
            {
                RemoveGamer(machine.gamers[i]);
            }

            allMachines.Remove(machine);
            machineFromId.Remove(machine.id);
            connectionFromMachine.Remove(machine);
        }

        private void RemoveAllMachines()
        {
            for (int i = allMachines.Count - 1; i >= 0; i--)
            {
                RemoveMachine(allMachines[i]);
            }
        }

        private void UpdateMachineRoundtrips()
        {
            // TODO: Don't assume the same ping to host
            foreach (var machine in allMachines)
            {
                if (!connectionFromMachine.ContainsKey(machine))
                {
                    continue;
                }

                float seconds = connectionFromMachine[machine].AverageRoundtripTime;
                if (!isHost && !machine.isHost)
                {
                    seconds *= 2.0f;
                }
                machine.roundtripTime = TimeSpan.FromSeconds(seconds);
            }
        }

        private void AddGamer(NetworkGamer gamer)
        {
            if (gamer.state != NetworkGamerState.Pending)
            {
                throw new InvalidOperationException("Only new gamers can be added");
            }
            gamer.state = NetworkGamerState.Added;

            gamer.machine.gamers.Add(gamer);

            allGamers.Add(gamer);
            allGamers.Sort(NetworkGamerIdComparer.Instance);

            if (gamer.IsLocal)
            {
                var localGamer = (LocalNetworkGamer)gamer;
                localGamers.Add(localGamer);
                localGamers.Sort(NetworkGamerIdComparer.Instance);
                localGamerFromSignedInGamer.Add(localGamer.signedInGamer, localGamer);
            }
            else
            {
                remoteGamers.Add(gamer);
                remoteGamers.Sort(NetworkGamerIdComparer.Instance);
            }

            if (!gamerFromId.ContainsKey(gamer.id))
            {
                gamerFromId.Add(gamer.id, gamer);
                
            }
            if (gamerFromId[gamer.id] != gamer)
            {
                throw new InvalidOperationException();
            }

            InvokeGamerJoinedEvent(new GamerJoinedEventArgs(gamer));
        }

        private void RemoveGamer(NetworkGamer gamer)
        {
            if (gamer.state != NetworkGamerState.Added)
            {
                throw new InvalidOperationException("Only gamers in the session can be removed");
            }
            bool wasHost = gamer.IsHost;
            gamer.state = NetworkGamerState.Removed;

            gamer.machine.gamers.Remove(gamer);

            allGamers.Remove(gamer);

            if (gamer.IsLocal)
            {
                var localGamer = (LocalNetworkGamer)gamer;
                localGamers.Remove(localGamer);
                localGamerFromSignedInGamer.Remove(localGamer.signedInGamer);
            }
            else
            {
                remoteGamers.Remove(gamer);
            }

            gamerFromId.Remove(gamer.id);

            AddPreviousGamer(gamer);
            InvokeGamerLeftEvent(new GamerLeftEventArgs(gamer));

            if (wasHost || localGamers.Count == 0)
            {
                End(NetworkSessionEndReason.ClientSignedOut);
            }
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
            if (IsDisposed)
            {
                return;
            }

            RemoveLocalGamer(args.Gamer);
        }

        internal void DisconnectMachine(NetworkMachine machine, NetworkSessionEndReason reason)
        {
            if (!isHost) throw new InvalidOperationException();

            if (connectionFromMachine[machine].Status == NetConnectionStatus.Connected)
            {
                connectionFromMachine[machine].Disconnect(reason.ToString());
            }
        }

        private bool GetUniqueId<T>(Dictionary<byte, T> lookupTable, out byte id)
        {
            // Be careful not to use <= or a value greater than 255 here
            for (byte candidate = 0; candidate < 255; candidate++) // 255 is reserved for "error"
            {
                if (!lookupTable.ContainsKey(candidate))
                {
                    id = candidate;
                    return true;
                }
            }
            id = 255;
            return false;
        }

        internal void SilentUpdate()
        {
            if (IsDisposed)
            {
                return;
            }

            // Recycle inbound packets that the user has read from the last frame
            foreach (var localGamer in localGamers)
            {
                localGamer.RecycleInboundPackets();
            }

            ReceiveMessages();

            if (IsDisposed)
            {
                return;
            }

            // Add delayed inbound packets if sender has joined (Might add new inbound packets)
            foreach (var localGamer in localGamers)
            {
                localGamer.TryAddDelayedInboundPackets();
            }

            RegisterWithMasterServer();

            peer.FlushSendQueue();

            UpdateMachineRoundtrips();

            gameStartRequestThisFrame = false;
            gameEndRequestThisFrame = false;
            resetReadyRequestThisFrame = false;
        }

        private void TriggerEvents(bool recursive)
        {
            int originalCount = eventQueue.Count;

            // Do not use foreach as eventQueue might change
            for (int i = 0; i < (recursive ? eventQueue.Count : originalCount); i++)
            {
                // Performance is not a problem since events are rare!
                var arg = eventQueue[i];
                if (arg is GamerJoinedEventArgs)
                {
                    if (GamerJoined != null)
                    {
                        GamerJoined.Invoke(this, arg as GamerJoinedEventArgs);
                    }
                }
                else if (arg is GamerLeftEventArgs)
                {
                    if (GamerLeft != null)
                    {
                        GamerLeft.Invoke(this, arg as GamerLeftEventArgs);
                    }
                }
                else if (arg is GameStartedEventArgs)
                {
                    if (GameStarted != null)
                    {
                        GameStarted.Invoke(this, arg as GameStartedEventArgs);
                    }
                }
                else if (arg is GameEndedEventArgs)
                {
                    if (GameEnded != null)
                    {
                        GameEnded.Invoke(this, arg as GameEndedEventArgs);
                    }
                }
                else if (arg is NetworkSessionEndedEventArgs)
                {
                    if (SessionEnded != null)
                    {
                        SessionEnded.Invoke(this, arg as NetworkSessionEndedEventArgs);
                    }
                }
            }

            if (recursive)
            {
                eventQueue.Clear();
            }
            else
            {
                eventQueue.RemoveRange(0, originalCount);
            }
        }

        public void Update()
        {
            if (IsDisposed) throw new ObjectDisposedException("NetworkSession");

            SilentUpdate();

            if (IsDisposed)
            {
                return;
            }

            TriggerEvents(true);

            // Update public gamer collections
            AllGamers.CopyFromReference();
            PreviousGamers.CopyFromReference();
            RemoteGamers.CopyFromReference();
            LocalGamers.CopyFromReference();

            foreach (var machine in allMachines)
            {
                machine.Gamers.CopyFromReference();
            }
        }

        internal void End(NetworkSessionEndReason reason)
        {
            if (IsDisposed || state == NetworkSessionState.Ended)
            {
                return;
            }

            // Note that we do not want to dispose the NetworkSession until the end of this call, as the user
            // might want to do some clean up on SessionEnded. Furthermore, the user might call Dispose() in the
            // callback so the early return above is important. Finally, make sure not to trigger any new events that
            // might arise.
            state = NetworkSessionState.Ended;

            if (isHost)
            {
                // Notify clients gracefully before shutting down
                foreach (var machine in allMachines)
                {
                    if (machine.isLocal)
                    {
                        continue;
                    }
                    DisconnectMachine(machine, NetworkSessionEndReason.HostEndedSession);
                }

                UnregisterWithMasterServer();

                peer.FlushSendQueue();
            }

            InvokeSessionEnded(new NetworkSessionEndedEventArgs(reason));

            TriggerEvents(false);

            RemoveAllMachines();

            peer.Shutdown("Done");

            Session = null;

            IsDisposed = true;
        }

        public void Dispose()
        {
            End(isHost ? NetworkSessionEndReason.HostEndedSession : NetworkSessionEndReason.Disconnected);
        }
    }
}
