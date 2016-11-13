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
        RequestHosts,
        RequestIntroduction
    };

    internal class HostData
    {
        internal IPEndPoint internalEndPoint;
        internal IPEndPoint externalEndPoint;
        internal DiscoveryContents contents;

        public HostData(IPEndPoint internalEndPoint, IPEndPoint externalEndPoint, DiscoveryContents contents)
        {
            this.internalEndPoint = internalEndPoint;
            this.externalEndPoint = externalEndPoint;
            this.contents = contents;
        }
    }

    public class LidgrenMasterServer : IMasterServer
    {
        private NetPeer server;

        private IDictionary<long, HostData> hosts = new Dictionary<long, HostData>();

        public void Start(string appId)
        {
            NetPeerConfiguration config = new NetPeerConfiguration(appId);
            config.AcceptIncomingConnections = false;
            config.Port = NetworkSessionSettings.MasterServerPort;
            config.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            server = new NetPeer(config);
            server.Start();

            Console.WriteLine("Master server started. (AppId: " + appId + ", Port: " + config.Port + ")");
        }

        public void Update()
        {
            if (server == null || server.Status == NetPeerStatus.NotRunning)
            {
                return;
            }

            NetIncomingMessage rawMsg;
            while ((rawMsg = server.ReadMessage()) != null)
            {
                if (rawMsg.MessageType == NetIncomingMessageType.UnconnectedData)
                {
                    // First byte is message type
                    IncomingMessage msg = new IncomingMessage();
                    msg.Buffer = rawMsg;

                    MasterServerMessageType messageType = (MasterServerMessageType)msg.ReadByte();

                    Console.WriteLine("Message of type " + messageType + " received...");

                    if (messageType == MasterServerMessageType.RegisterHost)
                    {
                        long hostId = msg.ReadLong();
                        IPEndPoint internalEndPoint = msg.ReadIPEndPoint();
                        IPEndPoint externalEndPoint = rawMsg.SenderEndPoint;
                        DiscoveryContents contents = new DiscoveryContents();
                        contents.Unpack(msg);

                        hosts[hostId] = new HostData(internalEndPoint, externalEndPoint, contents);

                        Console.WriteLine("Host " + hostId + " added. (Internal endpoint: " + internalEndPoint + ", External endpoint: " + externalEndPoint + ")");
                    }
                    else if (messageType == MasterServerMessageType.RequestHosts)
                    {
                        foreach (var elem in hosts)
                        {
                            OutgoingMessage response = new OutgoingMessage();
                            response.Write(elem.Key);
                            response.Write(elem.Value.externalEndPoint);
                            elem.Value.contents.Pack(response);

                            response.Buffer.Position = 0;

                            NetOutgoingMessage rawResponse = server.CreateMessage();
                            rawResponse.Write(response.Buffer);
                            server.SendUnconnectedMessage(rawResponse, rawMsg.SenderEndPoint);
                        }

                        Console.WriteLine("List of " + hosts.Count + " hosts sent to " + rawMsg.SenderEndPoint + ".");
                    }
                    else if (messageType == MasterServerMessageType.RequestIntroduction)
                    {
                        IPEndPoint senderInternalEndPoint = msg.ReadIPEndPoint();
                        long hostId = msg.ReadLong();

                        if (hosts.ContainsKey(hostId))
                        {
                            HostData hostData = hosts[hostId];

                            server.Introduce(hostData.internalEndPoint, hostData.externalEndPoint, senderInternalEndPoint, rawMsg.SenderEndPoint, string.Empty);

                            Console.WriteLine("Introduced host (" + hostData.internalEndPoint + ", " + hostData.externalEndPoint + ") with client (" + senderInternalEndPoint + ", " + rawMsg.SenderEndPoint + ").");
                        }
                        else
                        {
                            Console.WriteLine("Introduction requested but host was not found.");
                        }
                    }
                }

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

                server.Recycle(rawMsg);
            }
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
