using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Net.Backend;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class ConnectionAcknowledged : InternalMessage
    {
        public void Create(NetworkMachine recipient)
        {
            if (recipient == null)
            {
                throw new ArgumentNullException("recipient");
            }

            OutgoingMessage msg = Backend.GetMessage(recipient.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.ConnectionAcknowledged);

            // Encode validation info
            msg.Write((int)CurrentMachine.LocalGamers.Count);

            // Send a priori state
            if (CurrentMachine.IsHost)
            {
                CurrentMachine.Session.InternalMessages.SessionStateChanged.Create(recipient);
            }

            foreach (LocalNetworkGamer localGamer in CurrentMachine.LocalGamers)
            {
                CurrentMachine.Session.InternalMessages.GamerJoined.Create(localGamer, recipient);
            }

            // Make sure to send acknowledged message after a priori messages (above)
            Queue.Place(msg);
        }

        public override void Receive(IncomingMessage msg, NetworkMachine senderMachine)
        {
            if (senderMachine.IsLocal)
            {
                return;
            }
            if (senderMachine.HasAcknowledgedLocalMachine)
            {
                // TODO: SuspiciousRepeatedInfo
                Debug.Assert(false);
                return;
            }

            // Decode validation info
            int gamerCount = msg.ReadInt();

            if (gamerCount > 0 && !senderMachine.IsFullyConnected)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            // Make sure we have received all a priori state
            if (senderMachine.IsHost && !senderMachine.HasSentSessionStateToLocalMachine)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            if (senderMachine.Gamers.Count != gamerCount)
            {
                // TODO: SuspiciousUnexpectedMessage
                Debug.Assert(false);
                return;
            }

            // Everything went fine
            senderMachine.HasAcknowledgedLocalMachine = true;
        }
    }
}
