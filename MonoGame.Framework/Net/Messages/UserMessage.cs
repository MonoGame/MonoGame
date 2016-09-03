using Lidgren.Network;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
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

        public InternalMessageType MessageType { get { return InternalMessageType.UserMessage; } }
        public int SequenceChannel { get { return 0; } }
        public SendDataOptions Options { get { return options; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsFullyConnected)
            {
                throw new NetworkException("UserMessage from not fully connected peer");
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
            if (!currentMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                return;
            }
            if (!senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                return;
            }

            byte senderId = input.ReadByte();
            bool sendToAll = input.ReadBoolean();
            byte recipientId = input.ReadByte();
            int length = input.ReadInt32();
            Packet packet = currentMachine.Session.packetPool.GetPacket(length);
            input.ReadBytes(packet.data, 0, length);

            NetworkGamer sender = currentMachine.Session.FindGamerById(senderId);

            if (sender == null)
            {
                // TODO: Check previous gamers in case the remove message came before this, otherwise:
                // TODO: SuspiciousInvalidGamerId
                return;
            }
            if (sender.Machine != senderMachine)
            {
                Debug.WriteLine("Warning: User message sender does not belong to the sender machine!");
                // TODO: SuspiciousInvalidGamerId
                return;
            }

            if (sendToAll)
            {
                foreach (LocalNetworkGamer localGamer in currentMachine.Session.LocalGamers)
                {
                    localGamer.InboundPackets.Add(new InboundPacket(packet, sender));
                }
            }
            else
            {
                NetworkGamer recipient = currentMachine.Session.FindGamerById(recipientId);

                if (recipient == null)
                {
                    // TODO: Check previous gamers in case the remove message came before this, otherwise:
                    // TODO: SuspiciousInvalidGamerId
                    return;
                }
                if (!recipient.IsLocal)
                {
                    Debug.WriteLine("Warning: User message sent to the wrong peer!");
                    // TODO: SuspiciousInvalidGamerId
                    return;
                }

                LocalNetworkGamer localGamer = recipient as LocalNetworkGamer;
                localGamer.InboundPackets.Add(new InboundPacket(packet, sender));
            }
        }
    }
}
