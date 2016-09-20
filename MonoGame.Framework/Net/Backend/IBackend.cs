using System;
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
        string ReadString();
        void ReadBytes(byte[] into, int offset, int length);
    }
    internal interface IBackendListener
    {
        NetworkSessionType SessionType { get; }
        NetworkSessionProperties SessionProperties { get; }

        bool ShouldSendDiscoveryResponse { get; }
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
    internal interface IBackend
    {
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
        
        void SendMessage(IOutgoingMessage data);
        void Update();
        void UpdateStatistics();
        void Shutdown(string byeMessage);
    }
}
