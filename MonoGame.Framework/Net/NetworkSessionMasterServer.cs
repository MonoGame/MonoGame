using System;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
    internal enum MasterServerMessageType : byte
    {
        RegisterHost,
        UnregisterHost,
        RequestHosts,
        RequestIntroduction
    };

    internal class HostData
    {
        public Guid Guid;
        public IPEndPoint InternalIp;
        public IPEndPoint ExternalIp;
        public NetworkSessionPublicInfo PublicInfo;
        public DateTime LastUpdated;

        public HostData(Guid guid, IPEndPoint internalIp, IPEndPoint externalIp, NetworkSessionPublicInfo publicInfo)
        {
            Guid = guid;
            InternalIp = internalIp;
            ExternalIp = externalIp;
            PublicInfo = publicInfo;
            LastUpdated = DateTime.Now;
        }

        public override string ToString()
        {
            return $"[Guid: {Guid}, InternalIp: {InternalIp}, ExternalIp: {ExternalIp}]";
        }
    }

    public class NetworkSessionMasterServer
    {
        private static readonly TimeSpan ReportStatusInterval = TimeSpan.FromSeconds(60.0);

        private NetPeer serverPeer;
        private IDictionary<Guid, HostData> hosts = new Dictionary<Guid, HostData>();
        private DateTime lastReportedStatus = DateTime.MinValue;

        public void Start(string gameAppId)
        {
            var config = new NetPeerConfiguration(gameAppId)
            {
                Port = NetworkSessionSettings.MasterServerPort,
                AcceptIncomingConnections = false,
                AutoFlushSendQueue = true,
            };
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            serverPeer = new NetPeer(config);
            try
            {
                serverPeer.Start();
            }
            catch (Exception e)
            {
                throw new NetworkException("Could not start server peer", e);
            }

            Console.WriteLine($"Master server with game app id {gameAppId} started on port {config.Port}.");
        }

        private List<Guid> hostsToRemove = new List<Guid>();

        protected void TrimHosts()
        {
            var currentTime = DateTime.Now;
            var threshold = NetworkSessionSettings.MasterServerRegistrationInterval + TimeSpan.FromSeconds(5.0);

            hostsToRemove.Clear();

            foreach (var host in hosts)
            {
                if ((currentTime - host.Value.LastUpdated) > threshold)
                {
                    hostsToRemove.Add(host.Key);
                }
            }

            foreach (var endPoint in hostsToRemove)
            {
                var host = hosts[endPoint];
                hosts.Remove(endPoint);

                Console.WriteLine($"Host removed due to timeout. {host}");
            }
        }

        protected void ReportStatus()
        {
            var currentTime = DateTime.Now;

            if (currentTime - lastReportedStatus > ReportStatusInterval)
            {
                Console.WriteLine($"Status: {hosts.Count} registered hosts.");

                lastReportedStatus = currentTime;
            }
        }

        protected void ReceiveMessages()
        {
            NetIncomingMessage msg;
            while ((msg = serverPeer.ReadMessage()) != null)
            {
                if (msg.MessageType == NetIncomingMessageType.UnconnectedData)
                {
                    try
                    {
                        HandleMessage(msg);
                    }
                    catch (NetException e)
                    {
                        Console.WriteLine($"Encountered malformed message from {msg.SenderEndPoint}. Lidgren reports '{e.Message}'.");
                    }
                }
                else
                {
                    NetworkSession.HandleLidgrenMessage(msg);
                }
                serverPeer.Recycle(msg);
            }
        }

        internal static void RegisterHost(NetPeer peer, Guid guid, IPEndPoint internalIp, NetworkSessionPublicInfo publicInfo)
        {
            var request = peer.CreateMessage();
            request.Write(peer.Configuration.AppIdentifier);
            request.Write((byte)MasterServerMessageType.RegisterHost);
            request.Write(guid.ToString());
            request.Write(internalIp);
            publicInfo.Pack(request);

            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            peer.SendUnconnectedMessage(request, serverEndPoint);

            Debug.WriteLine($"Registering with master server (Guid: {guid}, InternalIp: {internalIp}, PublicInfo: ...)");
        }

        internal static void UnregisterHost(NetPeer peer, Guid guid)
        {
            var request = peer.CreateMessage();
            request.Write(peer.Configuration.AppIdentifier);
            request.Write((byte)MasterServerMessageType.UnregisterHost);
            request.Write(guid.ToString());

            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            peer.SendUnconnectedMessage(request, serverEndPoint);

            Debug.WriteLine($"Unregistering with master server (Guid: {guid})");
        }

        internal static void RequestHosts(NetPeer peer)
        {
            var request = peer.CreateMessage();
            request.Write(peer.Configuration.AppIdentifier);
            request.Write((byte)MasterServerMessageType.RequestHosts);

            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            peer.SendUnconnectedMessage(request, serverEndPoint);
        }

        internal static void SerializeRequestHostsResponse(NetOutgoingMessage response, Guid guid, NetworkSessionPublicInfo publicInfo)
        {
            response.Write(guid.ToString());
            publicInfo.Pack(response);
        }

        internal static bool ParseRequestHostsResponse(NetIncomingMessage response, out Guid guid, out NetworkSessionPublicInfo hostPublicInfo)
        {
            try
            {
                guid = new Guid(response.ReadString());
                hostPublicInfo = NetworkSessionPublicInfo.FromMessage(response);
                return true;
            }
            catch
            {
                guid = Guid.Empty;
                hostPublicInfo = null;
                return false;
            }
        }

        internal static void RequestIntroduction(NetPeer peer, Guid guid, IPEndPoint internalIp)
        {
            var request = peer.CreateMessage();
            request.Write(peer.Configuration.AppIdentifier);
            request.Write((byte)MasterServerMessageType.RequestIntroduction);
            request.Write(guid.ToString());
            request.Write(internalIp);

            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            peer.SendUnconnectedMessage(request, serverEndPoint);
        }

        protected void HandleMessage(NetIncomingMessage msg)
        {
            string senderGameAppId = msg.ReadString();
            if (!senderGameAppId.Equals(serverPeer.Configuration.AppIdentifier, StringComparison.Ordinal))
            {
                Console.WriteLine($"Received message with incorrect game app id from {msg.SenderEndPoint}.");
                return;
            }

            var messageType = (MasterServerMessageType)msg.ReadByte();
            if (messageType == MasterServerMessageType.RegisterHost)
            {
                var guid = new Guid(msg.ReadString());
                var internalIp = msg.ReadIPEndPoint();
                var externalIp = msg.SenderEndPoint;
                var publicInfo = NetworkSessionPublicInfo.FromMessage(msg);

                hosts[guid] = new HostData(guid, internalIp, externalIp, publicInfo);

                Console.WriteLine($"Host registered/updated. {hosts[guid]}");
            }
            else if (messageType == MasterServerMessageType.UnregisterHost)
            {
                var guid = new Guid(msg.ReadString());
                if (hosts.ContainsKey(guid))
                {
                    var host = hosts[guid];
                    if (msg.SenderEndPoint.Equals(host.ExternalIp))
                    {
                        hosts.Remove(guid);

                        Console.WriteLine($"Host unregistered. {host}");
                    }
                    else
                    {
                        Console.WriteLine($"Unregister requested for host not registered by {msg.SenderEndPoint}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Unregister requested for unknown host from {msg.SenderEndPoint}.");
                }
            }
            else if (messageType == MasterServerMessageType.RequestHosts)
            {
                foreach (var host in hosts.Values)
                {
                    var response = serverPeer.CreateMessage();
                    SerializeRequestHostsResponse(response, host.Guid, host.PublicInfo);
                    serverPeer.SendUnconnectedMessage(response, msg.SenderEndPoint);
                }

                Console.WriteLine($"List of {hosts.Count} hosts sent to {msg.SenderEndPoint}.");
            }
            else if (messageType == MasterServerMessageType.RequestIntroduction)
            {
                var guid = new Guid(msg.ReadString());
                if (hosts.ContainsKey(guid))
                {
                    var host = hosts[guid];
                    var clientInternalIp = msg.ReadIPEndPoint();
                    var clientExternalIp = msg.SenderEndPoint;

                    serverPeer.Introduce(host.InternalIp, host.ExternalIp, clientInternalIp, clientExternalIp, string.Empty);

                    Console.WriteLine($"Introduced host {host} and client [InternalIp: {clientInternalIp}, ExternalIp: {clientExternalIp}].");
                }
                else
                {
                    Console.WriteLine($"Introduction requested for unknwon host from {msg.SenderEndPoint}.");
                }
            }
        }

        public void Update()
        {
            if (serverPeer == null || serverPeer.Status == NetPeerStatus.NotRunning)
            {
                return;
            }

            ReceiveMessages();

            TrimHosts();

            ReportStatus();
        }

        public void Shutdown()
        {
            if (serverPeer == null || serverPeer.Status == NetPeerStatus.NotRunning || serverPeer.Status == NetPeerStatus.ShutdownRequested)
            {
                return;
            }

            serverPeer.Shutdown("Done");

            Console.WriteLine("Master server shut down.");
        }
    }
}
