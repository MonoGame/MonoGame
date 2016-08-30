using Lidgren.Network;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Message
{
    internal struct UserMessageSender : IInternalMessageSender
    {
        private NetworkGamer sender;
        private NetworkGamer recipient;
        private SendDataOptions options;
        private Packet packet;

        public UserMessageSender(NetworkGamer sender, NetworkGamer recipient, SendDataOptions options, Packet packet)
        {
            this.sender = sender;
            this.recipient = recipient;
            this.options = options;
            this.packet = packet;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.User; } }
        public int SequenceChannel { get { return 0; } }
        public SendDataOptions Options { get { return options; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            if (currentMachine.IsPending)
            {
                throw new NetworkException("User message from pending machine");
            }

            bool sendToAll = recipient == null;

            output.Write(sender.Id);
            output.Write(sendToAll);
            output.Write((byte)(sendToAll ? 255 : recipient.Id));
            output.Write(packet.length);
            output.Write(packet.data);
        }
    }

    internal struct UserMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (currentMachine.IsPending || senderMachine.IsPending)
            {
                return;
            }

            byte senderId = input.ReadByte();
            bool sendToAll = input.ReadBoolean();
            byte recipientId = input.ReadByte();
            int length = input.ReadInt32();
            Packet packet = NetworkSession.Session.packetPool.GetPacket(length);
            input.ReadBytes(packet.data, 0, length);

            NetworkGamer sender = NetworkSession.Session.FindGamerById(senderId);

            if (sendToAll)
            {
                foreach (LocalNetworkGamer localGamer in NetworkSession.Session.LocalGamers)
                {
                    localGamer.InboundPackets.Add(new InboundPacket(packet, sender));
                }
            }
            else
            {
                LocalNetworkGamer localGamer = NetworkSession.Session.FindGamerById(recipientId) as LocalNetworkGamer;

                if (localGamer == null)
                {
                    Debug.WriteLine("Warning: User message sent to the wrong peer!");
                    return;
                }

                localGamer.InboundPackets.Add(new InboundPacket(packet, sender));
            }
        }
    }
}
