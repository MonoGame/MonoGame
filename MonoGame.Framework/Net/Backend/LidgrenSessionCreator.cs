using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework.GamerServices;

using Lidgren.Network;
using System.Net;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class LidgrenSessionCreator : ISessionCreator
    {
        private const int DiscoveryTime = 1000;
        private const int FullyConnectedPollingTime = 50;
        private const int FullyConnectedTimeOut = 500;

        private static bool WaitUntilFullyConnected(NetworkSession session)
        {
            int totalTime = 0;

            while (!session.IsFullyConnected)
            {
                if (totalTime > FullyConnectedTimeOut)
                {
                    return false;
                }

                session.SilentUpdate();

                Thread.Sleep(FullyConnectedPollingTime);
                totalTime += FullyConnectedPollingTime;
            }

            return true;
        }

        public NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            if (sessionType == NetworkSessionType.PlayerMatch || sessionType == NetworkSessionType.Ranked)
            {
                throw new NotImplementedException("PlayerMatch and Ranked are not implemented yet");
            }

            NetPeerConfiguration config = new NetPeerConfiguration(NetworkSessionSettings.AppId);
            config.Port = NetworkSessionSettings.Port;
            config.AcceptIncomingConnections = true;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            NetPeer peer = new NetPeer(config);

            try
            {
                peer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Lidgren error: " + e.Message, e);
            }

            NetworkSession session = new NetworkSession(new LidgrenBackend(peer, null), maxGamers, privateGamerSlots, sessionType, sessionProperties, localGamers);

            if (!WaitUntilFullyConnected(session))
            {
                throw new NetworkException("Could not initialize session");
            }

            return session;
        }

        private static void AddAvailableNetworkSession(NetIncomingMessage msg, long id, IPEndPoint endPoint, IEnumerable<SignedInGamer> localGamers, NetworkSessionType searchType, NetworkSessionProperties searchProperties, IList<AvailableNetworkSession> availableSessions)
        {
            // Use backend message type in order to unpack session properties properly
            IncomingMessage incomingMsg = new IncomingMessage();
            incomingMsg.Buffer = msg;

            DiscoveryContents contents = new DiscoveryContents();
            contents.Unpack(incomingMsg);

            if (searchType == contents.sessionType && searchProperties.SearchMatch(contents.sessionProperties))
            {
                AvailableNetworkSession availableSession = new AvailableNetworkSession(endPoint, localGamers, contents.maxGamers, contents.privateGamerSlots, contents.sessionType, contents.currentGamerCount, contents.hostGamertag, contents.openPrivateGamerSlots, contents.openPublicGamerSlots, contents.sessionProperties);

                availableSession.Tag = id;

                availableSessions.Add(availableSession);
            }
        }

        public AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            IPEndPoint masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            NetPeerConfiguration config = new NetPeerConfiguration(NetworkSessionSettings.AppId);
            config.Port = 0;
            config.AcceptIncomingConnections = false;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            NetPeer discoverPeer = new NetPeer(config);

            try
            {
                discoverPeer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Lidgren error: " + e.Message, e);
            }

            // Send discovery request
            if (sessionType == NetworkSessionType.SystemLink)
            {
                discoverPeer.DiscoverLocalPeers(NetworkSessionSettings.Port);
            }
            else if (sessionType == NetworkSessionType.PlayerMatch || sessionType == NetworkSessionType.Ranked)
            {
                NetOutgoingMessage request = discoverPeer.CreateMessage();
                request.Write((byte)MasterServerMessageType.RequestHosts);
                discoverPeer.SendUnconnectedMessage(request, masterServerEndPoint);
            }
            else
            {
                throw new InvalidOperationException();
            }

            // Wait for answers
            Thread.Sleep(DiscoveryTime);

            // Get list of answers
            List<AvailableNetworkSession> availableSessions = new List<AvailableNetworkSession>();

            NetIncomingMessage msg;
            while ((msg = discoverPeer.ReadMessage()) != null)
            {
                switch (msg.MessageType)
                {
                    case NetIncomingMessageType.UnconnectedData:
                        if (!msg.SenderEndPoint.Equals(masterServerEndPoint))
                        {
                            long hostId = msg.ReadInt64();
                            IPEndPoint hostEndPoint = msg.ReadIPEndPoint();

                            AddAvailableNetworkSession(msg, hostId, hostEndPoint, localGamers, sessionType, searchProperties, availableSessions);
                        }
                        break;
                    case NetIncomingMessageType.DiscoveryResponse:
                        AddAvailableNetworkSession(msg, -1, msg.SenderEndPoint, localGamers, sessionType, searchProperties, availableSessions);
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
            IPEndPoint masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            NetPeerConfiguration config = new NetPeerConfiguration(NetworkSessionSettings.AppId);
            config.Port = 0;
            config.AcceptIncomingConnections = true;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest); // if peer becomes host in the future
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval); // if peer becomes host in the future
            NetPeer peer = new NetPeer(config);

            try
            {
                peer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Lidgren error: " + e.Message, e);
            }

            AvailableNetworkSession aS = availableSession;

            if (aS.SessionType == NetworkSessionType.SystemLink)
            {
                peer.Connect(aS.RemoteEndPoint);
            }
            else if (aS.SessionType == NetworkSessionType.PlayerMatch || aS.SessionType == NetworkSessionType.Ranked)
            {
                // Note: Actual connect call is handled by backend once nat introduction is successful
                NetOutgoingMessage msg = peer.CreateMessage();
                msg.Write((byte)MasterServerMessageType.RequestIntroduction);
                msg.Write((long)availableSession.Tag);
                peer.SendUnconnectedMessage(msg, masterServerEndPoint);
            }
            else
            {
                throw new InvalidOperationException();
            }

            NetworkSession session = new NetworkSession(new LidgrenBackend(peer, aS.RemoteEndPoint), aS.MaxGamers, aS.PrivateGamerSlots, aS.SessionType, aS.SessionProperties, aS.LocalGamers);

            if (!WaitUntilFullyConnected(session))
            {
                throw new NetworkSessionJoinException("Could not fully connect to session");
            }

            return session;
        }
    }
}