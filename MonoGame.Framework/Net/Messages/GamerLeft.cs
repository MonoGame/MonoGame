using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GamerLeftSender : IInternalMessage
    {
        public IBackend Backend { get; set; }
        public IMessageQueue Queue { get; set; }
        public NetworkMachine CurrentMachine { get; set; }

        public void Create(LocalNetworkGamer localGamer, NetworkMachine recipient)
        {
            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageType.GamerLeft);

            msg.Write(localGamer.Id);

            Queue.Place(msg);
        }

        public void Receive(IIncomingMessage input, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            byte id = input.ReadByte();
            NetworkGamer gamer = CurrentMachine.Session.FindGamerById(id);

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

            CurrentMachine.Session.RemoveGamer(gamer);
        }
    }
}
