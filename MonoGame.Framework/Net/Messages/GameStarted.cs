using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GameStarted : InternalMessage
    {
        public void Create(NetworkMachine recipient)
        {
            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send StartGame");
            }

            // TODO: Send all known gamer ids and their ready state to all peers here so that they can verify that
            // they know about all of them and the ready state matches. The peers should wait a few seconds before
            // answering so that any late gamer state changes are not missed. If enough peers suggests that a gamer
            // is a cheater the gamer in question and its NetworkMachine should be removed from the game.
            //
            // OR
            //
            // Let host handle all gamer joining and leaving messages, probably better if doable!

            var msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.GameStarted);
            Queue.Place(msg);
        }

        public override void Receive(BaseIncomingMessage msg, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            // Reset state after exiting lobby
            CurrentMachine.Session.SessionState = NetworkSessionState.Playing;
            CurrentMachine.Session.InvokeGameStartedEvent(new GameStartedEventArgs());
        }
    }
}
