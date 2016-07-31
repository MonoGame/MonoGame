using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.GamerServices;

using Lidgren.Network;
using System.Threading;

namespace Microsoft.Xna.Framework.Net
{
    public sealed class NetworkSession
    {
        private static int Port = 14242;
        private static int DiscoveryTime = 1000;

        public static NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            NetworkSession session = new NetworkSession(true);

            session.Create();

            return session;
        }

        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            NetworkSession session = new NetworkSession(false);

            return session.Find();
        }

        private NetPeer peer;

        internal NetworkSession(bool host)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("MonoGameApp");

            if (host)
            {
                config.Port = Port;
            }
            config.AcceptIncomingConnections = true;

            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);

            peer = new NetPeer(config);
        }

        internal void Create()
        {
            peer.Start();
        }

        internal AvailableNetworkSessionCollection Find()
        {
            peer.Start();
            peer.DiscoverLocalPeers(Port);

            Thread.Sleep(DiscoveryTime);

            List<AvailableNetworkSession> availableSessions = new List<AvailableNetworkSession>();

            while (true)
            {
                NetIncomingMessage msg = peer.ReadMessage();
                if (msg == null)
                {
                    break;
                }

                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.DiscoveryRequest:
                        // Ignore own message
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        availableSessions.Add(new AvailableNetworkSession(msg.ReadString()));
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
            }

            return new AvailableNetworkSessionCollection(availableSessions);
        }

        public void Update()
        {
            while (true)
            {
                NetIncomingMessage msg = peer.ReadMessage();
                if (msg == null)
                {
                    break;
                }

                // Message decoding
                switch (msg.MessageType)
                {
                    // Discovery
                    case NetIncomingMessageType.DiscoveryRequest:
                        System.Diagnostics.Debug.WriteLine("discovery request received");
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
    }
}