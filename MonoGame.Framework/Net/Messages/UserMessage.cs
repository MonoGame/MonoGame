using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class UserMessage : InternalMessage
    {
        public void Create(NetworkGamer sender, NetworkGamer recipient, SendDataOptions options, Packet packet)
        {
            if (!CurrentMachine.IsFullyConnected)
            {
                throw new NetworkException("UserMessage from not fully connected peer");
            }

            BaseOutgoingMessage msg = Backend.GetMessage(recipient?.Machine.peer, options, 0);
            msg.Write((byte)InternalMessageIndex.UserMessage);

            bool sendToAll = recipient == null;

            msg.Write(sender.Id);
            msg.Write(sendToAll);
            msg.Write((byte)(sendToAll ? 255 : recipient.Id));
            msg.Write((byte)options);
            msg.Write(packet.length);
            msg.Write(packet.data);

            Queue.Place(msg);
        }

        public override void Receive(BaseIncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!CurrentMachine.IsFullyConnected || !senderMachine.IsFullyConnected)
            {
                // Will occur naturally for non fully connected machines if a machine sends to everyone
                return;
            }

            byte senderId = msg.ReadByte();
            bool sendToAll = msg.ReadBoolean();
            byte recipientId = msg.ReadByte();
            SendDataOptions options = (SendDataOptions)msg.ReadByte();
            int length = msg.ReadInt();
            Packet packet = CurrentMachine.Session.PacketPool.Get(length);
            msg.ReadBytes(packet.data, 0, length);

            NetworkGamer sender = CurrentMachine.Session.FindGamerById(senderId);

            if (sender != null && sender.Machine != senderMachine)
            {
                // TODO: SuspiciousInvalidGamerId
                Debug.Assert(false);
                return;
            }

            if (sendToAll)
            {
                bool firstGamer = true;

                foreach (LocalNetworkGamer localGamer in CurrentMachine.localGamers)
                {
                    if (firstGamer)
                    {
                        localGamer.AddInboundPacket(packet, senderId, options);
                    }
                    else
                    {
                        localGamer.AddInboundPacket(CurrentMachine.Session.PacketPool.GetAndFillWith(packet.data), senderId, options);
                    }

                    firstGamer = false;
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
