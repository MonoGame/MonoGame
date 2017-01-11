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
        internal LidgrenEndPoint endPoint;
        internal IPEndPoint intIPEndPoint;
        internal IPEndPoint extIPEndPoint;
        internal NetworkSessionPublicInfo publicInfo;
        internal DateTime lastUpdated;

        public HostData(LidgrenEndPoint endPoint, IPEndPoint intIPEndPoint, IPEndPoint extIPEndPoint, NetworkSessionPublicInfo publicInfo)
        {
            this.endPoint = endPoint;
            this.intIPEndPoint = intIPEndPoint;
            this.extIPEndPoint = extIPEndPoint;
            this.publicInfo = publicInfo;
            this.lastUpdated = DateTime.Now;
        }

        public override string ToString()
        {
            return "[EP: " + endPoint + ", Int: " + intIPEndPoint + ", Ext: " + extIPEndPoint + "]";
        }
    }

    internal class LidgrenMasterServer : MasterServer
    {
        private static readonly TimeSpan ReportStatusInterval = TimeSpan.FromSeconds(60.0);

        private NetPeer server;
        private IDictionary<LidgrenEndPoint, HostData> hosts = new Dictionary<LidgrenEndPoint, HostData>();
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

        private IList<LidgrenEndPoint> hostsToRemove = new List<LidgrenEndPoint>();

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

            foreach (LidgrenEndPoint key in hostsToRemove)
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
                Console.WriteLine("Status: " + hosts.Count + " registered hosts.");

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
                LidgrenEndPoint hostEndPoint = LidgrenEndPoint.Parse(msg.ReadString());
                IPEndPoint intIPEndPoint = msg.ReadIPEndPoint();
                IPEndPoint extIPEndPoint = senderIPEndPoint;
                NetworkSessionPublicInfo publicInfo = NetworkSessionPublicInfo.FromMessage(msg);

                hosts[hostEndPoint] = new HostData(hostEndPoint, intIPEndPoint, extIPEndPoint, publicInfo);

                Console.WriteLine("Host registered/updated. " + hosts[hostEndPoint]);
            }
            else if (messageType == MasterServerMessageType.UnregisterHost)
            {
                LidgrenEndPoint hostEndPoint = LidgrenEndPoint.Parse(msg.ReadString());

                if (hosts.ContainsKey(hostEndPoint))
                {
                    HostData hostData = hosts[hostEndPoint];

                    hosts.Remove(hostEndPoint);

                    Console.WriteLine("Host unregistered. " + hostData);
                }
                else
                {
                    Console.WriteLine("Unregister requested from " + senderIPEndPoint + " but host was not found.");
                }
            }
            else if (messageType == MasterServerMessageType.RequestHosts)
            {
                foreach (HostData hostData in hosts.Values)
                {
                    LidgrenOutgoingMessage response = new LidgrenOutgoingMessage();
                    response.Write(hostData.endPoint.ToString());
                    hostData.publicInfo.Pack(response);

                    response.Buffer.Position = 0;

                    NetOutgoingMessage rawResponse = server.CreateMessage();
                    rawResponse.Write(response.Buffer);
                    server.SendUnconnectedMessage(rawResponse, senderIPEndPoint);
                }

                Console.WriteLine("List of " + hosts.Count + " hosts sent to " + senderIPEndPoint + ".");
            }
            else if (messageType == MasterServerMessageType.RequestIntroduction)
            {
                LidgrenEndPoint hostEndPoint = LidgrenEndPoint.Parse(msg.ReadString());

                if (hosts.ContainsKey(hostEndPoint))
                {
                    IPEndPoint senderIntIPEndPoint = msg.ReadIPEndPoint();
                    IPEndPoint senderExtIPEndPoint = senderIPEndPoint;
                    HostData hostData = hosts[hostEndPoint];

                    // As the client will receive the NatIntroductionSuccess message, send the host's endPoint as token:
                    string token = hostData.endPoint.ToString();
                    server.Introduce(hostData.intIPEndPoint, hostData.extIPEndPoint, senderIntIPEndPoint, senderExtIPEndPoint, token);

                    Console.WriteLine("Introduced host " + hostData + " and client [Int: " + senderIntIPEndPoint + ", Ext: " + senderIPEndPoint + "].");
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
