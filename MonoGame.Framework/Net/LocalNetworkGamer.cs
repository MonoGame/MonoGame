using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

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
        private const int MaxDelayedPacketsAllowed = 100;

        private Dictionary<byte, List<Packet>> delayedUnordered = new Dictionary<byte, List<Packet>>();
        private Dictionary<byte, List<Packet>> delayedOrdered = new Dictionary<byte, List<Packet>>();

        private int inboundPacketIndex = 0;
        private List<InboundPacket> inboundPackets = new List<InboundPacket>();

        internal LocalNetworkGamer(SignedInGamer signedInGamer, NetworkMachine machine, byte id, bool isPrivateSlot)
            : base(machine, id, isPrivateSlot, false, signedInGamer.DisplayName, signedInGamer.Gamertag)
        {
            this.SignedInGamer = signedInGamer;
        }
        
        public bool IsDataAvailable { get { return inboundPacketIndex < inboundPackets.Count; } }

        public override bool IsReady
        {
            set
            {
                if (IsDisposed) throw new InvalidOperationException(nameof(LocalNetworkGamer));

                if (session.SessionState != NetworkSessionState.Lobby)
                {
                    throw new InvalidOperationException("Session state is not lobby");
                }

                if (isReady != value)
                {
                    isReady = value;

                    session.SendGamerStateChanged(this);
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
                session.packetPool.Recycle(inboundPackets[i].packet);
            }

            if (inboundPacketIndex > 0)
            {
                inboundPackets.RemoveRange(0, inboundPacketIndex);
            }

            inboundPacketIndex = 0;
        }

        internal bool AddInboundPacket(Packet packet, byte senderId, SendDataOptions options)
        {
            var sender = session.FindGamerById(senderId);

            if (options == SendDataOptions.InOrder || options == SendDataOptions.ReliableInOrder)
            {
                bool packetsInQueue = delayedOrdered.ContainsKey(senderId) && delayedOrdered[senderId].Count > 0;

                if (sender == null || packetsInQueue)
                {
                    if (!delayedOrdered.ContainsKey(senderId))
                    {
                        delayedOrdered.Add(senderId, new List<Packet>());
                    }
                    if (delayedOrdered[senderId].Count > MaxDelayedPacketsAllowed)
                    {
                        return false;
                    }
                    delayedOrdered[senderId].Add(packet);
                }
                else
                {
                    inboundPackets.Add(new InboundPacket(packet, sender));
                }
            }
            else
            {
                if (sender == null)
                {
                    if (!delayedUnordered.ContainsKey(senderId))
                    {
                        delayedUnordered.Add(senderId, new List<Packet>());
                    }
                    if (delayedUnordered.Count > MaxDelayedPacketsAllowed)
                    {
                        return false;
                    }
                    delayedUnordered[senderId].Add(packet);
                }
                else
                {
                    inboundPackets.Add(new InboundPacket(packet, sender));
                }
            }

            return true;
        }

        internal void TryAddDelayedInboundPackets()
        {
            // Unordered
            foreach (var pair in delayedUnordered)
            {
                var sender = session.FindGamerById(pair.Key);
                var delayedPackets = pair.Value;

                if (sender != null)
                {
                    foreach (var delayedPacket in delayedPackets)
                    {
                        inboundPackets.Add(new InboundPacket(delayedPacket, sender));
                    }
                    delayedPackets.Clear();
                }
            }
            // Ordered
            foreach (var pair in delayedOrdered)
            {
                var sender = session.FindGamerById(pair.Key);
                var delayedPackets = pair.Value;

                if (sender != null)
                {
                    foreach (var delayedPacket in delayedPackets)
                    {
                        inboundPackets.Add(new InboundPacket(delayedPacket, sender));
                    }
                    delayedPackets.Clear();
                }
            }
        }

        // Receiving data
        public int ReceiveData(byte[] data, out NetworkGamer sender)
        {
            try { return ReceiveData(data, 0, out sender); }
            catch { throw; }
        }

        public int ReceiveData(byte[] data, int offset, out NetworkGamer sender)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (offset >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if (inboundPacketIndex >= inboundPackets.Count)
            {
                sender = null;
                return 0;
            }

            var inboundPacket = inboundPackets[inboundPacketIndex++];
            if (inboundPacket.packet.length > data.Length - offset)
            {
                throw new ArgumentException($"{nameof(data)} is too small to accommodate the incoming network packet");
            }

            inboundPacket.packet.data.CopyTo(data, offset);
            sender = inboundPacket.sender;
            return inboundPacket.packet.length;
        }

        public int ReceiveData(PacketReader data, out NetworkGamer sender)
        {
            if (inboundPacketIndex >= inboundPackets.Count)
            {
                sender = null;
                return 0;
            }

            var inboundPacket = inboundPackets[inboundPacketIndex++];

            data.BaseStream.SetLength(0);
            data.BaseStream.Position = 0;
            data.BaseStream.Write(inboundPacket.packet.data, 0, inboundPacket.packet.length);
            data.BaseStream.Position = 0;

            sender = inboundPacket.sender;
            return inboundPacket.packet.length;
        }

        // Sending data
        public void SendData(byte[] data, SendDataOptions options)
        {
            try { InternalSendData(data, 0, data.Length, options, null); }
            catch { throw; }
        }

        public void SendData(byte[] data, SendDataOptions options, NetworkGamer recipient)
        {
            if (recipient == null)
            {
                throw new NullReferenceException(nameof(recipient));
            }

            try { InternalSendData(data, 0, data.Length, options, recipient); }
            catch { throw; }
        }

        public void SendData(byte[] data, int offset, int count, SendDataOptions options)
        {
            try { InternalSendData(data, offset, count, options, null); }
            catch { throw; }
        }

        public void SendData(byte[] data, int offset, int count, SendDataOptions options, NetworkGamer recipient)
        {
            if (recipient == null)
            {
                throw new NullReferenceException(nameof(recipient));
            }

            try { InternalSendData(data, offset, count, options, recipient); }
            catch { throw; }
        }

        private void InternalSendData(byte[] data, int offset, int count, SendDataOptions options, NetworkGamer recipient)
        {
            if (data == null)
            {
                throw new NullReferenceException(nameof(data));
            }
            if (data.Length == 0)
            {
                throw new NetworkException($"{nameof(data)} empty");
            }
            if (offset < 0 || offset >= data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }
            if (count <= 0 || offset + count > data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            session.SendUserMessage(this, options, data, recipient);
        }

        private void InternalSendData(PacketWriter data, SendDataOptions options, NetworkGamer recipient)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.Length == 0)
            {
                throw new NetworkException("PacketWriter empty");
            }

            session.SendUserMessage(this, options, data, recipient);
            
            data.BaseStream.SetLength(0);
            data.BaseStream.Position = 0;
        }

        public void SendData(PacketWriter data, SendDataOptions options)
        {
            try { InternalSendData(data, options, null); }
            catch { throw; }
        }

        public void SendData(PacketWriter data, SendDataOptions options, NetworkGamer recipient)
        {
            if (recipient == null)
            {
                throw new NullReferenceException(nameof(recipient));
            }

            try { InternalSendData(data, options, recipient); }
            catch { throw; }
        }

        public void SendPartyInvites()
        {
            throw new NotImplementedException();
        }
    }
}
