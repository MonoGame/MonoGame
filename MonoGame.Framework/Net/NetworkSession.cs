using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.GamerServices;

using Lidgren.Network;
using System.Threading;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class NetworkSession : IDisposable
    {
        private static int Port = 14242;
        private static int DiscoveryTime = 1000;

        internal static NetworkSession Session = null;

        internal static NetPeerConfiguration CreateNetPeerConfig(bool specifyPort)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("MonoGameApp");

            config.Port = specifyPort ? Port : 0;
            config.AcceptIncomingConnections = true;

            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);

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

            Session = new NetworkSession(peer);
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
                        availableSessions.Add(new AvailableNetworkSession(msg.SenderEndPoint, msg.ReadString()));
                        break;
                    // Error checking
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        System.Diagnostics.Debug.WriteLine("Lidgren: " + msg.ReadString());
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unhandled type: " + msg.MessageType);
                        break;
                }

                discoverPeer.Recycle(msg);
            }

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

            peer.Connect(availableSession.remoteEndPoint);

            if (peer.ConnectionsCount == 0)
            {
                throw new NetworkSessionJoinException("Connection failed", NetworkSessionJoinError.SessionNotFound);
            }

            Session = new NetworkSession(peer);
            return Session;
        }

        private NetPeer peer;

        internal NetworkSession(NetPeer peer)
        {
            this.peer = peer;
        }

        public void Update()
        {
            NetIncomingMessage msg;
            while ((msg = peer.ReadMessage()) != null)
            {

                // Message decoding
                switch (msg.MessageType)
                {
                    // Discovery
                    case NetIncomingMessageType.DiscoveryRequest:
                        System.Diagnostics.Debug.WriteLine("Discovery request received");
                        NetOutgoingMessage response = peer.CreateMessage();
                        response.Write("Some Gamertag");
                        peer.SendDiscoveryResponse(response, msg.SenderEndPoint);
                        break;
                    // Error checking
                    case NetIncomingMessageType.VerboseDebugMessage:
                    case NetIncomingMessageType.DebugMessage:
                    case NetIncomingMessageType.WarningMessage:
                    case NetIncomingMessageType.ErrorMessage:
                        System.Diagnostics.Debug.WriteLine(msg.ReadString());
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine("Unhandled type: " + msg.MessageType);
                        break;
                }

                peer.Recycle(msg);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}