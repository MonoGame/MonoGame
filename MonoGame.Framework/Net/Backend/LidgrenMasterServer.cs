using System;
using System.Collections.Generic;
using System.Net;
using System.Diagnostics;

using Lidgren.Network;

namespace Microsoft.Xna.Framework.Net.Backend.Lidgren
{
    internal enum MasterServerMessageType
    {
        RegisterHost,
        UnregisterHost,
        RequestHosts,
        RequestIntroduction
    };

    internal class HostData
    {
        internal Guid guid;
        internal IPEndPoint internalEndPoint;
        internal IPEndPoint externalEndPoint;
        internal NetworkSessionPublicInfo publicInfo;
        internal DateTime lastUpdated;

        public HostData(Guid guid, IPEndPoint internalEndPoint, IPEndPoint externalEndPoint, NetworkSessionPublicInfo publicInfo)
        {
            this.guid = guid;
            this.internalEndPoint = internalEndPoint;
            this.externalEndPoint = externalEndPoint;
            this.publicInfo = publicInfo;
            this.lastUpdated = DateTime.Now;
        }

        public override string ToString()
        {
            return "[Guid: " + guid + ", InternalEP: " + internalEndPoint + ", ExternalEP: " + externalEndPoint + "]";
        }
    }

    internal class LidgrenMasterServer : MasterServer
    {
        private static readonly TimeSpan ReportStatusInterval = TimeSpan.FromSeconds(60.0);

        private NetPeer server;
        private IDictionary<Guid, HostData> hosts = new Dictionary<Guid, HostData>();
        private DateTime lastReportedStatus = DateTime.MinValue;

        public override void Start(string gameAppId)
        {
            NetPeerConfiguration config = new NetPeerConfiguration(gameAppId);
            config.AcceptIncomingConnections = false;
            config.Port = NetworkSessionSettings.MasterServerPort;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            server = new NetPeer(config);
            server.Start();

            Console.WriteLine("Master server with game app id " + gameAppId + " started on port " + config.Port + ".");
        }

        protected IList<Guid> hostsToRemove = new List<Guid>();

        protected void TrimHosts()
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan threshold = NetworkSessionSettings.MasterServerRegistrationInterval + TimeSpan.FromSeconds(5.0);

            hostsToRemove.Clear();

            foreach (var host in hosts)
            {
                if ((currentTime - host.Value.lastUpdated) > threshold)
                {
                    hostsToRemove.Add(host.Key);
                }
            }

            foreach (Guid key in hostsToRemove)
            {
                HostData host = hosts[key];

                hosts.Remove(key);

                Console.WriteLine("Host removed due to timeout. " + host);
            }
        }

        protected void ReportStatus()
        {
            DateTime currentTime = DateTime.Now;

            if (currentTime - lastReportedStatus > ReportStatusInterval)
            {
                Console.WriteLine("Status: " + hosts.Count + " registered hosts");

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
                        LidgrenIncomingMessage msg = new LidgrenIncomingMessage();
                        msg.Buffer = rawMsg;
                        HandleMessage(msg, rawMsg.SenderEndPoint);
                    }
                    catch (NetException e)
                    {
                        Console.WriteLine("Encountered malformed message from " + rawMsg.SenderEndPoint + ". Lidgren reports '" + e.Message + "'.");
                    }
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

                server.Recycle(rawMsg);
            }
        }

        protected void HandleMessage(LidgrenIncomingMessage msg, IPEndPoint senderIPEndPoint)
        {
            string senderGameAppId = msg.ReadString();

            if (!senderGameAppId.Equals(server.Configuration.AppIdentifier, StringComparison.Ordinal))
            {
                Console.WriteLine("Received message with incorrect game app id from " + senderIPEndPoint + ".");
                return;
            }

            MasterServerMessageType messageType = (MasterServerMessageType)msg.ReadByte();

            if (messageType == MasterServerMessageType.RegisterHost)
            {
                Guid hostGuid = Guid.Parse(msg.ReadString());
                IPEndPoint internalEndPoint = msg.ReadIPEndPoint();
                IPEndPoint externalEndPoint = senderIPEndPoint;
                NetworkSessionPublicInfo publicInfo = NetworkSessionPublicInfo.FromMessage(msg);

                hosts[hostGuid] = new HostData(hostGuid, internalEndPoint, externalEndPoint, publicInfo);

                Console.WriteLine("Host registered/updated. " + hosts[hostGuid]);
            }
            else if (messageType == MasterServerMessageType.UnregisterHost)
            {
                Guid hostGuid = Guid.Parse(msg.ReadString());

                if (hosts.ContainsKey(hostGuid))
                {
                    HostData hostData = hosts[hostGuid];

                    hosts.Remove(hostGuid);

                    Console.WriteLine("Host unregistered. " + hostData);
                }
                else
                {
                    Console.WriteLine("Unregister requested from " + senderIPEndPoint + " but host was not found.");
                }
            }
            else if (messageType == MasterServerMessageType.RequestHosts)
            {
                foreach (var elem in hosts)
                {
                    LidgrenOutgoingMessage response = new LidgrenOutgoingMessage();
                    response.Write(elem.Key.ToString());
                    elem.Value.publicInfo.Pack(response);

                    response.Buffer.Position = 0;

                    NetOutgoingMessage rawResponse = server.CreateMessage();
                    rawResponse.Write(response.Buffer);
                    server.SendUnconnectedMessage(rawResponse, senderIPEndPoint);
                }

                Console.WriteLine("List of " + hosts.Count + " hosts sent to " + senderIPEndPoint + ".");
            }
            else if (messageType == MasterServerMessageType.RequestIntroduction)
            {
                Guid hostGuid = Guid.Parse(msg.ReadString());

                if (hosts.ContainsKey(hostGuid))
                {
                    IPEndPoint senderInternalEndPoint = msg.ReadIPEndPoint();
                    IPEndPoint senderExternalEndPoint = senderIPEndPoint;

                    HostData hostData = hosts[hostGuid];

                    string token = hostData.guid.ToString();

                    server.Introduce(hostData.internalEndPoint, hostData.externalEndPoint, senderInternalEndPoint, senderExternalEndPoint, token);

                    Console.WriteLine("Introduced host " + hostData + " and client [InternalEP: " + senderInternalEndPoint + ", ExternalEP: " + senderIPEndPoint + "].");
                }
                else
                {
                    Console.WriteLine("Introduction requested but host was not found.");
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
