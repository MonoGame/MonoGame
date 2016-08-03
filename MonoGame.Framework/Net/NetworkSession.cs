using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using Microsoft.Xna.Framework.GamerServices;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    internal enum CustomMessageType
    {
        JoinRequest, // Sent by host
        JoinSuccessful, // Sent by non-host
        GamerJoinRequest, // Sent by anyone
        GamerJoinResponse, // Sent by host
        GamerJoined, // Sent by anyone
        UserData
    }

    public sealed class NetworkSession : IDisposable
    {
        private static int Port = 14242;
        private static int DiscoveryTime = 1000;
        private static int JoinTime = 1000;

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
            // ArgumentOutOfRangeException if maxGamers/privateGamerSlots out of bounds
            // ObjectDisposedException if session disposed

            NetPeer peer = new NetPeer(CreateNetPeerConfig(true));

            try
            {
                peer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Internal error", e);
            }

            Session = new NetworkSession(peer, true, null, localGamers);
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
                        availableSessions.Add(new AvailableNetworkSession(msg.SenderEndPoint, localGamers, msg.ReadString()));
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
            // ObjectDisposedException if availableSession disposed
            // NetworkSessionJoinException if availableSession full/not joinable/cannot be found

            NetPeer peer = new NetPeer(CreateNetPeerConfig(false));
            peer.Start();
            peer.Connect(availableSession.remoteEndPoint);

            Thread.Sleep(JoinTime);

            if (peer.ConnectionsCount == 0)
            {
                throw new NetworkSessionJoinException("Connection failed", NetworkSessionJoinError.SessionNotFound);
            }

            Session = new NetworkSession(peer, false, peer.GetConnection(availableSession.remoteEndPoint), availableSession.gamers);
            return Session;
        }

        private NetPeer peer;
        private NetworkMachine machine;
        private NetConnection hostConnection;

        private IList<SignedInGamer> pendingSignedInGamers;
        private ICollection<IPEndPoint> pendingEndPoints = new List<IPEndPoint>();
        
        // Host stores which connections were open when a particular peer connected
        private Dictionary<NetConnection, ICollection<NetConnection>> pendingPeerConnections = new Dictionary<NetConnection, ICollection<NetConnection>>();

        internal NetworkSession(NetPeer peer, bool isHost, NetConnection hostConnection, IEnumerable<SignedInGamer> signedInGamers)
        {
            this.peer = peer;
            this.machine = new NetworkMachine(!isHost, true);
            this.hostConnection = hostConnection;
            this.pendingSignedInGamers = new List<SignedInGamer>(signedInGamers);

            this.IsHost = isHost;

            this.peer.Tag = this.machine;
        }

        public bool IsHost { get; }
        public NetworkGamer Host { get; }

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

        private bool GetNewUniqueId(out byte id)
        {
            // TODO: Calculate accurate
            id = 0;
            return true;
        }

        private bool IsConnectedToEndPoint(IPEndPoint endPoint)
        {
            return peer.GetConnection(endPoint) != null;
        }

        private void SendCustomMessage(CustomMessageType type)
        {
            foreach (NetConnection c in peer.Connections)
            {
                SendCustomMessage(type, c);
            }
        }

        private void SendCustomMessage(CustomMessageType type, NetConnection recipient)
        {
            peer.SendMessage(BuildCustomMessage(type, recipient), recipient, NetDeliveryMethod.ReliableUnordered, 0);
        }

        private NetOutgoingMessage BuildCustomMessage(CustomMessageType type, NetConnection recipient)
        {
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write((byte)type);

            switch (type)
            {
                case CustomMessageType.JoinRequest:
                    ICollection<NetConnection> requestedConnections = pendingPeerConnections[recipient];
                    msg.Write((int)requestedConnections.Count);
                    foreach (NetConnection c in requestedConnections)
                    {
                        msg.Write(c.RemoteEndPoint);
                    }
                    Debug.WriteLine("JoinRequest sent (with " + requestedConnections.Count + " connections)");
                    break;
                case CustomMessageType.JoinSuccessful:
                    Debug.WriteLine("JoinSuccessful sent");
                    break;
                case CustomMessageType.GamerJoinRequest:
                    Debug.WriteLine("GamerJoinRequest sent");
                    break;
                case CustomMessageType.GamerJoinResponse:
                    byte id;
                    if (GetNewUniqueId(out id))
                    {
                        msg.Write(true);
                        msg.Write(id);
                    }
                    else
                    {
                        msg.Write(false);
                    }
                    Debug.WriteLine("GamerJoinResponse sent");
                    break;
            }

            return msg;
        }

        private void SendGamerJoined(LocalNetworkGamer localGamer)
        {
            NetOutgoingMessage msg = peer.CreateMessage();
            msg.Write((byte)CustomMessageType.GamerJoined);

            msg.Write(localGamer.Gamertag);
            msg.Write(localGamer.Id);

            peer.SendMessage(msg, peer.Connections, NetDeliveryMethod.ReliableUnordered, 0);
        }

        private void ReceiveCustomMessage(NetIncomingMessage msg)
        {
            CustomMessageType type = (CustomMessageType)msg.ReadByte();

            if (type == CustomMessageType.JoinRequest)
            {
                Debug.WriteLine("JoinRequest received");
                pendingEndPoints.Clear();

                int requestedConnectionCount = msg.ReadInt32();
                for (int i = 0; i < requestedConnectionCount; i++)
                {
                    IPEndPoint endPoint = msg.ReadIPEndPoint();
                    pendingEndPoints.Add(endPoint);

                    if (!IsConnectedToEndPoint(endPoint))
                    {
                        peer.Connect(endPoint);
                    }
                }
            }
            else if (type == CustomMessageType.JoinSuccessful)
            {
                Debug.WriteLine("JoinSuccessful received");
                NetworkMachine remoteMachine = msg.SenderConnection.Peer.Tag as NetworkMachine;
                remoteMachine.isPending = false;
            }
            else if (type == CustomMessageType.GamerJoinRequest)
            {
                Debug.WriteLine("GamerJoinRequest received");
                if (IsHost)
                {
                    SendCustomMessage(CustomMessageType.GamerJoinResponse, msg.SenderConnection);
                }
            }
            else if (type == CustomMessageType.GamerJoinResponse)
            {
                Debug.WriteLine("GamerJoinResponse received");
                if (IsHost || msg.SenderConnection == hostConnection)
                {
                    if (msg.ReadBoolean())
                    {
                        byte uniqueId = msg.ReadByte();

                        // Now possible to create local network gamer
                        SignedInGamer signedInGamer = pendingSignedInGamers[0];
                        pendingSignedInGamers.RemoveAt(0);

                        LocalNetworkGamer localGamer = new LocalNetworkGamer(signedInGamer, uniqueId);
                        machine.AddLocalGamer(localGamer); // TODO: Move network gamer creation to gamer joined message

                        SendGamerJoined(localGamer);

                        GamerJoined.Invoke(this, new GamerJoinedEventArgs(localGamer));
                    }
                    else
                    {
                        Debug.WriteLine("Gamer join was not accepted by host!");
                    }
                }
            }
            else if (type == CustomMessageType.GamerJoined)
            {
                string gamertag = msg.ReadString();
                byte id = msg.ReadByte();
                Debug.WriteLine("Gamer joined received with display name " + gamertag + " and id " + id);
                // TODO: If message sent from this machine, create local network gamer, otherwise network gamer
                NetworkGamer remoteGamer = new NetworkGamer(false, id);
                NetworkMachine remoteMachine = msg.SenderConnection.Peer.Tag as NetworkMachine;
                remoteMachine.AddRemoteGamer(remoteGamer);

                // TODO: Fire GamerJoined event
                GamerJoined.Invoke(this, new GamerJoinedEventArgs(remoteGamer));
            }
        }

        public void Update()
        {
            // Handle incoming messages
            NetIncomingMessage msg;
            while ((msg = peer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    // Discovery
                    case NetIncomingMessageType.DiscoveryRequest:
                        Debug.WriteLine("Discovery request received");
                        NetOutgoingMessage response = peer.CreateMessage();
                        response.Write("Some Gamertag");
                        peer.SendDiscoveryResponse(response, msg.SenderEndPoint);
                        break;
                    // Peer state changes
                    case NetIncomingMessageType.StatusChanged:
                        NetConnectionStatus status = (NetConnectionStatus)msg.ReadByte();
                        Debug.WriteLine("Status now: " + status + "; Reason: " + msg.ReadString());

                        if (status == NetConnectionStatus.Connected)
                        {
                            NetConnection newConnection = msg.SenderConnection;

                            // Create a pending network machine
                            newConnection.Peer.Tag = new NetworkMachine(true, false);

                            if (IsHost)
                            {
                                // Save snapshot of current connections and send them to new peer
                                ICollection<NetConnection> requestedConnections = new HashSet<NetConnection>(peer.Connections);
                                requestedConnections.Remove(newConnection);
                                pendingPeerConnections.Add(newConnection, requestedConnections);

                                SendCustomMessage(CustomMessageType.JoinRequest, newConnection);
                            }
                        }

                        if (status == NetConnectionStatus.Disconnected)
                        {
                            if (IsHost)
                            {
                                NetConnection disconnectedConnection = msg.SenderConnection;

                                // If disconnected peer was pending, remove it
                                pendingPeerConnections.Remove(disconnectedConnection);

                                // Update pending peers
                                foreach (var pendingPeer in pendingPeerConnections)
                                {
                                    if (pendingPeer.Value.Contains(disconnectedConnection))
                                    {
                                        pendingPeer.Value.Remove(disconnectedConnection);

                                        SendCustomMessage(CustomMessageType.JoinRequest, pendingPeer.Key);
                                    }
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
                        ReceiveCustomMessage(msg);
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

            // Handle pending peer
            if (machine.isPending)
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
                    SendCustomMessage(CustomMessageType.JoinSuccessful);

                    for (int i = 0; i < pendingSignedInGamers.Count; i++)
                    {
                        SendCustomMessage(CustomMessageType.GamerJoinRequest, hostConnection);
                    }

                    machine.isPending = false;
                }
            }
        }

        public void Dispose()
        {
            peer.Shutdown("Peer done");
        }
    }
}