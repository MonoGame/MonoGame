using System;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
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
        public GuidEndPoint EndPoint;
        public IPEndPoint InternalIp;
        public IPEndPoint ExternalIp;
        public NetworkSessionPublicInfo PublicInfo;
        public DateTime LastUpdated;

        public HostData(GuidEndPoint endPoint, IPEndPoint internalIp, IPEndPoint externalIp, NetworkSessionPublicInfo publicInfo)
        {
            EndPoint = endPoint;
            InternalIp = internalIp;
            ExternalIp = externalIp;
            PublicInfo = publicInfo;
            LastUpdated = DateTime.Now;
        }

        public override string ToString()
        {
            return "[EndPoint: " + EndPoint + ", InternalIp: " + InternalIp + ", ExternalIp: " + ExternalIp + "]";
        }
    }

    internal class MasterServer : BaseMasterServer
    {
        private static readonly TimeSpan ReportStatusInterval = TimeSpan.FromSeconds(60.0);

        private NetPeer server;
        private IDictionary<GuidEndPoint, HostData> hosts = new Dictionary<GuidEndPoint, HostData>();
        private DateTime lastReportedStatus = DateTime.MinValue;

        public override void Start(string gameAppId)
        {
            var config = new NetPeerConfiguration(gameAppId);
            config.AcceptIncomingConnections = false;
            config.Port = NetworkSessionSettings.MasterServerPort;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            server = new NetPeer(config);
            server.Start();

            Console.WriteLine($"Master server with game app id {gameAppId} started on port {config.Port}.");
        }

        private IList<GuidEndPoint> hostsToRemove = new List<GuidEndPoint>();

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
            NetIncomingMessage rawMsg;
            while ((rawMsg = server.ReadMessage()) != null)
            {
                if (rawMsg.MessageType == NetIncomingMessageType.UnconnectedData)
                {
                    try
                    {
                        var msg = new IncomingMessage(rawMsg);
                        HandleMessage(msg, rawMsg.SenderEndPoint);
                    }
                    catch (NetException e)
                    {
                        Console.WriteLine($"Encountered malformed message from {rawMsg.SenderEndPoint}. Lidgren reports '{e.Message}'.");
                    }
                }
                else
                {
                    switch (rawMsg.MessageType)
                    {
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            Debug.WriteLine($"Lidgren: {rawMsg.ReadString()}");
                            break;
                        default:
                            Debug.WriteLine($"Unhandled type: {rawMsg.MessageType}");
                            break;
                    }
                }

                server.Recycle(rawMsg);
            }
        }

        public static void RegisterHost(NetPeer peer, GuidEndPoint endPoint, IPEndPoint internalIp, NetworkSessionPublicInfo publicInfo)
        {
            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            var msg = MessagePool.Outgoing.Get();
            msg.Write(peer.Configuration.AppIdentifier);
            msg.Write((byte)MasterServerMessageType.RegisterHost);
            msg.Write(endPoint);
            msg.Write(internalIp);
            publicInfo.Pack(msg);

            var request = peer.CreateMessage();
            request.Write(msg.Buffer);
            peer.SendUnconnectedMessage(request, serverEndPoint);
            MessagePool.Outgoing.Recycle(msg);
        }

        public static void UnregisterHost(NetPeer peer, GuidEndPoint endPoint)
        {
            var serverEndPoint = NetUtility.Resolve(NetworkSessionSettings.MasterServerAddress, NetworkSessionSettings.MasterServerPort);
            var msg = MessagePool.Outgoing.Get();
            msg.Write(peer.Configuration.AppIdentifier);
            msg.Write((byte)MasterServerMessageType.UnregisterHost);
            msg.Write(endPoint);

            var request = peer.CreateMessage();
            request.Write(msg.Buffer);
            peer.SendUnconnectedMessage(request, serverEndPoint);
            MessagePool.Outgoing.Recycle(msg);
        }

        protected void HandleMessage(IncomingMessage msg, IPEndPoint senderIpEndPoint)
        {
            string senderGameAppId = msg.ReadString();

            if (!senderGameAppId.Equals(server.Configuration.AppIdentifier, StringComparison.Ordinal))
            {
                Console.WriteLine($"Received message with incorrect game app id from {senderIpEndPoint}.");
                return;
            }

            var messageType = (MasterServerMessageType)msg.ReadByte();
            if (messageType == MasterServerMessageType.RegisterHost)
            {
                var hostEndPoint = msg.ReadGuidEndPoint();
                var internalIp = msg.ReadIPEndPoint();
                var externalIp = senderIpEndPoint;
                var publicInfo = NetworkSessionPublicInfo.FromMessage(msg);

                hosts[hostEndPoint] = new HostData(hostEndPoint, internalIp, externalIp, publicInfo);

                Console.WriteLine($"Host registered/updated. {hosts[hostEndPoint]}");
            }
            else if (messageType == MasterServerMessageType.UnregisterHost)
            {
                var hostEndPoint = msg.ReadGuidEndPoint();

                if (hosts.ContainsKey(hostEndPoint))
                {
                    var hostData = hosts[hostEndPoint];

                    if (senderIpEndPoint == hostData.ExternalIp)
                    {
                        hosts.Remove(hostEndPoint);

                        Console.WriteLine($"Host unregistered. {hostData}");
                    }
                    else
                    {
                        Console.WriteLine($"Unregister requested for host not registered by {senderIpEndPoint}.");
                    }
                }
                else
                {
                    Console.WriteLine($"Unregister requested for unknown host from {senderIpEndPoint}.");
                }
            }
            else if (messageType == MasterServerMessageType.RequestHosts)
            {
                foreach (var hostData in hosts.Values)
                {
                    var response = new OutgoingMessage();
                    response.Write(hostData.EndPoint.ToString());
                    hostData.PublicInfo.Pack(response);

                    response.Buffer.Position = 0;

                    var rawResponse = server.CreateMessage();
                    rawResponse.Write(response.Buffer);
                    server.SendUnconnectedMessage(rawResponse, senderIpEndPoint);
                }

                Console.WriteLine($"List of {hosts.Count} hosts sent to {senderIpEndPoint}.");
            }
            else if (messageType == MasterServerMessageType.RequestIntroduction)
            {
                var hostEndPoint = msg.ReadGuidEndPoint();

                if (hosts.ContainsKey(hostEndPoint))
                {
                    var clientInternalIp = msg.ReadIPEndPoint();
                    var clientExternalIp = senderIpEndPoint;
                    var hostData = hosts[hostEndPoint];

                    // As the client will receive the NatIntroductionSuccess message, send the host's endPoint as token:
                    string token = new IntroducerToken(hostData.EndPoint,
                                                        hostData.ExternalIp,
                                                        clientExternalIp).Serialize();

                    server.Introduce(hostData.InternalIp, hostData.ExternalIp, clientInternalIp, clientExternalIp, token);

                    Console.WriteLine($"Introduced host {hostData} and client [InternalIp: {clientInternalIp}, ExternalIp: {clientExternalIp}].");
                }
                else
                {
                    Console.WriteLine($"Introduction requested for unknwon host from {senderIpEndPoint}.");
                }
            }
        }

        public override void Update()
        {
            if (server == null || server.Status == NetPeerStatus.NotRunning)
            {
                return;
            }

            ReceiveMessages();

            TrimHosts();

            ReportStatus();
        }

        public override void Shutdown()
        {
            if (server == null || server.Status == NetPeerStatus.NotRunning || server.Status == NetPeerStatus.ShutdownRequested)
            {
                return;
            }

            server.Shutdown("Done");

            Console.WriteLine("Master server shut down.");
        }
    }
}
