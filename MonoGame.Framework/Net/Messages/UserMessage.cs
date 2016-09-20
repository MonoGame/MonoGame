using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class UserMessageSender : IInternalMessage
    {
        public IBackend Backend { get; set; }
        public IMessageQueue Queue { get; set; }
        public NetworkMachine CurrentMachine { get; set; }

        public void Create(NetworkGamer sender, NetworkGamer recipient, SendDataOptions options, Packet packet)
        {
            if (!CurrentMachine.IsFullyConnected)
            {
                throw new NetworkException("UserMessage from not fully connected peer");
            }
            
            IOutgoingMessage msg = Backend.GetMessage(recipient?.Machine.peer, options, 0);
            msg.Write((byte)InternalMessageType.UserMessage);

            bool sendToAll = recipient == null;

            msg.Write(sender.Id);
            msg.Write(sendToAll);
            msg.Write((byte)(sendToAll ? 255 : recipient.Id));
            msg.Write((byte)options);
            msg.Write(packet.length);
            msg.Write(packet.data);

            Queue.Place(msg);
        }

        public void Receive(IIncomingMessage input, NetworkMachine senderMachine)
        {
            if (!CurrentMachine.IsFullyConnected || !senderMachine.IsFullyConnected)
            {
                // Will occur naturally for non fully connected machines if a machine sends to everyone
                return;
            }

            byte senderId = input.ReadByte();
            bool sendToAll = input.ReadBoolean();
            byte recipientId = input.ReadByte();
            SendDataOptions options = (SendDataOptions)input.ReadByte();
            int length = input.ReadInt();
            Packet packet = CurrentMachine.Session.packetPool.GetPacket(length);
            input.ReadBytes(packet.data, 0, length);

            NetworkGamer sender = CurrentMachine.Session.FindGamerById(senderId);

            if (sender != null && sender.Machine != senderMachine)
            {
                // TODO: SuspiciousInvalidGamerId
                Debug.Assert(false);
                return;
            }

            if (sendToAll)
            {
                foreach (LocalNetworkGamer localGamer in CurrentMachine.Session.LocalGamers)
                {
                    localGamer.AddInboundPacket(packet, senderId, options);
                }
            }
            else
            {
                NetworkGamer recipient = CurrentMachine.Session.FindGamerById(recipientId);

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
