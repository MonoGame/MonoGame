using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net.Backend
{
    internal interface IPeerEndPoint : IEquatable<IPeerEndPoint>
    { }

    internal interface IOutgoingMessage
    {
        IPeer Recipient { get; }
        SendDataOptions Options { get; }
        int Channel { get; }
        void Write(IPeer value);
        void Write(IPeerEndPoint value);
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
        IPeerEndPoint ReadPeerEndPoint();
        bool ReadBoolean();
        byte ReadByte();
        int ReadInt();
        long ReadLong();
        string ReadString();
        void ReadBytes(byte[] into, int offset, int length);
    }

    internal interface IBackendListener
    {
        bool IsDiscoverableAsHost { get; }
        NetworkSessionPublicInfo SessionPublicInfo { get; }

        bool AllowConnectionFromClient(IPeerEndPoint endPoint);
        void IntroducedAsClient(IPeerEndPoint targetEndPoint, string providedToken);
        void PeerConnected(IPeer peer);
        void PeerDisconnected(IPeer peer);
        void ReceiveMessage(IIncomingMessage data, IPeer sender);
    }

    internal interface IPeer
    {
        IPeerEndPoint EndPoint { get; }
        TimeSpan RoundtripTime { get; }
        object Tag { get; set; }
        void Disconnect(string byeMessage);
    }

    internal interface ISessionBackend
    {
        bool HasShutdown { get; }
        IBackendListener Listener { get; set; }
        IPeer LocalPeer { get; }
        TimeSpan SimulatedLatency { get; set; }
        float SimulatedPacketLoss { get; set; }
        int BytesPerSecondReceived { get; set; }
        int BytesPerSecondSent { get; set; }

        void Introduce(IPeer client, IPeer target);
        void Connect(IPeerEndPoint endPoint);
        bool IsConnectedToEndPoint(IPeerEndPoint endPoint);
        IPeer FindRemotePeerByEndPoint(IPeerEndPoint endPoint);
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
