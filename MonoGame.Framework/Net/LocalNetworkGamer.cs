using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net.Message;

namespace Microsoft.Xna.Framework.Net
{
    internal struct InboundPacket
    {
        public Packet packet;
        public NetworkGamer sender;

        public InboundPacket(Packet packet, NetworkGamer sender)
        {
            this.packet = packet;
            this.sender = sender;
        }
    }

    internal struct OutboundPacket
    {
        public Packet packet;
        public LocalNetworkGamer sender;
        public NetworkGamer recipient;
        public SendDataOptions options;

        public OutboundPacket(Packet packet, LocalNetworkGamer sender, NetworkGamer recipient, SendDataOptions options)
        {
            this.packet = packet;
            this.sender = sender;
            this.recipient = recipient;
            this.options = options;
        }
    }

    public sealed class LocalNetworkGamer : NetworkGamer
    {
        private int inboundPacketIndex = 0;
        private List<InboundPacket> inboundPackets = new List<InboundPacket>();
        private List<OutboundPacket> outboundPackets = new List<OutboundPacket>();

        internal LocalNetworkGamer(byte id, bool isGuest, bool isHost, bool isPrivateSlot, NetworkSession session, SignedInGamer signedInGamer) : base(signedInGamer.DisplayName, signedInGamer.Gamertag, id, isGuest, isHost, true, isPrivateSlot, session.machine, session)
        {
            this.SignedInGamer = signedInGamer;
        }

        internal IList<InboundPacket> InboundPackets { get { return inboundPackets; } }
        internal IList<OutboundPacket> OutboundPackets { get { return outboundPackets; } }

        public bool IsDataAvailable { get { return inboundPacketIndex < inboundPackets.Count; } }

        public override bool IsReady
        {
            set
            {
                if (IsDisposed)
                {
                    throw new InvalidOperationException("Gamer disposed");
                }

                if (Session.SessionState != NetworkSessionState.Lobby)
                {
                    throw new InvalidOperationException("Session state is not lobby");
                }

                if (isReady != value)
                {
                    isReady = value;

                    Session.Send(new GamerStateChangeMessageSender(this, false, true));
                }
            }
        }

        public SignedInGamer SignedInGamer { get; }

        public void EnableSendVoice(NetworkGamer remoteGamer, bool enable)
        {
            throw new NotImplementedException();
        }

        internal void RecycleInboundPackets()
        {
            for (int i = 0; i < inboundPacketIndex; i++)
            {
                Session.packetPool.RecyclePacket(inboundPackets[i].packet);
            }

            if (inboundPacketIndex > 0)
            {
                inboundPackets.RemoveRange(0, inboundPacketIndex);
            }

            inboundPacketIndex = 0;
        }

        internal void RecycleOutboundPackets()
        {
            foreach (OutboundPacket outboundPacket in outboundPackets)
            {
                Session.packetPool.RecyclePacket(outboundPacket.packet);
            }

            outboundPackets.Clear();
        }

        // Receiving data
        public int ReceiveData(byte[] data, out NetworkGamer sender)
        {
            throw new NotImplementedException();
        }

        public int ReceiveData(byte[] data, int offset, out NetworkGamer sender)
        {
            throw new NotImplementedException();
        }

        public int ReceiveData(PacketReader data, out NetworkGamer sender)
        {
            if (inboundPacketIndex >= inboundPackets.Count)
            {
                sender = null;
                return 0;
            }

            // Get one packet from queue
            InboundPacket inboundPacket = inboundPackets[inboundPacketIndex];
            inboundPacketIndex++;

            // Write inbound packet data to stream
            data.BaseStream.SetLength(0);
            data.BaseStream.Write(inboundPacket.packet.data, 0, inboundPacket.packet.length);
            data.BaseStream.Position = 0;

            sender = inboundPacket.sender;
            return inboundPacket.packet.length;
        }

        // Sending data
        public void SendData(byte[] data, SendDataOptions options)
        {
            throw new NotImplementedException();
        }

        public void SendData(byte[] data, SendDataOptions options, NetworkGamer recipient)
        {
            throw new NotImplementedException();
        }

        public void SendData(byte[] data, int offset, int count, SendDataOptions options)
        {
            throw new NotImplementedException();
        }

        public void SendData(byte[] data, int offset, int count, SendDataOptions options, NetworkGamer recipient)
        {
            throw new NotImplementedException();
        }

        private void InternalSendData(PacketWriter data, SendDataOptions options, NetworkGamer recipient)
        {
            if (data == null)
            {
                throw new NullReferenceException("data");
            }
            if (data.Length == 0)
            {
                throw new NetworkException("PacketWriter empty");
            }
            if (Session == null)
            {
                throw new ObjectDisposedException("NetworkSession");
            }

            // Write stream contents to an outbound packet
            Packet packet = Session.packetPool.GetPacket(data.Length);
            data.BaseStream.Position = 0;
            data.BaseStream.Read(packet.data, 0, packet.length);

            // Reset stream
            data.BaseStream.SetLength(0);

            outboundPackets.Add(new OutboundPacket(packet, this, recipient, options));
        }

        public void SendData(PacketWriter data, SendDataOptions options)
        {
            InternalSendData(data, options, null);
        }

        public void SendData(PacketWriter data, SendDataOptions options, NetworkGamer recipient)
        {
            if (recipient == null)
            {
                throw new NullReferenceException("recipient");
            }

            InternalSendData(data, options, recipient);
        }

        public void SendPartyInvites()
        { }
    }
}