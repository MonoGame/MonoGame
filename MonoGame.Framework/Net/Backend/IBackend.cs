using Microsoft.Xna.Framework.GamerServices;
using System;
using System.Collections.Generic;
using System.Net;

namespace Microsoft.Xna.Framework.Net.Backend
{
    internal interface IOutgoingMessage
    {
        IPeer Recipient { get; }
        SendDataOptions Options { get; }
        int Channel { get; }
        void Write(IPeer value);
        void Write(IPEndPoint value);
        void Write(bool value);
        void Write(byte value);
        void Write(int value);
        void Write(long value);
        void Write(string value);
        void Write(byte[] value);
    }

    internal interface IIncomingMessage
    {
        IPeer ReadPeer();
        IPEndPoint ReadIPEndPoint();
        bool ReadBoolean();
        byte ReadByte();
        int ReadInt();
        long ReadLong();
        string ReadString();
        void ReadBytes(byte[] into, int offset, int length);
    }

    internal interface IBackendListener
    {
        NetworkSessionType SessionType { get; }
        NetworkSessionProperties SessionProperties { get; }

        int MaxGamers { get; }
        int PrivateGamerSlots { get; }
        int CurrentGamerCount { get; }
        string HostGamertag { get; }
        int OpenPrivateGamerSlots { get; }
        int OpenPublicGamerSlots { get; }

        void PeerConnected(IPeer peer);
        void PeerDisconnected(IPeer peer);
        void ReceiveMessage(IIncomingMessage data, IPeer sender);
    }

    internal interface IPeer
    {
        IPEndPoint EndPoint { get; }
        TimeSpan RoundtripTime { get; }
        object Tag { get; set; }
        void Disconnect(string byeMessage);
    }

    internal interface ISessionBackend
    {
        IPEndPoint HostEndPoint { get; }
        bool HasShutdown { get; }
        IBackendListener Listener { get; set; }
        IPeer LocalPeer { get; }
        TimeSpan SimulatedLatency { get; set; }
        float SimulatedPacketLoss { get; set; }
        int BytesPerSecondReceived { get; set; }
        int BytesPerSecondSent { get; set; }

        void Connect(IPEndPoint endPoint);
        bool IsConnectedToEndPoint(IPEndPoint endPoint);
        IPeer FindRemotePeerByEndPoint(IPEndPoint endPoint);
        
        IOutgoingMessage GetMessage(IPeer recipient, SendDataOptions options, int channel);
        void SendMessage(IOutgoingMessage message);

        void Update();
        void UpdateStatistics();
        void Shutdown(string byeMessage);
    }

    internal interface ISessionCreator
    {
        NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties);
        AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties);
        NetworkSession Join(AvailableNetworkSession availableSession);
    }

    internal interface IMasterServer
    {
        void Start(string appId);
        void Update();
        void Shutdown();
    }

    internal struct DiscoveryContents
    {
        internal NetworkSessionType sessionType;
        internal NetworkSessionProperties sessionProperties;
        internal string hostGamertag;
        internal int maxGamers;
        internal int privateGamerSlots;
        internal int currentGamerCount;
        internal int openPrivateGamerSlots;
        internal int openPublicGamerSlots;

        internal DiscoveryContents(IBackendListener backendListener)
        {
            this.sessionType = backendListener.SessionType;
            this.sessionProperties = backendListener.SessionProperties;
            this.hostGamertag = backendListener.HostGamertag;
            this.maxGamers = backendListener.MaxGamers;
            this.privateGamerSlots = backendListener.PrivateGamerSlots;
            this.currentGamerCount = backendListener.CurrentGamerCount;
            this.openPrivateGamerSlots = backendListener.OpenPrivateGamerSlots;
            this.openPublicGamerSlots = backendListener.OpenPublicGamerSlots;
        }

        internal void Pack(IOutgoingMessage msg)
        {
            msg.Write((byte)sessionType);
            sessionProperties.Pack(msg);
            msg.Write(hostGamertag);
            msg.Write(maxGamers);
            msg.Write(privateGamerSlots);
            msg.Write(currentGamerCount);
            msg.Write(openPrivateGamerSlots);
            msg.Write(openPublicGamerSlots);
        }

        internal void Unpack(IIncomingMessage msg)
        {
            if (sessionProperties == null)
            {
                sessionProperties = new NetworkSessionProperties();
            }

            sessionType = (NetworkSessionType)msg.ReadByte();
            sessionProperties.Unpack(msg);
            hostGamertag = msg.ReadString();
            maxGamers = msg.ReadInt();
            privateGamerSlots = msg.ReadInt();
            currentGamerCount = msg.ReadInt();
            openPrivateGamerSlots = msg.ReadInt();
            openPublicGamerSlots = msg.ReadInt();
        }
    }
}
