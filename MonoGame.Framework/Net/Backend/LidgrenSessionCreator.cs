using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework.GamerServices;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class LidgrenSessionCreator : ISessionCreator
    {
        private const int Port = 14242;
        private const int DiscoveryTime = 1000;
        private const int JoinTime = 1000;

        private static NetPeerConfiguration CreateNetPeerConfig(bool specifyPort)
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

        public NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            if (sessionType == NetworkSessionType.PlayerMatch || sessionType == NetworkSessionType.Ranked)
            {
                throw new NotImplementedException("PlayerMatch and Ranked are not implemented yet");
            }

            NetPeer peer = new NetPeer(CreateNetPeerConfig(true));

            try
            {
                peer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Lidgren error: " + e.Message, e);
            }

            return new NetworkSession(new LidgrenBackend(peer), null, maxGamers, privateGamerSlots, sessionType, sessionProperties, localGamers);
        }

        public AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            if (sessionType == NetworkSessionType.PlayerMatch || sessionType == NetworkSessionType.Ranked)
            {
                throw new NotImplementedException("PlayerMatch and Ranked are not implemented yet");
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
                        // Use backend message type in order to unpack session properties properly
                        IncomingMessage incomingMsg = new IncomingMessage();
                        incomingMsg.Buffer = msg;

                        NetworkSessionType remoteSessionType = (NetworkSessionType)incomingMsg.ReadByte();
                        NetworkSessionProperties properties = new NetworkSessionProperties();
                        properties.Unpack(incomingMsg);
                        int maxGamers = incomingMsg.ReadInt();
                        int privateGamerSlots = incomingMsg.ReadInt();
                        int currentGamerCount = incomingMsg.ReadInt();
                        string hostGamertag = incomingMsg.ReadString();
                        int openPrivateGamerSlots = incomingMsg.ReadInt();
                        int openPublicGamerSlots = incomingMsg.ReadInt();

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

        public NetworkSession Join(AvailableNetworkSession availableSession)
        {
            // TODO: NetworkSessionJoinException if availableSession full/not joinable/cannot be found
            NetPeer peer = new NetPeer(CreateNetPeerConfig(false));
            peer.Start();
            peer.Connect(availableSession.remoteEndPoint);

            Thread.Sleep(JoinTime);

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

            return new NetworkSession(new LidgrenBackend(peer), availableSession.remoteEndPoint, maxGamers, privateGamerSlots, sessionType, sessionProperties, localGamers);
        }
    }
}
