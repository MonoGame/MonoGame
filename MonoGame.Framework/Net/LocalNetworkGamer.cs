using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Net.Messages;

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
        private IDictionary<byte, IList<Packet>> delayedUnordered = new Dictionary<byte, IList<Packet>>();
        private IDictionary<byte, IList<Packet>> delayedOrdered = new Dictionary<byte, IList<Packet>>();

        private int inboundPacketIndex = 0;
        private List<InboundPacket> inboundPackets = new List<InboundPacket>();
        private List<OutboundPacket> outboundPackets = new List<OutboundPacket>();

        internal LocalNetworkGamer(NetworkMachine machine, SignedInGamer signedInGamer, byte id, bool isPrivateSlot) : base(machine, signedInGamer.DisplayName, signedInGamer.Gamertag, id, isPrivateSlot, false)
        {
            this.SignedInGamer = signedInGamer;
        }
        
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

                if (ready != value)
                {
                    ready = value;

                    Session.InternalMessages.GamerStateChanged.Create(this, false, true, null);
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
                Session.PacketPool.Recycle(inboundPackets[i].packet);
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
                Session.PacketPool.Recycle(outboundPacket.packet);
            }

            outboundPackets.Clear();
        }

        internal void AddInboundPacket(Packet packet, byte senderId, SendDataOptions options)
        {
            NetworkGamer sender = Session.FindGamerById(senderId);

            if (options == SendDataOptions.InOrder || options == SendDataOptions.ReliableInOrder)
            {
                bool packetsInQueue = delayedOrdered.ContainsKey(senderId) && delayedOrdered[senderId].Count > 0;

                if (sender == null || packetsInQueue)
                {
                    if (!delayedOrdered.ContainsKey(senderId))
                    {
                        delayedOrdered.Add(senderId, new List<Packet>());
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
                    delayedUnordered[senderId].Add(packet);
                }
                else
                {
                    inboundPackets.Add(new InboundPacket(packet, sender));
                }
            }
        }

        internal void TryAddDelayedInboundPackets()
        {
            // Unordered
            foreach (var pair in delayedUnordered)
            {
                byte senderId = pair.Key;
                IList<Packet> delayedPackets = pair.Value;
                NetworkGamer sender = Session.FindGamerById(senderId);

                if (sender != null)
                {
                    foreach (Packet delayedPacket in delayedPackets)
                    {
                        inboundPackets.Add(new InboundPacket(delayedPacket, sender));
                    }
                    delayedPackets.Clear();
                }
            }

            // Ordered
            foreach (var pair in delayedOrdered)
            {
                byte senderId = pair.Key;
                IList<Packet> delayedPackets = pair.Value;
                NetworkGamer sender = Session.FindGamerById(senderId);

                if (sender != null)
                {
                    foreach (Packet delayedPacket in delayedPackets)
                    {
                        inboundPackets.Add(new InboundPacket(delayedPacket, sender));
                    }
                    delayedPackets.Clear();
                }
            }
        }

        internal void QueueOutboundPackets()
        {
            foreach (OutboundPacket outboundPacket in outboundPackets)
            {
                Session.InternalMessages.UserMessage.Create(outboundPacket.sender, outboundPacket.recipient, outboundPacket.options, outboundPacket.packet);
            }
        }

        // Receiving data
        public int ReceiveData(byte[] data, out NetworkGamer sender)
        {
            return ReceiveData(data, 0, out sender);
        }

        public int ReceiveData(byte[] data, int offset, out NetworkGamer sender)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            if (offset >= data.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }

            if (inboundPacketIndex >= inboundPackets.Count)
            {
                sender = null;
                return 0;
            }

            // Get one packet from queue
            InboundPacket inboundPacket = inboundPackets[inboundPacketIndex];
            inboundPacketIndex++;

            if (inboundPacket.packet.length > data.Length - offset)
            {
                throw new ArgumentException("data is too small to accommodate the incoming network packet");
            }

            // Write inbound packet data to array
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

            // Get one packet from queue
            InboundPacket inboundPacket = inboundPackets[inboundPacketIndex];
            inboundPacketIndex++;

            // Write inbound packet data to stream
            data.BaseStream.SetLength(0);
            data.BaseStream.Position = 0;
            data.BaseStream.Write(inboundPacket.packet.data, 0, inboundPacket.packet.length);

            // Prepare for reading again
            data.BaseStream.Position = 0;

            sender = inboundPacket.sender;
            return inboundPacket.packet.length;
        }

        // Sending data
        private void InternalSendData(byte[] data, int offset, int count, SendDataOptions options, NetworkGamer recipient)
        {
            if (data == null)
            {
                throw new NullReferenceException("data");
            }
            if (data.Length == 0)
            {
                throw new NetworkException("data empty");
            }
            if (offset < 0 || offset >= data.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (count <= 0 || offset + count > data.Length)
            {
                throw new ArgumentOutOfRangeException("count");
            }
            if (Session == null)
            {
                throw new ObjectDisposedException("NetworkSession");
            }

            Packet packet = Session.PacketPool.GetAndFillWith(data, offset, count);
            outboundPackets.Add(new OutboundPacket(packet, this, recipient, options));
        }

        public void SendData(byte[] data, SendDataOptions options)
        {
            try { InternalSendData(data, 0, data.Length, options, null); }
            catch { throw; }
        }

        public void SendData(byte[] data, SendDataOptions options, NetworkGamer recipient)
        {
            if (recipient == null)
            {
                throw new NullReferenceException("recipient");
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
                throw new NullReferenceException("recipient");
            }

            try { InternalSendData(data, offset, count, options, recipient); }
            catch { throw; }
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
            Packet packet = Session.PacketPool.Get(data.Length);
            data.BaseStream.Position = 0;
            data.BaseStream.Read(packet.data, 0, packet.length);

            // Prepare for writing again
            data.BaseStream.SetLength(0);
            data.BaseStream.Position = 0;

            outboundPackets.Add(new OutboundPacket(packet, this, recipient, options));
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
                throw new NullReferenceException("recipient");
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