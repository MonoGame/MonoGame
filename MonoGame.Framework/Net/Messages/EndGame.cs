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

            // TODO: How to handle ready flag?
            foreach (NetworkGamer gamer in NetworkSession.Session.AllGamers)
            {
                gamer.SetReadyState(false);
            }
            
            NetworkSession.Session.SessionState = NetworkSessionState.Lobby;
            NetworkSession.Session.InvokeGameStartedEvent(new GameStartedEventArgs());
        }
    }
}