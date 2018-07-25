using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GameEnded : InternalMessage
    {
        public void Create(NetworkMachine recipient)
        {
            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send EndGame");
            }

            // Queue reset ready first
            CurrentMachine.Session.InternalMessages.ResetReady.Create();

            OutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.GameEnded);
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
            
            // Assert: Reset ready message must have happened before this call as it was sent just before
            if (CurrentMachine.IsHost)
            {
                foreach (NetworkGamer gamer in CurrentMachine.Session.allGamers)
                {
                    if (gamer.IsReady)
                    {
                        throw new NetworkException("A gamer is ready even though we are about to end game and go into the lobby state");
                    }
                }
            }

            // Reset state before going into lobby
            CurrentMachine.Session.SessionState = NetworkSessionState.Lobby;
            CurrentMachine.Session.InvokeGameEndedEvent(new GameEndedEventArgs());
        }
    }
}
