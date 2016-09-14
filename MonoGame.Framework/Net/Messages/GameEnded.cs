using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct GameEndedSender : IInternalMessageContent
    {
        public InternalMessageType MessageType { get { return InternalMessageType.GameEnded; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Write(IOutgoingMessage output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsHost)
            {
                throw new NetworkException("Only host can send EndGame");
            }
        }
    }

    internal class GameEndedReceiver : IInternalMessageReceiver
    {
        public void Receive(IIncomingMessage input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                // TODO: SuspiciousHostClaim
                Debug.Assert(false);
                return;
            }

            // Make sure that the host can not accidentaly start the game too early
            if (currentMachine.IsHost)
            {
                foreach (NetworkGamer gamer in currentMachine.Session.AllGamers)
                {
                    // Safe because any ready state change from a remote gamer will happen after the scope of this Receive() call
                    gamer.SetReadyState(false);
                }
            }

            // Tell everyone that our local gamers are not yet ready
            foreach (LocalNetworkGamer localGamer in currentMachine.LocalGamers)
            {
                localGamer.SetReadyState(false);

                currentMachine.Session.QueueMessage(new GamerStateChangedSender(localGamer, false, true));
            }

            // Reset state before going into lobby
            currentMachine.Session.SessionState = NetworkSessionState.Lobby;
            currentMachine.Session.InvokeGameEndedEvent(new GameEndedEventArgs());
        }
    }
}