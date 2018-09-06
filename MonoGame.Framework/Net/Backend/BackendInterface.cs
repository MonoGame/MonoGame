using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net.Backend
{
    /**
     * Interfaces would be ideal here, but abstract classes provide better performance in practice.
     */
    internal abstract class BasePeerEndPoint : IEquatable<BasePeerEndPoint>
    {
        public abstract bool Equals(BasePeerEndPoint other);
    }

    internal abstract class BaseOutgoingMessage
    {
        public abstract Peer Recipient { get; }
        public abstract SendDataOptions Options { get; }
        public abstract int Channel { get; }

        public abstract void Write(Peer value);
        public abstract void Write(BasePeerEndPoint value);
        public abstract void Write(bool value);
        public abstract void Write(byte value);
        public abstract void Write(int value);
        public abstract void Write(long value);
        public abstract void Write(string value);
        public abstract void Write(byte[] value);
    }

    internal abstract class BaseIncomingMessage
    {
        public abstract Peer ReadPeer();
        public abstract BasePeerEndPoint ReadPeerEndPoint();
        public abstract bool ReadBoolean();
        public abstract byte ReadByte();
        public abstract int ReadInt();
        public abstract long ReadLong();
        public abstract string ReadString();
        public abstract void ReadBytes(byte[] into, int offset, int length);
    }

    internal abstract class Peer
    {
        public abstract BasePeerEndPoint EndPoint { get; }
        public abstract TimeSpan RoundtripTime { get; }
        public abstract object Tag { get; set; }

        public abstract void Disconnect(string byeMessage);
    }

    internal interface ISessionBackendListener
    {
        bool IsDiscoverableLocally { get; }
        bool IsDiscoverableOnline { get; }
        NetworkSessionPublicInfo SessionPublicInfo { get; }

        bool AllowConnectionFromClient(BasePeerEndPoint endPoint);
        bool AllowConnectionToHostAsClient(BasePeerEndPoint targetEndPoint);
        void PeerConnected(Peer peer);
        void PeerDisconnected(Peer peer);
        void ReceiveMessage(BaseIncomingMessage data, Peer sender);
    }

    internal abstract class BaseSessionBackend
    {
        public abstract bool HasShutdown { get; }
        public abstract ISessionBackendListener Listener { get; set; }
        public abstract Peer LocalPeer { get; }
        public abstract TimeSpan SimulatedLatency { get; set; }
        public abstract float SimulatedPacketLoss { get; set; }
        public abstract int BytesPerSecondReceived { get; set; }
        public abstract int BytesPerSecondSent { get; set; }

        public abstract void Introduce(Peer client, Peer target);
        public abstract bool IsConnectedToEndPoint(BasePeerEndPoint endPoint);
        public abstract Peer FindRemotePeerByEndPoint(BasePeerEndPoint endPoint);
        public abstract BaseOutgoingMessage GetMessage(Peer recipient, SendDataOptions options, int channel);
        public abstract void SendMessage(BaseOutgoingMessage message);
        public abstract void Update();
        public abstract void Shutdown(string byeMessage);
    }

    internal abstract class BaseSessionCreator
    {
        public abstract NetworkSession Create(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, int maxGamers, int privateGamerSlots, NetworkSessionProperties sessionProperties);
        public abstract AvailableNetworkSessionCollection Find(NetworkSessionType sessionType, IEnumerable<SignedInGamer> localGamers, NetworkSessionProperties searchProperties);
        public abstract NetworkSession Join(AvailableNetworkSession availableSession);
    }

    internal abstract class BaseMasterServer
    {
        public abstract void Start(string appId);
        public abstract void Update();
        public abstract void Shutdown();
    }
}
