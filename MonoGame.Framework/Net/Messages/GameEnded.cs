using Microsoft.Xna.Framework.Net.Backend;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal class GameEndedSender : IInternalMessage
    {
        public IBackend Backend { get; set; }
        public IMessageQueue Queue { get; set; }
        public NetworkMachine CurrentMachine { get; set; }

        public void Create(NetworkMachine recipient)
        {
            if (!CurrentMachine.IsHost)
            {
                throw new NetworkException("Only host can send EndGame");
            }

            IOutgoingMessage msg = Backend.GetMessage(recipient?.peer, SendDataOptions.ReliableInOrder, 1);
            msg.Write((byte)InternalMessageType.GameEnded);
            Queue.Place(msg);
        }

        public void Receive(IIncomingMessage input, NetworkMachine senderMachine)
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

                CurrentMachine.Session.internalMessages.GamerStateChanged.Create(localGamer, false, true, null);
            }

            // Reset state before going into lobby
            CurrentMachine.Session.SessionState = NetworkSessionState.Lobby;
            CurrentMachine.Session.InvokeGameEndedEvent(new GameEndedEventArgs());
        }
    }
}