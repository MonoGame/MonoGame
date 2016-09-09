using Lidgren.Network;
using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerLeftSender : IInternalMessageContent
    {
        private LocalNetworkGamer localGamer;

        public GamerLeftSender(LocalNetworkGamer localGamer)
        {
            this.localGamer = localGamer;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.GamerLeft; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Write(NetBuffer output, NetworkMachine currentMachine)
        {
            output.Write(localGamer.Id);
        }
    }

    internal class GamerLeftReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            byte id = input.ReadByte();
            NetworkGamer gamer = currentMachine.Session.FindGamerById(id);

            if (gamer == null)
            {
                // TODO: SuspiciousInvalidGamerId
                Debug.Assert(false);
                return;
            }
            if (gamer.Machine != senderMachine)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            currentMachine.Session.RemoveGamer(gamer);
        }
    }
}
