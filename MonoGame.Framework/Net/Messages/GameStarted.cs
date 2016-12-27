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

            OutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageIndex.GameStarted);
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

            // Reset state after exiting lobby
            CurrentMachine.Session.SessionState = NetworkSessionState.Playing;
            CurrentMachine.Session.InvokeGameStartedEvent(new GameStartedEventArgs());
        }
    }
}