using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GamerLeft : InternalMessage
    {
        public void Create(LocalNetworkGamer localGamer, NetworkMachine recipient)
        {
            var msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.GamerLeft);

            msg.Write(localGamer.Id);

            Queue.Place(msg);
        }

        public override void Receive(BaseIncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            byte id = msg.ReadByte();
            var gamer = CurrentMachine.Session.FindGamerById(id);

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
