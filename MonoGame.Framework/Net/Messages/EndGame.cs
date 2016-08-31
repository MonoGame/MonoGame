using Lidgren.Network;
using System;

namespace Microsoft.Xna.Framework.Net.Messages
{
    internal struct EndGameMessageSender : IInternalMessageSender
    {
        public InternalMessageType MessageType { get { return InternalMessageType.EndGame; } }
        public int SequenceChannel { get { return 1; } }
        public SendDataOptions Options { get { return SendDataOptions.ReliableInOrder; } }

        public void Send(NetBuffer output, NetworkMachine currentMachine)
        {
            if (!currentMachine.IsHost)
            {
                throw new NetworkException("Only host can send EndGame");
            }
        }
    }

    internal struct EndGameMessageReceiver : IInternalMessageReceiver
    {
        public void Receive(NetBuffer input, NetworkMachine currentMachine, NetworkMachine senderMachine)
        {
            if (!senderMachine.IsHost)
            {
                return;
            }

            // Make sure that the host can not accidentaly start the game too early
            if (currentMachine.IsHost)
            {
                foreach (NetworkGamer gamer in NetworkSession.Session.AllGamers)
                {
                    // Safe because any ready state change from a remote gamer will happen after the scope of this Receive() call
                    gamer.SetReadyState(false);
                }
            }

            // Tell everyone that our local gamers are not yet ready
            foreach (LocalNetworkGamer localGamer in currentMachine.LocalGamers)
            {
                localGamer.IsReady = false;
            }
            
            NetworkSession.Session.SessionState = NetworkSessionState.Lobby;
            NetworkSession.Session.InvokeGameStartedEvent(new GameStartedEventArgs());
        }
    }
}