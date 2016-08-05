using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework.GamerServices;

namespace Microsoft.Xna.Framework.Net
{
    internal struct PacketSenderPair
    {
        public Packet packet;
        public NetworkGamer sender;

        public PacketSenderPair(Packet packet, NetworkGamer sender)
        {
            this.packet = packet;
            this.sender = sender;
        }
    }

    public sealed class LocalNetworkGamer : NetworkGamer
    {
        internal IList<PacketSenderPair> inboundPackets = new List<PacketSenderPair>();

        internal LocalNetworkGamer(byte id, bool isGuest, bool isHost, bool isPrivateSlot, NetworkSession session, SignedInGamer signedInGamer) : base(signedInGamer.DisplayName, signedInGamer.Gamertag, id, isGuest, isHost, true, isPrivateSlot, session.machine, session)
        {
            this.SignedInGamer = signedInGamer;
        }
        
        public bool IsDataAvailable { get { return inboundPackets.Count > 0; } }
        public SignedInGamer SignedInGamer { get; }

        public void EnableSendVoice(NetworkGamer remoteGamer, bool enable)
        {
            throw new NotImplementedException();
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
            if (inboundPackets.Count == 0)
            {
                sender = null;
                return 0;
            }

            // Pop packet from stack
            int lastIndex = inboundPackets.Count - 1;
            PacketSenderPair pair = inboundPackets[lastIndex];
            inboundPackets.RemoveAt(lastIndex);

            // Write to output packet writer
            int dataLength = pair.packet.length;

            data.BaseStream.SetLength(0);
            data.BaseStream.Write(pair.packet.data, 0, dataLength);

            Session.packetPool.RecyclePacket(pair.packet);

            sender = pair.sender;
            return dataLength;
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

        public void SendData(PacketWriter data, SendDataOptions options)
        {
            if (data == null)
            {
                throw new NullReferenceException("data");
            }
            if (Session == null)
            {
                throw new ObjectDisposedException("NetworkSession");
            }
            
        }

        public void SendData(PacketWriter data, SendDataOptions options, NetworkGamer recipient)
        {
            throw new NotImplementedException();
        }

        public void SendPartyInvites()
        { }
    }
}