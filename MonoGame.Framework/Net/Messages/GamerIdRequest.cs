using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GamerIdRequest : InternalMessage
    {
        public void Create(NetworkMachine recipient)
        {
            OutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.GamerIdRequest);
            Queue.Place(msg);
        }

        public override void Receive(IncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!CurrentMachine.IsHost || !senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            CurrentMachine.Session.InternalMessages.GamerIdResponse.Create(senderMachine);
        }
    }
}
