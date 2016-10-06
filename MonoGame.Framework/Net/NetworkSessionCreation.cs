using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net.Backend;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    internal static class NetworkSessionCreation
    {
        private static NetPeerConfiguration CreateNetPeerConfig(bool specifyPort)
        {
            NetPeerConfiguration config = new NetPeerConfiguration("MonoGameApp");

            config.Port = specifyPort ? LidgrenBackend.Port : 0;
            config.AcceptIncomingConnections = true;

            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            return config;
        }

        public static NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            NetPeer peer = new NetPeer(CreateNetPeerConfig(true));

            try
            {
                peer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Internal error: " + e.Message, e);
            }

            NetworkSession.Session = new NetworkSession(new LidgrenBackend(peer), null, maxGamers, privateGamerSlots, sessionType, sessionProperties, localGamers);
            return NetworkSession.Session;
        }

        public static AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            // Send discover requests on subnet
            NetPeer discoverPeer = new NetPeer(CreateNetPeerConfig(false));
            discoverPeer.Start();
            discoverPeer.DiscoverLocalPeers(LidgrenBackend.Port);

            Thread.Sleep(LidgrenBackend.DiscoveryTime);

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
                        NetworkSessionProperties properties = new NetworkSessionProperties();
                        properties.Receive(msg);

                        int maxGamers = msg.ReadInt32();
                        int privateGamerSlots = msg.ReadInt32();
                        int currentGamerCount = msg.ReadInt32();
                        string hostGamertag = msg.ReadString();
                        int openPrivateGamerSlots = msg.ReadInt32();
                        int openPublicGamerSlots = msg.ReadInt32();

                        if (sessionType == remoteSessionType && searchProperties.SearchMatch(properties))
                        {
                            availableSessions.Add(new AvailableNetworkSession(msg.SenderEndPoint, localGamers, maxGamers, privateGamerSlots, sessionType, currentGamerCount, hostGamertag, openPrivateGamerSlots, openPublicGamerSlots, properties));
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

            discoverPeer.Shutdown("Discovery complete");

            return new AvailableNetworkSessionCollection(availableSessions);
        }

        public static NetworkSession Join(AvailableNetworkSession availableSession)
        {
            // TODO: NetworkSessionJoinException if availableSession full/not joinable/cannot be found
            NetPeer peer = new NetPeer(CreateNetPeerConfig(false));
            peer.Start();
            peer.Connect(availableSession.remoteEndPoint);

            Thread.Sleep(LidgrenBackend.JoinTime);

            if (peer.ConnectionsCount != 1)
            {
                peer.Shutdown("Connection failed");
                throw new NetworkSessionJoinException("Connection failed", NetworkSessionJoinError.SessionNotFound);
            }
            
            int maxGamers = availableSession.maxGamers;
            int privateGamerSlots = availableSession.privateGamerSlots;
            NetworkSessionType sessionType = availableSession.sessionType;
            NetworkSessionProperties sessionProperties = availableSession.SessionProperties;
            IEnumerable<SignedInGamer> localGamers = availableSession.localGamers;

            NetworkSession.Session = new NetworkSession(new LidgrenBackend(peer), availableSession.remoteEndPoint, maxGamers, privateGamerSlots, sessionType, sessionProperties, localGamers);
            return NetworkSession.Session;
        }
    }
}
