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
        bool AllowConnect { get; }
        bool RegisterWithMasterServer { get; }
        NetworkSessionPublicInfo SessionPublicInfo { get; }

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
}
