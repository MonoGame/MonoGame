using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net.Backend
{
    internal abstract class PeerEndPoint : IEquatable<PeerEndPoint>
    {
        public abstract bool Equals(PeerEndPoint other);
    }

    internal abstract class OutgoingMessage
    {
        public abstract Peer Recipient { get; }
        public abstract SendDataOptions Options { get; }
        public abstract int Channel { get; }

        public abstract void Write(Peer value);
        public abstract void Write(PeerEndPoint value);
        public abstract void Write(bool value);
        public abstract void Write(byte value);
        public abstract void Write(int value);
        public abstract void Write(long value);
        public abstract void Write(string value);
        public abstract void Write(byte[] value);
    }

    internal abstract class IncomingMessage
    {
        public abstract Peer ReadPeer();
        public abstract PeerEndPoint ReadPeerEndPoint();
        public abstract bool ReadBoolean();
        public abstract byte ReadByte();
        public abstract int ReadInt();
        public abstract long ReadLong();
        public abstract string ReadString();
        public abstract void ReadBytes(byte[] into, int offset, int length);
    }

    internal abstract class Peer
    {
        public abstract PeerEndPoint EndPoint { get; }
        public abstract TimeSpan RoundtripTime { get; }
        public abstract object Tag { get; set; }

        public abstract void Disconnect(string byeMessage);
    }

    internal interface ISessionBackendListener
    {
        bool IsDiscoverableLocally { get; }
        bool IsDiscoverableOnline { get; }
        NetworkSessionPublicInfo SessionPublicInfo { get; }

        bool AllowConnectionFromClient(PeerEndPoint endPoint);
        bool AllowConnectWhenIntroducedAsClient(PeerEndPoint targetEndPoint);
        void PeerConnected(Peer peer);
        void PeerDisconnected(Peer peer);
        void ReceiveMessage(IncomingMessage data, Peer sender);
    }

    internal abstract class SessionBackend
    {
        public abstract bool HasShutdown { get; }
        public abstract ISessionBackendListener Listener { get; set; }
        public abstract Peer LocalPeer { get; }
        public abstract TimeSpan SimulatedLatency { get; set; }
        public abstract float SimulatedPacketLoss { get; set; }
        public abstract int BytesPerSecondReceived { get; set; }
        public abstract int BytesPerSecondSent { get; set; }

        public abstract void Introduce(Peer client, Peer target);
        public abstract bool IsConnectedToEndPoint(PeerEndPoint endPoint);
        public abstract Peer FindRemotePeerByEndPoint(PeerEndPoint endPoint);
        public abstract OutgoingMessage GetMessage(Peer recipient, SendDataOptions options, int channel);
        public abstract void SendMessage(OutgoingMessage message);
        public abstract void Update();
        public abstract void Shutdown(string byeMessage);
    }

    internal abstract class SessionCreator
    {
        public abstract NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties);
        public abstract AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties);
        public abstract NetworkSession Join(AvailableNetworkSession availableSession);
    }

    internal abstract class MasterServer
    {
        public abstract void Start(string appId);
        public abstract void Update();
        public abstract void Shutdown();
    }
}
