using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class ResetReady : InternalMessage
    {
        public void Create()
        {
            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send ResetReady");
            }

            OutgoingMessage msg = Backend.GetMessage(null, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.ResetReady);
            Queue.Place(msg);
        }

        public override void Receive(IncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            // Make sure that the host can not accidentaly start the game too early
            if (CurrentMachine.IsHost)
            {
                foreach (NetworkGamer gamer in CurrentMachine.Session.AllGamers)
                {
                    // Safe because any ready state change from a remote gamer will happen after the scope of this Receive() call
                    gamer.SetReadyState(false);
                }
            }

            // Tell everyone that our local gamers are not yet ready
            foreach (LocalNetworkGamer localGamer in CurrentMachine.LocalGamers)
            {
                localGamer.SetReadyState(false);

                CurrentMachine.Session.InternalMessages.GamerStateChanged.Create(localGamer, false, true, null);
            }
        }
    }
}