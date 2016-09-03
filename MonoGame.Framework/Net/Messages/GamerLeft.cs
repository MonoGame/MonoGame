using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GamerLeftSender : IInternalMessageSender
    {
        private LocalNetworkGamer localGamer;

        public GamerLeftSender(LocalNetworkGamer localGamer)
        {
            this.localGamer = localGamer;
        }

        public InternalMessageType MessageType { get { return InternalMessageType.GamerLeft; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            output.Write(localGamer.Id);
        }
    }

    internal struct GamerLeftReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }
            if (!senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                return;
            }

            byte id = input.ReadByte();
            NetworkGamer remoteGamer = currentMachine.Session.FindGamerById(id);

            if (remoteGamer.Machine != senderMachine)
            {
                // TODO: SuspiciousUnexpectedMessage
                return;
            }

            currentMachine.Session.RemoveGamer(remoteGamer);
        }
    }
}
