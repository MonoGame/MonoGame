using Lidgren.Network;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct UserMessageSender : IInternalMessageContent
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
            output.Write((byte)options);
            output.Write(packet.length);
            output.Write(packet.data);
        }
    }

    internal struct UserMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!currentMachine.IsFullyConnected || !senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            byte senderId = input.ReadByte();
            bool sendToAll = input.ReadBoolean();
            byte recipientId = input.ReadByte();
            SendDataOptions options = (SendDataOptions)input.ReadByte();
            int length = input.ReadInt32();
            Packet packet = currentMachine.Session.packetPool.GetPacket(length);
            input.ReadBytes(packet.data, 0, length);

            NetworkGamer sender = currentMachine.Session.FindGamerById(senderId);

            if (sender != null && sender.Machine != senderMachine)
            {
                // TODO: SuspiciousInvalidGamerId
                Debug.Assert(false);
                return;
            }

            if (sendToAll)
            {
                foreach (LocalNetworkGamer localGamer in currentMachine.Session.LocalGamers)
                {
                    localGamer.AddInboundPacket(packet, senderId, options);
                }
            }
            else
            {
                NetworkGamer recipient = currentMachine.Session.FindGamerById(recipientId);

                if (recipient == null)
                {
                    // TODO: Check previous gamers in case the remove message came before this, otherwise:
                    // SuspiciousInvalidGamerId
                    return;
                }
                if (!recipient.IsLocal)
                {
                    // TODO: SuspiciousInvalidGamerId
                    Debug.Assert(false);
                    return;
                }

                LocalNetworkGamer localGamer = recipient as LocalNetworkGamer;
                localGamer.AddInboundPacket(packet, senderId, options);
            }
        }
    }
}
