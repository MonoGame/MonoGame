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
        internal long id;
        internal IPEndPoint internalEndPoint;
        internal IPEndPoint externalEndPoint;
        internal NetworkSessionPublicInfo publicInfo;
        internal DateTime lastUpdated;

        public HostData(long id, IPEndPoint internalEndPoint, IPEndPoint externalEndPoint, NetworkSessionPublicInfo publicInfo)
        {
            this.id = id;
            this.internalEndPoint = internalEndPoint;
            this.externalEndPoint = externalEndPoint;
            this.publicInfo = publicInfo;
            this.lastUpdated = DateTime.Now;
        }

        public override string ToString()
        {
            return "[Id: " + id + ", InternalEP: " + internalEndPoint + ", ExternalEP: " + externalEndPoint + "]";
        }
    }

    public class LidgrenMasterServer : IMasterServer
    {
        private static readonly TimeSpan ReportStatusInterval = TimeSpan.FromSeconds(60.0);

        private NetPeer server;
        private IDictionary<long, HostData> hosts = new Dictionary<long, HostData>();
        private DateTime lastReportedStatus = DateTime.MinValue;

        public void Start(string gameAppId)
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

        protected IList<long> hostsToRemove = new List<long>();

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

            foreach (long key in hostsToRemove)
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
                        HandleMessage(rawMsg);
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

        protected void HandleMessage(NetIncomingMessage rawMsg)
        {
            IncomingMessage msg = new IncomingMessage();
            msg.Buffer = rawMsg;

            string senderGameAppId = msg.ReadString();

            if (senderGameAppId != server.Configuration.AppIdentifier)
            {
                Console.WriteLine("Received message with incorrect game app id from " + rawMsg.SenderEndPoint + ".");
                return;
            }

            MasterServerMessageType messageType = (MasterServerMessageType)msg.ReadByte();

            if (messageType == MasterServerMessageType.RegisterHost)
            {
                long hostId = msg.ReadLong();
                IPEndPoint internalEndPoint = (msg.ReadPeerEndPoint() as LidgrenEndPoint).endPoint;
                IPEndPoint externalEndPoint = rawMsg.SenderEndPoint;
                NetworkSessionPublicInfo publicInfo = NetworkSessionPublicInfo.FromMessage(msg);

                hosts[hostId] = new HostData(hostId, internalEndPoint, externalEndPoint, publicInfo);

                Console.WriteLine("Host registered/updated. " + hosts[hostId]);
            }
            else if (messageType == MasterServerMessageType.UnregisterHost)
            {
                long hostId = msg.ReadLong();

                if (hosts.ContainsKey(hostId))
                {
                    HostData hostData = hosts[hostId];

                    hosts.Remove(hostId);

                    Console.WriteLine("Host unregistered. " + hostData);
                }
                else
                {
                    Console.WriteLine("Unregister requested from " + rawMsg.SenderEndPoint + " but host was not found.");
                }
            }
            else if (messageType == MasterServerMessageType.RequestHosts)
            {
                foreach (var elem in hosts)
                {
                    OutgoingMessage response = new OutgoingMessage();
                    response.Write(elem.Key);
                    response.Write(new LidgrenEndPoint(elem.Value.externalEndPoint));
                    elem.Value.publicInfo.Pack(response);

                    response.Buffer.Position = 0;

                    NetOutgoingMessage rawResponse = server.CreateMessage();
                    rawResponse.Write(response.Buffer);
                    server.SendUnconnectedMessage(rawResponse, rawMsg.SenderEndPoint);
                }

                Console.WriteLine("List of " + hosts.Count + " hosts sent to " + rawMsg.SenderEndPoint + ".");
            }
            else if (messageType == MasterServerMessageType.RequestIntroduction)
            {
                IPEndPoint senderInternalEndPoint = (msg.ReadPeerEndPoint() as LidgrenEndPoint).endPoint;
                long hostId = msg.ReadLong();

                if (hosts.ContainsKey(hostId))
                {
                    HostData hostData = hosts[hostId];

                    server.Introduce(hostData.internalEndPoint, hostData.externalEndPoint, senderInternalEndPoint, rawMsg.SenderEndPoint, string.Empty);

                    Console.WriteLine("Introduced host " + hostData + " and client [InternalEP: " + senderInternalEndPoint + ", ExternalEP: " + rawMsg.SenderEndPoint + "].");
                }
                else
                {
                    Console.WriteLine("Introduction requested but host was not found.");
                }
            }
        }

        public void Update()
        {
            if (server == null || server.Status == NetPeerStatus.NotRunning)
            {
                return;
            }

            ReceiveMessages();

            TrimHosts();

            ReportStatus();
        }

        public void Shutdown()
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
