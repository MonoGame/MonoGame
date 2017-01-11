﻿using System;
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
        private const int DiscoveryTime = 2000;
        private const int FullyConnectedPollingTime = 50;
        private const int FullyConnectedTimeOut = 2000;

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

        public override NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties)
        {
            NetPeerConfiguration config = new NetPeerConfiguration(NetworkSessionSettings.GameAppId);
            config.Port = NetworkSessionSettings.Port;
            config.AcceptIncomingConnections = true;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.NatIntroductionSuccess);
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

            Debug.WriteLine("Peer started.");

            NetworkSession session = new NetworkSession(new LidgrenBackend(peer), new PeerEndPoint[0], true, maxGamers, privateGamerSlots, sessionType, sessionProperties, localGamers);

            if (!WaitUntilFullyConnected(session))
            {
                throw new NetworkException("Could not initialize session");
            }

            return session;
        }

        private void AddAvailableNetworkSession(IPEndPoint localIPEndPoint, PeerEndPoint endPoint, NetworkSessionPublicInfo publicInfo, IEnumerable<SignedInGamer> localGamers, NetworkSessionType searchType, NetworkSessionProperties searchProperties, IList<AvailableNetworkSession> availableSessions)
        {
            if (searchType == publicInfo.sessionType && searchProperties.SearchMatch(publicInfo.sessionProperties))
            {
                AvailableNetworkSession availableSession = new AvailableNetworkSession(endPoint, localGamers, publicInfo.maxGamers, publicInfo.privateGamerSlots, publicInfo.sessionType, publicInfo.currentGamerCount, publicInfo.hostGamertag, publicInfo.openPrivateGamerSlots, publicInfo.openPublicGamerSlots, publicInfo.sessionProperties);

                availableSession.Tag = localIPEndPoint;

                availableSessions.Add(availableSession);
            }
        }

        public override AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties)
        {
            IPEndPoint masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            NetPeerConfiguration config = new NetPeerConfiguration(NetworkSessionSettings.GameAppId);
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

                NetOutgoingMessage request = discoverPeer.CreateMessage();
                request.Write(discoverPeer.Configuration.AppIdentifier);
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

            NetIncomingMessage rawMsg;
            while ((rawMsg = discoverPeer.ReadMessage()) != null)
            {
                if (rawMsg.MessageType == NetIncomingMessageType.UnconnectedData)
                {
                    if (rawMsg.SenderEndPoint.Equals(masterServerEndPoint))
                    {
                        IncomingMessage msg = new LidgrenIncomingMessage(rawMsg);
                        LidgrenEndPoint hostEndPoint = LidgrenEndPoint.Parse(msg.ReadString());
                        NetworkSessionPublicInfo hostPublicInfo = NetworkSessionPublicInfo.FromMessage(msg);

                        AddAvailableNetworkSession(null, hostEndPoint, hostPublicInfo, localGamers, sessionType, searchProperties, availableSessions);
                    }
                }
                else if (rawMsg.MessageType == NetIncomingMessageType.DiscoveryResponse)
                {
                    IncomingMessage msg = new LidgrenIncomingMessage(rawMsg);
                    LidgrenEndPoint hostEndPoint = LidgrenEndPoint.Parse(msg.ReadString());
                    NetworkSessionPublicInfo hostPublicInfo = NetworkSessionPublicInfo.FromMessage(msg);

                    AddAvailableNetworkSession(rawMsg.SenderEndPoint, hostEndPoint, hostPublicInfo, localGamers, sessionType, searchProperties, availableSessions);
                }
                else
                {
                    // Error checking
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
            IPEndPoint masterServerEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);

            NetPeerConfiguration config = new NetPeerConfiguration(NetworkSessionSettings.GameAppId);
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

            Debug.WriteLine("Peer started.");

            AvailableNetworkSession aS = availableSession;

            LidgrenBackend backend = new LidgrenBackend(peer);
            NetworkSession session = new NetworkSession(backend, new PeerEndPoint[] { aS.HostEndPoint }, false, aS.MaxGamers, aS.PrivateGamerSlots, aS.SessionType, aS.SessionProperties, aS.LocalGamers);

            if (aS.SessionType == NetworkSessionType.SystemLink)
            {
                if ((session as ISessionBackendListener).ConnectAsClientWhenIntroducedToTarget(aS.HostEndPoint))
                {
                    (backend.LocalPeer as LocalPeer).Connect((IPEndPoint)aS.Tag);
                }
            }
            else if (aS.SessionType == NetworkSessionType.PlayerMatch || aS.SessionType == NetworkSessionType.Ranked)
            {
                // Note: Actual connect call is handled by NetworkSession once nat introduction is successful
                NetOutgoingMessage msg = peer.CreateMessage();
                msg.Write(peer.Configuration.AppIdentifier);
                msg.Write((byte)MasterServerMessageType.RequestIntroduction);
                msg.Write((availableSession.HostEndPoint as LidgrenEndPoint).ToString());
                msg.Write((backend.LocalPeer as LocalPeer).InternalIPEndPoint);
                peer.SendUnconnectedMessage(msg, masterServerEndPoint);
            }
            else
            {
                throw new InvalidOperationException();
            }

            if (!WaitUntilFullyConnected(session))
            {
                throw new NetworkSessionJoinException("Could not fully connect to session");
            }

            return session;
        }
    }
}