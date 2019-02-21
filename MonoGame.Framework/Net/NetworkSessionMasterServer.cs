using System;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;
using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net
{
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
            return "[Guid: " + Guid + ", InternalIp: " + InternalIp + ", ExternalIp: " + ExternalIp + "]";
        }
    }

    public abstract partial class NetworkSessionMasterServer
    {
        private static readonly TimeSpan ReportStatusInterval = TimeSpan.FromSeconds(60.0);

        private NetPeer serverPeer;
        private IDictionary<Guid, HostData> hosts = new Dictionary<Guid, HostData>();
        private DateTime lastReportedStatus = DateTime.MinValue;

        public void Start()
        {
            var config = new NetPeerConfiguration(GameAppId)
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

            Console.WriteLine("Master server with game app id " + GameAppId + " started on port " + config.Port + ".");
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

                Console.WriteLine("Host removed due to timeout. " + host);
            }
        }

        protected void ReportStatus()
        {
            var currentTime = DateTime.Now;
            if (currentTime - lastReportedStatus > ReportStatusInterval)
            {
                Console.WriteLine("Status: " + hosts.Count + " registered hosts.");

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
                    if (!HandleMessage(msg))
                    {
                        Console.WriteLine("Encountered malformed message from " + msg.SenderEndPoint + ".");
                    }
                }
                else
                {
                    NetworkSession.HandleLidgrenMessage(msg);
                }
                serverPeer.Recycle(msg);
            }
        }

        protected bool HandleMessage(NetIncomingMessage msg)
        {
            string senderGameAppId;
            string senderPayload;
            try
            {
                senderGameAppId = msg.ReadString();
                senderPayload = msg.ReadString();
            }
            catch
            {
                return false;
            }
            if (!senderGameAppId.Equals(GameAppId, StringComparison.InvariantCulture))
            {
                Console.WriteLine("Received message with incorrect game app id from " + msg.SenderEndPoint + ".");
                return true;
            }
            var payloadValid = ValidatePayload(senderPayload);
            var messageType = (MasterServerMessageType)msg.ReadByte();

            if (messageType == MasterServerMessageType.RequestGeneralInfo)
            {
                // Note: Payload does not need to be valid to request general info (useful to handle new version alerts)
                SendRequestGeneralInfoResponse(serverPeer, msg.SenderEndPoint, GeneralInfo);

                Console.WriteLine("Sent general info to " + msg.SenderEndPoint + ".");
                return true;
            }

            if (!payloadValid)
            {
                SendErrorResponse(serverPeer, msg.SenderEndPoint, messageType, MasterServerMessageResult.InvalidPayload);

                Console.WriteLine("Received message that failed payload validation from " + msg.SenderEndPoint + ", error response sent.");
                return true;
            }

            if (messageType == MasterServerMessageType.RegisterHost)
            {
                Guid guid;
                IPEndPoint internalIp, externalIp;
                NetworkSessionPublicInfo publicInfo;
                if (!ParseRegisterHost(msg, out guid, out internalIp, out externalIp, out publicInfo))
                {
                    return false;
                }

                hosts[guid] = new HostData(guid, internalIp, externalIp, publicInfo);

                Console.WriteLine("Host registered/updated. " + hosts[guid]);
            }
            else if (messageType == MasterServerMessageType.UnregisterHost)
            {
                Guid guid;
                if (!ParseUnregisterHost(msg, out guid))
                {
                    return false;
                }

                if (hosts.ContainsKey(guid))
                {
                    var host = hosts[guid];
                    if (msg.SenderEndPoint.Equals(host.ExternalIp))
                    {
                        hosts.Remove(guid);

                        Console.WriteLine("Host unregistered. " + host);
                    }
                    else
                    {
                        Console.WriteLine("Unregister requested for host not registered by " + msg.SenderEndPoint + ".");
                    }
                }
                else
                {
                    Console.WriteLine("Unregister requested for unknown host from " + msg.SenderEndPoint + ".");
                }
            }
            else if (messageType == MasterServerMessageType.RequestHosts)
            {
                foreach (var host in hosts.Values)
                {
                    SendRequestHostsResponse(serverPeer, msg.SenderEndPoint, false, host.Guid, host.PublicInfo);
                }

                Console.WriteLine("List of " + hosts.Count + " hosts sent to " + msg.SenderEndPoint + ".");
            }
            else if (messageType == MasterServerMessageType.RequestIntroduction)
            {
                Guid guid;
                IPEndPoint clientInternalIp, clientExternalIp;
                if (!ParseRequestIntroduction(msg, out guid, out clientInternalIp, out clientExternalIp))
                {
                    return false;
                }

                if (hosts.ContainsKey(guid))
                {
                    var host = hosts[guid];
                    serverPeer.Introduce(host.InternalIp, host.ExternalIp, clientInternalIp, clientExternalIp, string.Empty);
                    Console.WriteLine("Introduced host " + host + " and client [InternalIp: " + clientInternalIp + ", ExternalIp: " + clientExternalIp + "].");
                }
                else
                {
                    Console.WriteLine("Introduction requested for unknwon host from " + msg.SenderEndPoint + ".");
                }
            }

            return true;
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

        public abstract string GameAppId { get; }
        public abstract string GeneralInfo { get; }
        public abstract bool ValidatePayload(string payload);
    }
}
