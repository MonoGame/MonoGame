using System;
using System.Net;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Microsoft.Xna.Framework.GamerServices;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal class LidgrenSessionCreator : SessionCreator
    {
        private const int DiscoveryTimeMs = 4000;
        private const int FullyConnectedPollingTimeMs = 50;
        private const int FullyConnectedTimeOutMs = 4000;

        private static bool WaitUntilFullyConnected(NetworkSession session)
        {
            int totalTime = 0;

            while (!session.IsFullyConnected)
            {
                if (totalTime > FullyConnectedTimeOutMs)
                {
                    return false;
                }

                session.SilentUpdate();

                Thread.Sleep(FullyConnectedPollingTimeMs);
                totalTime += FullyConnectedPollingTimeMs;
            }

            return true;
        }

        public override NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            var config = new NetPeerConfiguration(NetworkSessionSettings.GameAppId);
            config.Port = NetworkSessionSettings.Port;
            config.AcceptIncomingConnections = true;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            var peer = new NetPeer(config);
            try
            {
                peer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Lidgren error", e);
            }
            Debug.WriteLine("Peer started.");

            var session = new NetworkSession(new LidgrenBackend(peer), new PeerEndPoint[0], true, maxGamers, privateGamerSlots, sessionType, sessionProperties, localGamers);

            if (!WaitUntilFullyConnected(session))
            {
                session.Dispose();

                throw new NetworkException("Could not initialize session");
            }
            Debug.WriteLine("Peer fully connected.");

            return session;
        }

        private void AddAvailableNetworkSession(IPEndPoint localIPEndPoint, PeerEndPoint endPoint, NetworkSessionPublicInfo publicInfo, IEnumerable<SignedInGamer> localGamers, NetworkSessionType searchType, NetworkSessionProperties searchProperties, IList<AvailableNetworkSession> availableSessions)
        {
            if (searchType == publicInfo.sessionType && searchProperties.SearchMatch(publicInfo.sessionProperties))
            {
                var availableSession = new AvailableNetworkSession(endPoint, localGamers, publicInfo.maxGamers, publicInfo.privateGamerSlots, publicInfo.sessionType, publicInfo.currentGamerCount, publicInfo.hostGamertag, publicInfo.openPrivateGamerSlots, publicInfo.openPublicGamerSlots, publicInfo.sessionProperties);
                availableSession.Tag = localIPEndPoint;
                availableSessions.Add(availableSession);
            }
        }

        public override AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            var masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            var config = new NetPeerConfiguration(NetworkSessionSettings.GameAppId);
            config.Port = 0;
            config.AcceptIncomingConnections = false;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);

            var discoverPeer = new NetPeer(config);
            try
            {
                discoverPeer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Lidgren error", e);
            }
            Debug.WriteLine("Discovery peer started.");

            // Send discovery request
            if (sessionType == NetworkSessionType.SystemLink)
            {
                Debug.WriteLine("Sending local discovery request...");

                discoverPeer.DiscoverLocalPeers(NetworkSessionSettings.Port);
            }
            else if (sessionType == NetworkSessionType.PlayerMatch || sessionType == NetworkSessionType.Ranked)
            {
                Debug.WriteLine("Sending discovery request to master server...");

                var request = discoverPeer.CreateMessage();
                request.Write(discoverPeer.Configuration.AppIdentifier);
                request.Write((byte)MasterServerMessageType.RequestHosts);
                discoverPeer.SendUnconnectedMessage(request, masterServerEndPoint);
            }
            else
            {
                throw new InvalidOperationException();
            }

            // Wait for answers
            Thread.Sleep(DiscoveryTimeMs);

            // Get list of answers
            var availableSessions = new List<AvailableNetworkSession>();

            NetIncomingMessage rawMsg;
            while ((rawMsg = discoverPeer.ReadMessage()) != null)
            {
                if (rawMsg.MessageType == NetIncomingMessageType.UnconnectedData)
                {
                    if (rawMsg.SenderEndPoint.Equals(masterServerEndPoint))
                    {
                        var msg = new LidgrenIncomingMessage(rawMsg);
                        var hostEndPoint = LidgrenEndPoint.Parse(msg.ReadString());
                        var hostPublicInfo = NetworkSessionPublicInfo.FromMessage(msg);

                        AddAvailableNetworkSession(null, hostEndPoint, hostPublicInfo, localGamers, sessionType, searchProperties, availableSessions);
                    }
                }
                else if (rawMsg.MessageType == NetIncomingMessageType.DiscoveryResponse)
                {
                    var msg = new LidgrenIncomingMessage(rawMsg);
                    var hostEndPoint = LidgrenEndPoint.Parse(msg.ReadString());
                    var hostPublicInfo = NetworkSessionPublicInfo.FromMessage(msg);

                    AddAvailableNetworkSession(rawMsg.SenderEndPoint, hostEndPoint, hostPublicInfo, localGamers, sessionType, searchProperties, availableSessions);
                }
                else
                {
                    switch (rawMsg.MessageType)
                    {
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            Debug.WriteLine("Lidgren: " + rawMsg.ReadString());
                            break;
                        default:
                            Debug.WriteLine("Unhandled type: " + rawMsg.MessageType);
                            break;
                    }
                }

                discoverPeer.Recycle(rawMsg);
            }
            discoverPeer.Shutdown("Discovery complete");
            Debug.WriteLine("Discovery peer shut down.");

            return new AvailableNetworkSessionCollection(availableSessions);
        }

        public override NetworkSession Join(AvailableNetworkSession availableSession)
        {
            var masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            var config = new NetPeerConfiguration(NetworkSessionSettings.GameAppId);
            config.Port = 0;
            config.AcceptIncomingConnections = true;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
            // When support for host migration is added:
            //config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            //config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);

            var peer = new NetPeer(config);
            try
            {
                peer.Start();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Lidgren error: " + e.Message, e);
            }
            Debug.WriteLine("Peer started.");

            var backend = new LidgrenBackend(peer);
            var session = new NetworkSession(backend,
                new PeerEndPoint[] { availableSession.HostEndPoint },
                false,
                availableSession.MaxGamers,
                availableSession.PrivateGamerSlots,
                availableSession.SessionType,
                availableSession.SessionProperties,
                availableSession.LocalGamers);

            if (availableSession.SessionType == NetworkSessionType.SystemLink)
            {
                if ((session as ISessionBackendListener).AllowConnectionToTargetAsClient(availableSession.HostEndPoint))
                {
                    (backend.LocalPeer as LocalPeer).Connect((IPEndPoint)availableSession.Tag);
                }
            }
            else if (availableSession.SessionType == NetworkSessionType.PlayerMatch || availableSession.SessionType == NetworkSessionType.Ranked)
            {
                // Note: Actual Connect call is handled by NetworkSession once NAT introduction is successful
                var msg = peer.CreateMessage();
                msg.Write(peer.Configuration.AppIdentifier);
                msg.Write((byte)MasterServerMessageType.RequestIntroduction);
                msg.Write((availableSession.HostEndPoint as LidgrenEndPoint).ToString());
                msg.Write((backend.LocalPeer as LocalPeer).InternalIp);
                peer.SendUnconnectedMessage(msg, masterServerEndPoint);
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (!WaitUntilFullyConnected(session))
            {
                session.Dispose();

                throw new NetworkSessionJoinException("Could not fully connect to session");
            }
            Debug.WriteLine("Peer fully connected.");

            return session;
        }
    }
}
